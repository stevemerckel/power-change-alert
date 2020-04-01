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
        string EmailSenderName { get; }

        /// <summary>
        /// Email address for sender
        /// </summary>
        string EmailSenderAddress { get; }

        /// <summary>
        /// SMTP server name
        /// </summary>
        string EmailSmtpServer { get; }

        /// <summary>
        /// Port number for SMTP server
        /// </summary>
        int EmailSmtpPort { get; }

        /// <summary>
        /// Display name for the email recipient
        /// </summary>
        string EmailRecipientName { get; }

        /// <summary>
        /// Email address for recipient
        /// </summary>
        string EmailRecipientAddress { get; }

        /// <summary>
        /// Whether to use secure transmission
        /// </summary>
        bool EmailSmtpUseSsl { get; }

        /// <summary>
        /// Username for auth against <see cref="EmailSmtpServer"/>
        /// </summary>
        string EmailSmtpLogonName { get; }

        /// <summary>
        /// Password for <see cref="EmailSmtpLogonName"/> when authenticating against <see cref="EmailSmtpServer" />
        /// </summary>
        string EmailSmtpLogonPassword { get; }
    }
}