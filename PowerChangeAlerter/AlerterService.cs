﻿using Microsoft.Win32;
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
        private readonly IAlertManager _alertManager;
        private readonly object _lock = new object();
        private readonly Stopwatch _stopWatch = new Stopwatch();
        private DateTime _now;
        private PowerModes _currentPowerMode = PowerModes.Resume;

        public AlerterService(IRuntimeSettings runtimeSettings, IAppLogger logger, IFileManager fileManager)
        {
            InitializeComponent();
            _logger = logger;
            _alertManager = new AlertManager(runtimeSettings, _logger, fileManager);
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
                Debug.WriteLine($"An insignificant time change was detected (less than {ThresholdInSeconds} seconds)");
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
            // bind to system events
            SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(HandlePowerModeChanged);
            SystemEvents.DisplaySettingsChanged += HandleDisplaySettingsChanged;
            SystemEvents.UserPreferenceChanged += HandleUserPreferenceChanged;
            SystemEvents.TimeChanged += HandleTimeChanged;
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
            _alertManager.ManagerPause();
            _logger.Warn($"Exiting {nameof(OnPause)}");
        }

        protected override void OnContinue()
        {
            _logger.Warn($"Entered {nameof(OnContinue)}");
            base.OnContinue();
            _alertManager.ManagerContinue();
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