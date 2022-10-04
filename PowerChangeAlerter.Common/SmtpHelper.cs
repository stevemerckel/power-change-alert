using System;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace PowerChangeAlerter.Common
{
    /// <inheritdoc />
    public sealed class SmtpHelper : ISmtpHelper
    {
        private readonly IAppLogger _logger;
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly bool _isEmailSendingAllowed = true; // important to default to "true", only override for unit tests

        /// <summary>
        /// SMTP email sender object
        /// </summary>
        /// <param name="runtimeSettings">Runtime settings</param>
        /// <param name="logger">Application logger instance</param>
        public SmtpHelper(IRuntimeSettings runtimeSettings, IAppLogger logger)
        {
            _runtimeSettings = runtimeSettings;
            _logger = logger;
        }

        /// <summary>
        /// SMTP email sender object for use by unit- and integration-tests.
        /// </summary>
        /// <param name="runtimeSettings">Runtime settings</param>
        /// <param name="logger">Application logger instance</param>
        /// <param name="isEmailSendingAllowed">Whether to actually send emails (<c>true</c>) or to skip the actual sending of a message (<c>false</c>)</param>
        internal SmtpHelper(IRuntimeSettings runtimeSettings, IAppLogger logger, bool isEmailSendingAllowed) : this(runtimeSettings, logger)
        {
            _isEmailSendingAllowed = isEmailSendingAllowed;
        }

        /// <inheritdoc />
        public void Send(string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(_runtimeSettings.EmailSenderAddress))
            {
                _logger.Error($"Unable to send email because setting '{nameof(_runtimeSettings.EmailSenderAddress)}' is missing! -- subject = {subject} -- body = {body}");
                return;
            }

            if (string.IsNullOrWhiteSpace(_runtimeSettings.EmailRecipientAddress))
            {
                _logger.Error($"Unable to send email because setting '{nameof(_runtimeSettings.EmailRecipientAddress)}' is missing! -- subject = {subject} -- body = {body}");
                return;
            }

            SmtpClient client = null;
            MailMessage mm = null;
            try
            {
                _logger.Debug("Creating mail message");
                mm = new MailMessage
                {
                    BodyEncoding = Encoding.UTF8,
                    IsBodyHtml = false,
                    From = new MailAddress(_runtimeSettings.EmailSenderAddress, _runtimeSettings.EmailSenderName ?? _runtimeSettings.EmailSenderAddress),
                    Subject = subject,
                    Body = body
                };
                mm.To.Add(new MailAddress(_runtimeSettings.EmailRecipientAddress, _runtimeSettings.EmailRecipientName ?? _runtimeSettings.EmailRecipientAddress));

                _logger.Debug("Creating SMTP Client");
                client = new SmtpClient
                {
                    Host = _runtimeSettings.EmailSmtpServer,
                    Port = _runtimeSettings.EmailSmtpPort,
                    EnableSsl = _runtimeSettings.EmailSmtpUseSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_runtimeSettings.EmailSmtpLogonName, _runtimeSettings.EmailSmtpLogonPassword),
                    Timeout = 30000
                };

                if (_isEmailSendingAllowed)
                {
                    _logger.Debug("Sending message...");
                    client.Send(mm);
                }
                else
                {
                    _logger.Warn($"Did not send email because {nameof(_isEmailSendingAllowed)} was {_isEmailSendingAllowed}");
                }
            }
            catch (SmtpException smtpEx)
            {
                var message = $"{nameof(SmtpException)} trying to send SmtpClient message: {smtpEx}";
                if (smtpEx.InnerException != null)
                    message += $" --- Inner Exception: {smtpEx.InnerException}";
                _logger?.Error(message);
            }
            catch (Exception ex)
            {
                var message = $"General exception trying to send SmtpClient message: {ex}";
                if (ex.InnerException != null)
                    message += $" --- Inner Exception: {ex.InnerException}";
                _logger?.Error(message);
            }
            finally
            {
                client?.Dispose();
                mm?.Dispose();
            }
        }
    }
}