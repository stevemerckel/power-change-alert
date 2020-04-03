using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using PowerChangeAlerter.Common;
using System.Threading;
using System.Management;

namespace PowerChangeAlerter
{
    public partial class AlerterService : ServiceBase
    {
        private readonly IAppLogger _logger;
        private readonly IRuntimeSettings _rs;
        private readonly IFileManager _fm;
        private readonly Stopwatch _stopWatch = new Stopwatch();
        private DateTime _now;
        private int _uptimeMinutesCount = 0;
        private readonly int _uptimeDelayInMinutes;
        private readonly object _lock = new object();
        private readonly Timer _uptimeTimer;
        private bool _isFirstUptimeLogged = false;
        private PowerModes _currentPowerMode = PowerModes.Resume;

        public AlerterService(IRuntimeSettings runtimeSettings, IAppLogger logger, IFileManager fileManager)
        {
            InitializeComponent();
            _rs = runtimeSettings;
            _logger = logger;
            _fm = fileManager;
            _now = DateTime.Now;
            _stopWatch.Start();

            // initialize uptime tracker
            _uptimeDelayInMinutes = runtimeSettings.IsLocalDebugging ? 1 : 20;
            _uptimeTimer = new Timer(LogUptime);
            _uptimeTimer.Change(0, (int)TimeSpan.FromMinutes(_uptimeDelayInMinutes).TotalMilliseconds);

            // report battery presence
            if (IsBatteryAvailable())
                _logger.Info("Battery was found");
            else
                _logger.Warn("No battery was found, so changes in power state will not be reported");
        }

        private void LogUptime(object state)
        {
            if (!_isFirstUptimeLogged)
            {
                _logger.Info($"{nameof(LogUptime)} initialized!");
                _isFirstUptimeLogged = true;
                DumpBatteryInfo();
                return;
            }

            _uptimeMinutesCount += _uptimeDelayInMinutes;
            _logger.Info($"{nameof(LogUptime)} running for {_uptimeMinutesCount} minutes");
            DumpBatteryInfo();
        }

        private bool IsBatteryAvailable()
        {
            ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_Battery");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection collection = searcher.Get();
            return collection.Count > 0;
        }

        private void DumpBatteryInfo()
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

        private void HandleTimeChanged(object sender, EventArgs e)
        {
            _stopWatch.Stop();
            DateTime shouldBe;
            lock (_lock)
            {
                shouldBe = _now.Add(_stopWatch.Elapsed);
                _now = DateTime.Now;
                _stopWatch.Reset();
                _stopWatch.Start();
            }

            const int ThresholdInSeconds = 5;
            int deltaSeconds = Math.Abs((int)shouldBe.Subtract(DateTime.Now).TotalSeconds);
            if (deltaSeconds < ThresholdInSeconds)
            {
                _logger.Info($"An insignificant time change was detected (less than {ThresholdInSeconds} seconds)");
                return;
            }

            var msg = $"{nameof(HandleTimeChanged)} from {shouldBe} to {DateTime.Now:MM/dd/yyyy hh:mm:ss tt}";
            _logger.Warn(msg);
        }

        private void HandleUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            _logger.Info($"{nameof(HandleUserPreferenceChanged)} triggered: {e.Category}");
        }

        private void HandleDisplaySettingsChanged(object sender, EventArgs e)
        {
            _logger.Info($"{nameof(HandleDisplaySettingsChanged)} triggered");
        }

        private void HandlePowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (!IsBatteryAvailable())
                _logger.Warn($"{nameof(HandlePowerModeChanged)} was triggered but there is no connected battery... wtf??");

            _logger.Info($"{nameof(HandlePowerModeChanged)} triggered: {e.Mode}");
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    // online state resumed
                    break;
                case PowerModes.StatusChange:
                    // status changed from AC to battery or vice-versa
                    break;
                case PowerModes.Suspend:
                    // going into suspended power mode
                    break;
            }

            _currentPowerMode = e.Mode;
        }

        public void StartService()
        {
            // bind to system events
            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(HandlePowerModeChanged); //HandlePowerModeChanged;
            SystemEvents.DisplaySettingsChanged += HandleDisplaySettingsChanged;
            SystemEvents.UserPreferenceChanged += HandleUserPreferenceChanged;
            SystemEvents.TimeChanged += HandleTimeChanged;

            // write runtime startup metrics
            var currentVersion = _fm.GetCurrentAssemblyVersion();
            var sb = new StringBuilder();
            sb.AppendLine($"Starting up {nameof(AlerterService)} (version {currentVersion}) with the following settings:");
            sb.AppendLine($"\t{nameof(RuntimeSettings)} is {(_rs == null ? "null" : "initialized")}");
            sb.AppendLine($"\t- {nameof(_rs.IsLocalDebugging)} = {_rs.IsLocalDebugging}");
            sb.AppendLine($"\t- {nameof(_rs.EmailSmtpServer)} = {_rs.EmailSmtpServer}");
            sb.AppendLine($"\t- {nameof(_rs.EmailSmtpPort)} = {_rs.EmailSmtpPort}");
            sb.AppendLine($"\t- {nameof(_rs.EmailSmtpUseSsl)} = {_rs.EmailSmtpUseSsl}");
            sb.AppendLine($"\t- {nameof(_rs.EmailSenderAddress)} = {_rs.EmailSenderAddress}");
            sb.AppendLine($"\t- {nameof(_rs.EmailRecipientAddress)} = {_rs.EmailRecipientAddress}");
            _logger.Info(sb.ToString());

            // todo: finish implementation
        }

        public void StopService()
        {
            // remove bindings to system events
            SystemEvents.PowerModeChanged -= HandlePowerModeChanged;
            SystemEvents.DisplaySettingsChanged -= HandleDisplaySettingsChanged;
            SystemEvents.UserPreferenceChanged -= HandleUserPreferenceChanged;
            SystemEvents.TimeChanged -= HandleTimeChanged;

            // todo: anything left to do?
        }

        public void PauseService()
        {
            OnPause();
        }

        public void Continue()
        {
            OnContinue();
        }

        protected override void OnStart(string[] args)
        {
            StartService();
        }

        protected override void OnStop()
        {
            StopService();
        }

        protected override void OnPause()
        {
            _logger.Warn($"Entered {nameof(OnPause)}");
            base.OnPause();
            _logger.Warn($"Exiting {nameof(OnPause)}");
        }

        protected override void OnContinue()
        {
            _logger.Warn($"Entered {nameof(OnContinue)}");
            base.OnContinue();
            _logger.Warn($"Exiting {nameof(OnContinue)}");
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            _logger.Warn($"Entered {nameof(OnPowerEvent)} -- {nameof(PowerBroadcastStatus)}={powerStatus}");
            return base.OnPowerEvent(powerStatus);
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            _logger.Warn($"Entered {nameof(OnSessionChange)}({changeDescription})");
            base.OnSessionChange(changeDescription);
        }

        protected override void OnShutdown()
        {
            _logger.Warn($"Entered {nameof(OnShutdown)}");
            base.OnShutdown();
        }
    }
}