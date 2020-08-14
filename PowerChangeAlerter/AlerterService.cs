using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.ServiceProcess;
using PowerChangeAlerter.Common;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace PowerChangeAlerter
{
    public partial class AlerterService : ServiceBase
    {
        private readonly IAppLogger _logger;
        private volatile IAlertManager _alertManager;
        private readonly object _lock = new object();
        private readonly Stopwatch _stopWatch = new Stopwatch();
        private PowerModes _currentPowerMode = PowerModes.Resume;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="runtimeSettings">Runtime settings object</param>
        /// <param name="logger">Logger object</param>
        /// <param name="fileManager">File system manager</param>
        public AlerterService(IRuntimeSettings runtimeSettings, IAppLogger logger, IFileManager fileManager)
        {
            InitializeComponent();
            _logger = logger;
            _alertManager = new AlertManager(runtimeSettings, _logger, fileManager);
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
            _logger.Info($"{nameof(HandlePowerModeChanged)} triggered: {e.Mode}");
            switch (e.Mode)
            {
                case PowerModes.Resume:
                    // online state resumed
                    break;
                case PowerModes.StatusChange:
                    // status changed from AC to battery or vice-versa
                    var pbs = new PowerBroadcastStatus();
                    _logger.Info($"{nameof(HandlePowerModeChanged)} received {e.Mode} -- current {nameof(PowerBroadcastStatus)} is {pbs}");
                    // todo: send notification to alert manager with the proper power notification call
                    break;
                case PowerModes.Suspend:
                    // going into suspended power mode
                    break;
            }

            _currentPowerMode = e.Mode;
        }

        public void StartService()
        {
            _alertManager.ManagerStart();

            // set up hidden form to be used as message pump
            // Initially Based on StackOverflow idea --> https://stackoverflow.com/questions/9725180/c-sharp-event-to-detect-daylight-saving-or-even-manual-time-change
            // Based on article stored by Way Back Machine --> https://web.archive.org/web/20140706130218/http://connect.microsoft.com/VisualStudio/feedback/details/241133/detecting-a-wm-timechange-event-in-a-net-windows-service
            Thread t = new Thread(() => RunMessagePump(_alertManager));
            t.Start();
        }

        private void RunMessagePump(IAlertManager alertManager)
        {
            Application.Run(new HiddenForm(alertManager));
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
        }

        public void TriggerAcToBattery()
        {
            OnPowerEvent(PowerBroadcastStatus.PowerStatusChange);
        }

        public void TriggerStandby()
        {
            OnPowerEvent(PowerBroadcastStatus.Suspend);
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
            _alertManager.ManagerStop();
            _logger.Warn($"Exiting {nameof(OnPause)}");
        }

        protected override void OnContinue()
        {
            _logger.Warn($"Entered {nameof(OnContinue)}");
            base.OnContinue();
            _alertManager.ManagerStart();
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