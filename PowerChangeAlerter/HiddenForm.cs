﻿using Microsoft.Win32;
using PowerChangeAlerter.Common;
using System;
using System.Diagnostics;
using System.Management;
using System.ServiceProcess;
using System.Windows.Forms;

namespace PowerChangeAlerter
{
    public partial class HiddenForm : Form
    {
        private readonly IAlertManager _alertManager;
        private readonly IAppLogger _logger;
        private readonly object _lock = new object();
        private readonly Stopwatch _stopWatch = new Stopwatch();
        private DateTime _now;
        private PowerModes _currentPowerMode = PowerModes.Resume;
        private PowerLineStatus _currentPowerLineStatus = PowerLineStatus.Unknown;

        [CodeEntry]
        public HiddenForm(IAlertManager alertManager, IAppLogger logger)
        {
            _alertManager = alertManager ?? throw new ArgumentNullException(nameof(alertManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // initialize time change tracking
            _now = DateTime.Now;
            _stopWatch.Start();

            // fetch and assign current power line status and power status
            _currentPowerLineStatus = GetCurrentPowerLineStatus();

            InitializeComponent();

            // if we are running on battery, kick off alerts to outside listener
            if (_currentPowerLineStatus == PowerLineStatus.Offline)
                _alertManager.NotifyPowerOnBattery();
        }

        /// <summary>
        /// Returns current power line status of system
        /// </summary>
        /// <remarks>
        /// Pointer on which WMI call to make was found here: https://devblogs.microsoft.com/scripting/how-can-i-tell-whether-a-laptop-computer-is-running-off-batteries/
        /// </remarks>
        private PowerLineStatus GetCurrentPowerLineStatus()
        {
            //var wmiRoot = $@"winmgmts:\\.\root\wmi";
            var wmiRoot = $@"\\.\root\wmi";
            ManagementScope scope = new ManagementScope(wmiRoot);
            scope.Connect();
            ObjectQuery query = new ObjectQuery("SELECT * FROM BatteryStatus WHERE Voltage > 0");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
            var queryResults = searcher.Get();
            PowerLineStatus status = PowerLineStatus.Unknown;
            try
            {
                foreach (var mo in queryResults)
                {
                    const string TargetPropertyName = "PowerOnline";
                    var wasFound = bool.TryParse(mo[TargetPropertyName].ToString(), out bool isUsingWallPower);
                    if (!wasFound)
                    {
                        _logger.Warn($"Could not find '{TargetPropertyName}' in WMI query results !!");
                    }
                    else
                    {
                        status = isUsingWallPower ? PowerLineStatus.Online : PowerLineStatus.Offline;
                    }
                }
            }
            catch (ManagementException managementEx)
            {
                // Expected root cause is that this device does not have a battery at all.
                // For now we will assume wall power, and set the return value to that status.
                // Note: Querying "BatteryStatus" throws in this case, but "Win32_Battery" does not.
                // REFACTOR: See if there is a more elegant way to interrogate and fall-through.
                var msg = $"Failed to enumerate query results -- reason: {managementEx}";
                if (managementEx.InnerException != null)
                    msg += $" --- Inner Exception: {managementEx.InnerException}";
                _logger.Warn(msg);
                status = PowerLineStatus.Online;
            }

            return status;
        }

        [CodeEntry]
        private void HiddenForm_Load(object sender, EventArgs e)
        {
            SystemEvents.TimeChanged += HandleTimeChanged;
            SystemEvents.UserPreferenceChanged += HandleUserPreferenceChanged;
            SystemEvents.DisplaySettingsChanged += HandleDisplaySettingsChanged;
            SystemEvents.PowerModeChanged += HandlePowerModeChanged;
        }

        [CodeEntry]
        private void HiddenForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SystemEvents.TimeChanged -= HandleTimeChanged;
            SystemEvents.UserPreferenceChanged -= HandleUserPreferenceChanged;
            SystemEvents.DisplaySettingsChanged -= HandleDisplaySettingsChanged;
            SystemEvents.PowerModeChanged -= HandlePowerModeChanged;
        }

        [CodeEntry]
        private void HandleTimeChanged(object sender, EventArgs e)
        {
            _alertManager.NotifyTimeChange();
        }

        private void HandleUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            _logger.Debug($"{nameof(HandleUserPreferenceChanged)} triggered: {e.Category}");
        }

        private void HandleDisplaySettingsChanged(object sender, EventArgs e)
        {
            _logger.Debug($"{nameof(HandleDisplaySettingsChanged)} triggered");
        }

        [CodeEntry]
        private void HandlePowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            PowerBroadcastStatus powerBroadcastStatus = new PowerBroadcastStatus();
            _logger.Debug($"{nameof(powerBroadcastStatus)} = {powerBroadcastStatus}");
            PowerLineStatus powerLineStatus = SystemInformation.PowerStatus.PowerLineStatus;
            _logger.Debug($"{nameof(powerLineStatus)} = {powerLineStatus}");

            switch (e.Mode)
            {
                case PowerModes.Resume:
                    // online state resumed
                    break;
                case PowerModes.StatusChange:
                    // status changed from AC to battery or vice-versa
                    _logger.Debug($"{nameof(HandlePowerModeChanged)} received {e.Mode} -- current {nameof(PowerBroadcastStatus)} is {powerBroadcastStatus}");
                    switch(powerLineStatus)
                    {
                        case PowerLineStatus.Online:
                            _alertManager.NotifyPowerFromWall();
                            break;
                        case PowerLineStatus.Offline:
                            _alertManager.NotifyPowerOnBattery();
                            break;
                        case PowerLineStatus.Unknown:
                            _logger.Warn($"Unable to process value '{powerBroadcastStatus}'.  Exiting early.");
                            break;
                        default:
                            throw new NotSupportedException($"Unsure how to handle status of '{powerLineStatus}' !!");
                    }
                    break;
                case PowerModes.Suspend:
                    // going into suspended power mode
                    break;
            }

            _currentPowerMode = e.Mode;
            _currentPowerLineStatus = powerLineStatus;
        }
    }

    partial class HiddenForm
    {
        private readonly System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(0, 0);
            FormBorderStyle = FormBorderStyle.None;
            Name = "HiddenForm";
            Text = "HiddenForm";
            WindowState = FormWindowState.Minimized;
            Load += HiddenForm_Load;
            FormClosing += HiddenForm_FormClosing;
            ResumeLayout(false);
        }
    }
}