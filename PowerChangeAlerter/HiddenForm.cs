using Microsoft.Win32;
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
            _now = DateTime.Now;
        }

        private void HiddenForm_Load(object sender, EventArgs e)
        {
            SystemEvents.TimeChanged += SystemEvents_TimeChanged;
            Debug.WriteLine($"{nameof(HiddenForm_Load)} fired!");
        }

        private void HiddenForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SystemEvents.TimeChanged -= SystemEvents_TimeChanged;
            Debug.WriteLine($"{nameof(HiddenForm_FormClosing)} fired!");
        }

        private void SystemEvents_TimeChanged(object sender, EventArgs e)
        {
            Debug.WriteLine($"{nameof(SystemEvents_TimeChanged)} ({nameof(HiddenForm)}) hit!!");

            _stopWatch.Stop();
            DateTime previousSystemDateTime;
            lock (_lock)
            {
                previousSystemDateTime = _now.Add(_stopWatch.Elapsed);
                _now = DateTime.Now;
                _stopWatch.Reset();
                _stopWatch.Start();
            }

            const int ThresholdInSeconds = 15;
            int deltaSeconds = Math.Abs((int)previousSystemDateTime.Subtract(DateTime.Now).TotalSeconds);
            if (deltaSeconds < ThresholdInSeconds)
            {
                Debug.WriteLine($"An insignificant time change of {deltaSeconds} seconds was detected -- ignored because under {ThresholdInSeconds} second threshold");
                return;
            }

            var newSystemDateTime = DateTime.Now;
            var msg = $"{nameof(SystemEvents_TimeChanged)} from {previousSystemDateTime} to {newSystemDateTime:MM/dd/yyyy hh:mm:ss tt}";
            Debug.WriteLine(msg);
            _alertManager.NotifyTimeChange(previousSystemDateTime, newSystemDateTime);
        }
    }

    partial class HiddenForm
    {
        private System.ComponentModel.IContainer components = null;

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