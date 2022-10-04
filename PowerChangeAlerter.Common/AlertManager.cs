using System;
using System.Management;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PowerChangeAlerter.Common
{
    /// <inheritdoc />
    public sealed class AlertManager : IAlertManager
    {
        private volatile IAppLogger _logger;
        private readonly IRuntimeSettings _rs;
        private readonly IFileManager _fm;
        private int _uptimeMinutesCount = 0;
        private int _uptimeDelayInMinutes;
        private readonly object _lock = new object();
        private System.Threading.Timer _uptimeTimer;
        private bool _isFirstUptimeLogged = false;
        private bool _isBatteryDetected;
        private PowerLineStatus _powerSource = PowerLineStatus.Unknown;
        private volatile ISmtpHelper _smtpHelper;

        private Task _taskRunningOnBattery;
        private CancellationTokenSource _ctsBattery;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="runtimeSettings">Runtime settings object</param>
        /// <param name="logger">Application logger object</param>
        /// <param name="fileManager">File system object</param>
        public AlertManager(IRuntimeSettings runtimeSettings, IAppLogger logger, IFileManager fileManager)
        {
            _rs = runtimeSettings;
            _logger = logger;
            _fm = fileManager;
            _smtpHelper = new SmtpHelper(_rs, _logger);
        }

        /// <summary>
        /// Writes uptime messages to logger
        /// </summary>
        /// <param name="state"></param>
        private void LogUptime(object state)
        {
            if (!_isFirstUptimeLogged)
            {
                _logger.Info($"{nameof(LogUptime)} initialized!");
                DumpPowerInfo();
                _isFirstUptimeLogged = true;
                return;
            }

            _uptimeMinutesCount += _uptimeDelayInMinutes;
            _logger.Debug($"{nameof(LogUptime)} running for {_uptimeMinutesCount} minutes");
        }

        /// <summary>
        /// Whether a battery is found on the host device
        /// </summary>
        private bool IsBatteryAvailable()
        {
            ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Battery");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection collection = searcher.Get();
            return collection.Count > 0;
        }

        /// <summary>
        /// Writes a bunch of (possibly) useful power state information
        /// </summary>
        private void DumpPowerInfo()
        {
            PowerBroadcastStatus pbs = new PowerBroadcastStatus();
            _logger.Info($"{nameof(pbs)} = {pbs}");

            // dump WMI information from battery
            _logger.Info("Fetching battery info via WMI");
            ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Battery");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection collection = searcher.Get();
            _logger.Info($"Found {collection.Count} element(s) in WMI collection");
            foreach (ManagementObject mo in collection)
            {
                foreach (PropertyData property in mo.Properties)
                {
                    _logger.Info($"Property {property.Name}: Value is {property.Value}");
                }
            }
        }

        /// <inheritdoc />
        public void ManagerStart()
        {
            _logger.Info($"{nameof(ManagerStart)} was hit");

            // write runtime startup metrics
            var currentVersion = _fm.GetExecutingAssemblyVersion();
            var sb = new StringBuilder();
            sb.AppendLine($"Starting up {nameof(AlertManager)} (version {currentVersion}) with the following settings:");
            sb.AppendLine($"\t{nameof(RuntimeSettings)} is {(_rs == null ? "null" : "initialized")}");
            sb.AppendLine($"\t- {nameof(_rs.IsLocalDebugging)} = {_rs.IsLocalDebugging}");
            sb.AppendLine($"\t- {nameof(_rs.EmailSmtpServer)} = {_rs.EmailSmtpServer}");
            sb.AppendLine($"\t- {nameof(_rs.EmailSmtpPort)} = {_rs.EmailSmtpPort}");
            sb.AppendLine($"\t- {nameof(_rs.EmailSmtpUseSsl)} = {_rs.EmailSmtpUseSsl}");
            sb.AppendLine($"\t- {nameof(_rs.EmailSenderAddress)} = {_rs.EmailSenderAddress}");
            sb.AppendLine($"\t- {nameof(_rs.EmailRecipientAddress)} = {_rs.EmailRecipientAddress}");
            _logger.Info(sb.ToString());

            // initialize uptime tracker
            _uptimeDelayInMinutes = _rs.IsLocalDebugging ? 1 : 20;
            _uptimeTimer = new System.Threading.Timer(LogUptime);
            _uptimeTimer.Change(0, (int)TimeSpan.FromMinutes(_uptimeDelayInMinutes).TotalMilliseconds);

            // report battery presence
            _isBatteryDetected = IsBatteryAvailable();
            if (_isBatteryDetected)
                _logger.Info("Battery was found");
            else
                _logger.Warn("No battery was found, so changes in power state will not be reported");

            // report power draw
            _powerSource = SystemInformation.PowerStatus.PowerLineStatus;
            if (_powerSource == PowerLineStatus.Online)
                _logger.Info($"{nameof(PowerLineStatus)} - Running on wall power");
            else if (_powerSource == PowerLineStatus.Offline)
                _logger.Warn($"{nameof(PowerLineStatus)} - Running on battery");
            else
                _logger.Error($"{nameof(PowerLineStatus)} - Unable to determine power draw source!");

            // if on battery power mode, then drop into notification logic
            if (_powerSource == PowerLineStatus.Offline)
                NotifyPowerOnBattery();
        }

        /// <inheritdoc />
        public void ManagerStop()
        {
            _logger.Info($"{nameof(ManagerStop)} was hit");
        }

        /// <inheritdoc />
        public void NotifyHostShutdown()
        {
            _logger.Warn("System is shutting down");
        }

        /// <inheritdoc />
        public void NotifyPowerFromWall()
        {
            if (_taskRunningOnBattery == null)
            {
                _logger.Warn($"Received call to {nameof(NotifyPowerFromWall)} but the battery task {nameof(_taskRunningOnBattery)} was not running.  Exit early.");
                return;
            }

            _ctsBattery?.Cancel();
            var message = "Changed to wall power";
            _logger.Info(message);
            _smtpHelper.Send("Power on Wall", message);
        }

        /// <inheritdoc />
        public void NotifyPowerOnBattery()
        {
            if (_taskRunningOnBattery?.Status == TaskStatus.Running)
            {
                _logger.Warn($"{nameof(NotifyPowerOnBattery)} was triggered but is apparently still running.  Exiting early.");
                return;
            }

            // send periodic alerts until cancellation
            _ctsBattery = new CancellationTokenSource();
            _taskRunningOnBattery = new Task(() => SendBatteryNotices(_logger, _smtpHelper, _ctsBattery.Token), TaskCreationOptions.LongRunning);
            _taskRunningOnBattery.Start();
        }

        private async void SendBatteryNotices(IAppLogger logger, ISmtpHelper smtp, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            const int DelayBetweenResendingAlertInMinutes = 10;
            var delayBetweenResendingAlertInSeconds = TimeSpan.FromMinutes(DelayBetweenResendingAlertInMinutes).TotalSeconds;
            const int DelayBetweenCheckingTokenInSeconds = 5;
            var message = $"Changed to battery power!  Will send reminder every {DelayBetweenResendingAlertInMinutes} minutes.";
            logger.Info(message);
            smtp.Send("Power on Battery", message);

            var tokenCheckDelayInMilliseconds = (int)TimeSpan.FromSeconds(DelayBetweenCheckingTokenInSeconds).TotalMilliseconds;
            var intervalCountOfDelayUntilResend = (int)(TimeSpan.FromSeconds(delayBetweenResendingAlertInSeconds).TotalMilliseconds / tokenCheckDelayInMilliseconds);
            var resendCount = 0;
            while (true)
            {
                for (var i = 0; i < intervalCountOfDelayUntilResend; i++)
                {
                    if (token.IsCancellationRequested)
                        break;
                    await Task.Delay(tokenCheckDelayInMilliseconds);
                }

                if (token.IsCancellationRequested)
                    break;

                // still on battery, send additional notice
                resendCount++;
                var repeatMessage = $"Still on battery for about {DelayBetweenResendingAlertInMinutes * resendCount} minutes";
                smtp.Send("Power on Battery", repeatMessage);
            }
        }

        /// <inheritdoc />
        public void NotifyTimeChange(DateTime previous, DateTime adjusted)
        {
            var isForward = adjusted >= previous;
            var isDateChanged = previous.Date != adjusted.Date;
            var previousString = isDateChanged ? previous.ToString() : previous.ToLongTimeString();
            var adjustedString = isDateChanged ? adjusted.ToString() : adjusted.ToLongTimeString();

            var message = $"System time moved {(isForward ? "forward" : "backward")} from {previousString} to {adjustedString}";
            _logger.Info(message);
            _logger.Debug($"{nameof(NotifyTimeChange)} - {message}");
            _smtpHelper.Send("Time Change", message);
        }
    }
}