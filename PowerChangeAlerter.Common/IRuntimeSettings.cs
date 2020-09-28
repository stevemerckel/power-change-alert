namespace PowerChangeAlerter.Common
{
    /// <summary>
    /// Collection of runtime settings for use by the application
    /// </summary>
    public interface IRuntimeSettings
    {
        /// <summary>
        /// Whether you are running the application in a debugger
        /// </summary>
        bool IsLocalDebugging { get; }

        /// <summary>
        /// Display name for the email sender
        /// </summary>
        string EmailSenderName { get; set; }

        /// <summary>
        /// Email address for sender
        /// </summary>
        string EmailSenderAddress { get; set; }

        /// <summary>
        /// SMTP server name
        /// </summary>
        string EmailSmtpServer { get; set; }

        /// <summary>
        /// Port number for SMTP server
        /// </summary>
        int EmailSmtpPort { get; set; }

        /// <summary>
        /// Display name for the email recipient
        /// </summary>
        string EmailRecipientName { get; set; }

        /// <summary>
        /// Email address for recipient
        /// </summary>
        string EmailRecipientAddress { get; set; }

        /// <summary>
        /// Whether to use secure transmission
        /// </summary>
        bool EmailSmtpUseSsl { get; set; }

        /// <summary>
        /// Username for auth against <see cref="EmailSmtpServer"/>
        /// </summary>
        string EmailSmtpLogonName { get; set; }

        /// <summary>
        /// Password for <see cref="EmailSmtpLogonName"/> when authenticating against <see cref="EmailSmtpServer" />
        /// </summary>
        string EmailSmtpLogonPassword { get; set; }

        /// <summary>
        /// Validates the settings of the details stored in the settings file
        /// </summary>
        void Validate();
    }
}