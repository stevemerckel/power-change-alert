using Moq;
using PowerChangeAlerter.Common;
using System;

namespace PowerChangeAlerter.Tests
{
    /// <summary>
    /// <para>Contains common objects and mocks that are pre-initialized.</para>
    /// <para>The purpose is to consolidate frequently used (i.e. boilerplate) code for test objects into a single "get".</para>
    /// </summary>
    public sealed class DefaultTestObjects
    {
        /// <summary>
        /// Returns mock logger object that writes all messages to <seealso cref="Console"/>
        /// </summary>
        public Mock<IAppLogger> LoggerMock
        {
            get
            {
                var logger = new Mock<IAppLogger>();
                logger
                    .Setup(x => x.Critical(It.IsAny<string>()))
                    .Callback<string>(message => Console.WriteLine($"{nameof(logger.Object.Critical)} - {message}"));
                logger
                    .Setup(x => x.Error(It.IsAny<string>()))
                    .Callback<string>(message => Console.WriteLine($"{nameof(logger.Object.Error)} - {message}"));
                logger
                    .Setup(x => x.Info(It.IsAny<string>()))
                    .Callback<string>(message => Console.WriteLine($"{nameof(logger.Object.Info)} - {message}"));
                logger
                    .Setup(x => x.Warn(It.IsAny<string>()))
                    .Callback<string>(message => Console.WriteLine($"{nameof(logger.Object.Warn)} - {message}"));

                return logger;
            }
        }

        /// <summary>
        /// Returns a fake (aka legal but not valid) list of values for runtime settings
        /// </summary>
        public Mock<IRuntimeSettings> RuntimeSettingsMock
        {
            get
            {
                const string DummySenderEmail = "unit-test@local.host";
                var fakeSettings = new Mock<IRuntimeSettings>();
                
                fakeSettings
                    .Setup(x => x.EmailRecipientAddress)
                    .Returns("moq@moq.com");
                fakeSettings
                    .Setup(x => x.EmailRecipientName)
                    .Returns("Moq Library");
                fakeSettings
                    .Setup(x => x.EmailSenderAddress)
                    .Returns(DummySenderEmail);
                fakeSettings
                    .Setup(x => x.EmailSenderName)
                    .Returns("Unit Test");
                fakeSettings
                    .Setup(x => x.EmailSmtpLogonName)
                    .Returns(DummySenderEmail);
                fakeSettings
                    .Setup(x => x.EmailSmtpLogonPassword)
                    .Returns("1-2-3-4-5"); // it's the same combination as my luggage!
                fakeSettings
                    .Setup(x => x.EmailSmtpPort)
                    .Returns(587);
                fakeSettings
                    .Setup(x => x.EmailSmtpServer)
                    .Returns("fake.smtp.meh");

                return fakeSettings;
            }
        }

        /// <summary>
        /// Returns the actual runtime settings data, as read from the runtime settings file.  (See README doc for specifics.)
        /// </summary>
        public IRuntimeSettings RuntimeSettings
        {
            get
            {
                return RuntimeSettingsProvider.Instance.GetRuntimeSettings();
            }
        }
    }
}