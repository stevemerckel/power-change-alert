using System;
using System.Net.Mail;

namespace PowerChangeAlerter.Common
{
    /// <inheritdoc />
    public sealed class RuntimeSettings : IRuntimeSettings
    {
        /// <inheritdoc />
        public bool IsLocalDebugging => Utility.IsDebugging;

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

        /// <inheritdoc />
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(EmailSenderAddress))
                throw new ArgumentException(nameof(EmailSenderAddress));

            var mm = new MailAddress(EmailSenderAddress);

            if (string.IsNullOrWhiteSpace(EmailSmtpServer))
                throw new ArgumentException(nameof(EmailSmtpServer));

            if (EmailSmtpPort < 1 || EmailSmtpPort > 65536)
                throw new ArgumentException(nameof(EmailSmtpPort));

            if (string.IsNullOrWhiteSpace(EmailRecipientAddress))
                throw new ArgumentException(nameof(EmailRecipientAddress));

            mm = new MailAddress(EmailRecipientAddress);

            if (string.IsNullOrWhiteSpace(EmailSmtpLogonName))
                throw new ArgumentException(nameof(EmailSmtpLogonName));

            if (string.IsNullOrWhiteSpace(EmailSmtpLogonPassword))
                throw new ArgumentException(nameof(EmailSmtpLogonPassword));
        }
    }
}