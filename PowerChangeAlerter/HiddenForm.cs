﻿using Microsoft.Win32;
using PowerChangeAlerter.Common;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace PowerChangeAlerter
{
    public partial class HiddenForm : Form
    {
        private readonly IAlertManager _alertManager;
        private readonly object _lock = new object();
        private readonly Stopwatch _stopWatch = new Stopwatch();
        private DateTime _now;

        public HiddenForm(IAlertManager alertManager)
        {
            InitializeComponent();
            Debug.WriteLine($"Entered ctor for {nameof(HiddenForm)}");
            _alertManager = alertManager;
            SystemEvents.TimeChanged += HandleTimeChanged;
            _now = DateTime.Now;
        }

        private void HandleTimeChanged(object sender, EventArgs e)
        {
            Debug.WriteLine($"{nameof(HandleTimeChanged)} ({nameof(HiddenForm)}) hit!!");
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
        }

        ~HiddenForm()
        {
            SystemEvents.TimeChanged -= HandleTimeChanged;
        }
    }
}
