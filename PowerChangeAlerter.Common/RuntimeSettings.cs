using System;
using System.Diagnostics;

namespace PowerChangeAlerter.Common
{
    /// <inheritdoc />
    public sealed class RuntimeSettings : IRuntimeSettings
    {
        /// <inheritdoc />
        public bool IsLocalDebugging => Environment.UserInteractive && Debugger.IsAttached;

        /// <inheritdoc />
        public string EmailSenderName { get; set; }

        /// <inheritdoc />
        public string EmailSenderAddress { get; set; }

        /// <inheritdoc />
        public string EmailSmtpServer { get; set; }

        /// <inheritdoc />
        public int EmailSmtpPort { get; set; }

        /// <inheritdoc />
        public string EmailRecipientName { get; set; }

        /// <inheritdoc />
        public string EmailRecipientAddress { get; set; }

        /// <inheritdoc />
        public bool EmailSmtpUseSsl { get; set; }

        /// <inheritdoc />
        public string EmailSmtpLogonName { get; set; }

        /// <inheritdoc />
        public string EmailSmtpLogonPassword { get; set; }
    }
}