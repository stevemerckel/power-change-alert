using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.ServiceProcess;
using PowerChangeAlerter.Common;

namespace PowerChangeAlerter
{
    public partial class AlerterService : ServiceBase
    {
        private readonly IAppLogger _logger;
        private readonly IAlertManager _alertManager;
        private readonly object _lock = new object();
        private readonly Stopwatch _stopWatch = new Stopwatch();
        private DateTime _now;
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

        private void HandleTimeChanged(object sender, EventArgs e)
        {
            _stopWatch.Stop();
            DateTime previousSystemDateTime;
            lock (_lock)
            {
                previousSystemDateTime = _now.Add(_stopWatch.Elapsed);
                _now = DateTime.Now;
                _stopWatch.Reset();
                _stopWatch.Start();
            }

            const int ThresholdInSeconds = 30;
            int deltaSeconds = Math.Abs((int)previousSystemDateTime.Subtract(DateTime.Now).TotalSeconds);
            if (deltaSeconds < ThresholdInSeconds)
            {
                Debug.WriteLine($"An insignificant time change of {deltaSeconds} seconds was detected -- ignored because under {ThresholdInSeconds} second threshold");
                return;
            }

            var newSystemDateTime = DateTime.Now;
            var msg = $"{nameof(HandleTimeChanged)} from {previousSystemDateTime} to {newSystemDateTime:MM/dd/yyyy hh:mm:ss tt}";
            _alertManager.NotifyTimeChange(previousSystemDateTime, newSystemDateTime);
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

            // bind to system events
            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(HandlePowerModeChanged);
            SystemEvents.DisplaySettingsChanged += HandleDisplaySettingsChanged;
            SystemEvents.UserPreferenceChanged += HandleUserPreferenceChanged;
            SystemEvents.TimeChanged += HandleTimeChanged;

            _now = DateTime.Now;
        }

        public void StopService()
        {
            // remove bindings to system events
            SystemEvents.PowerModeChanged -= HandlePowerModeChanged;
            SystemEvents.DisplaySettingsChanged -= HandleDisplaySettingsChanged;
            SystemEvents.UserPreferenceChanged -= HandleUserPreferenceChanged;
            SystemEvents.TimeChanged -= HandleTimeChanged;

            _alertManager.ManagerStop();
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