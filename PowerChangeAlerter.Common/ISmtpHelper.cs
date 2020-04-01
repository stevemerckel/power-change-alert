namespace PowerChangeAlerter.Common
{
    /// <summary>
    /// Handles SMTP/Email Communication
    /// </summary>
    public interface ISmtpHelper
    {
        /// <summary>
        /// Sends an e-mail message
        /// </summary>
        /// <param name="subject">Message subject</param>
        /// <param name="message">Message body</param>
        void Send(string subject, string message);
    }
}