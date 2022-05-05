using System.Diagnostics;
using System.ServiceProcess;
using PowerChangeAlerter.Common;
using System.Windows.Forms;
using System.Threading;
using System;

namespace PowerChangeAlerter
{
    /// <summary>
    /// Windows service for bootstrapping the monitoring of events
    /// </summary>
    public partial class AlerterService : ServiceBase
    {
        private readonly IAppLogger _logger;
        private readonly IAlertManager _alertManager;
        private readonly object _lock = new object();
        private readonly Stopwatch _stopWatch = new Stopwatch();
        
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="runtimeSettings">Runtime settings object</param>
        /// <param name="logger">Logger object</param>
        /// <param name="fileManager">File system manager</param>
        public AlerterService(IRuntimeSettings runtimeSettings, IAppLogger logger, IFileManager fileManager)
        {
            InitializeComponent();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _alertManager = new AlertManager(runtimeSettings, _logger, fileManager);
        }

        public void StartService()
        {
            _alertManager.ManagerStart();

            // set up hidden form to be used as message pump
            // Initially Based on StackOverflow idea --> https://stackoverflow.com/questions/9725180/c-sharp-event-to-detect-daylight-saving-or-even-manual-time-change
            // Based on article stored by Way Back Machine --> https://web.archive.org/web/20140706130218/http://connect.microsoft.com/VisualStudio/feedback/details/241133/detecting-a-wm-timechange-event-in-a-net-windows-service
            Thread t = new Thread(() => RunMessagePump(_alertManager, _logger));
            t.Start();
        }

        /// <summary>
        /// Invokes the hidden form, see the form's code for the adjustments to the form's behavior and listeners
        /// </summary>
        private void RunMessagePump(IAlertManager alertManager, IAppLogger logger)
        {
            Application.Run(new HiddenForm(alertManager, logger));
        }

        public void StopService()
        {
            _alertManager.ManagerStop();
            Application.Exit();
        }

        public void PauseService()
        {
            OnPause();
        }

        public void Continue()
        {
            OnContinue();
        }

        public void TriggerResume()
        {
            OnPowerEvent(PowerBroadcastStatus.ResumeSuspend);
        }

        public void TriggerBatteryToAc()
        {
            OnPowerEvent(PowerBroadcastStatus.PowerStatusChange);
            _alertManager.NotifyPowerFromWall();
        }

        public void TriggerAcToBattery()
        {
            OnPowerEvent(PowerBroadcastStatus.PowerStatusChange);
            _alertManager.NotifyPowerOnBattery();
        }

        public void TriggerStandby()
        {
            OnPowerEvent(PowerBroadcastStatus.Suspend);
        }

        /// <inheritdoc />
        protected override void OnStart(string[] args)
        {
            StartService();
        }

        /// <inheritdoc />
        protected override void OnStop()
        {
            StopService();
        }

        /// <inheritdoc />
        protected override void OnPause()
        {
            _logger.Warn($"Entered {nameof(OnPause)}");
            base.OnPause();
            _alertManager.ManagerStop();
            _logger.Warn($"Exiting {nameof(OnPause)}");
        }

        /// <inheritdoc />
        protected override void OnContinue()
        {
            _logger.Warn($"Entered {nameof(OnContinue)}");
            base.OnContinue();
            _alertManager.ManagerStart();
            _logger.Warn($"Exiting {nameof(OnContinue)}");
        }

        /// <inheritdoc />
        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            _logger.Warn($"Entered {nameof(OnPowerEvent)} -- {nameof(PowerBroadcastStatus)}={powerStatus}");
            return base.OnPowerEvent(powerStatus);
        }

        /// <inheritdoc />
        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            _logger.Warn($"Entered {nameof(OnSessionChange)}({changeDescription})");
            base.OnSessionChange(changeDescription);
        }

        /// <inheritdoc />
        protected override void OnShutdown()
        {
            _logger.Warn($"Entered {nameof(OnShutdown)}");
            base.OnShutdown();
        }
    }
}