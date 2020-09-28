using Moq;
using NUnit.Framework;
using PowerChangeAlerter.Common;
using System;
using System.Linq;

namespace PowerChangeAlerter.Tests
{
    /// <summary>
    /// Abstract class for wrapping up tests against SMTP outbound communication
    /// </summary>
    public abstract class SmtpHelperTestBase
    {
        private SmtpHelper _sut;
        private Mock<IAppLogger> _logger;
        private readonly DefaultTestObjects _defaultObjs = new DefaultTestObjects();
        private readonly bool _shouldSendEmail;

        /// <summary>
        /// Default ctor
        /// </summary>
        /// <param name="shouldActuallySendEmail">Whether this series of tests should actually send the SMTP message (<c>true</c>) or bypass the final call to send (<c>false</c>)</param>
        public SmtpHelperTestBase(bool shouldActuallySendEmail)
        {
            _shouldSendEmail = shouldActuallySendEmail;
        }

        [SetUp]
        public void Setup()
        {
            // setup objects
            _logger = _defaultObjs.LoggerMock;
            var rs = _defaultObjs.RuntimeSettings;
            _sut = new SmtpHelper(rs, _logger.Object, _shouldSendEmail);

            // dump init info
            Console.WriteLine($"Finished running {nameof(Setup)} --- {nameof(_shouldSendEmail)}={_shouldSendEmail}");
        }

        [Test]
        public void Test_EmailSend_Success()
        {
            var subject = $"NUnit-Tests";
            var body = $"This is a test from {nameof(Test_EmailSend_Success)}";
            Assert.DoesNotThrow(() => _sut.Send(subject, body));

            var warningCalls = _logger.Invocations.Where(x => x.Method.Name == nameof(_logger.Object.Warn));
            var errorCalls = _logger.Invocations.Where(x => x.Method.Name == nameof(_logger.Object.Error));
            var criticalCalls = _logger.Invocations.Where(x => x.Method.Name == nameof(_logger.Object.Critical));

            // ensure no catastrophes
            Assert.AreEqual(0, errorCalls.Count());
            Assert.AreEqual(0, criticalCalls.Count());

            if (_shouldSendEmail)
            {
                // assertions when *actually* sending a SMTP message
                Assert.IsTrue(warningCalls.Count() == 0, $"At least one call to {nameof(_logger.Object.Warn)} was made!");

                // nothing left to do, so exit early
                return;
            }

            // assertions when *faking* the send of a SMTP message
            Assert.IsTrue(warningCalls.Count() > 0, $"No calls to {nameof(_logger.Object.Warn)} were made!");
            Assert.IsTrue(warningCalls.Count() == 1, $"More than one call to {nameof(_logger.Object.Warn)} was made!");
            var warningMessage = warningCalls.First().Arguments.First();
            Assert.AreEqual("Did not send email because _isEmailSendingAllowed was False", warningMessage, "Warning message did not match!");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Test_EmailSend_MissingSenderAddress_Fail(string address)
        {
            var rs = _defaultObjs.RuntimeSettingsMock;
            rs.Setup(x => x.EmailSenderAddress).Returns(address);
            _sut = new SmtpHelper(rs.Object, _logger.Object, _shouldSendEmail);
            Assert.DoesNotThrow(() => _sut.Send("TestSubject", "TestBody"));
            var loggerErrors = _logger.Invocations.Where(x => x.Method.Name == nameof(_logger.Object.Error));
            Assert.AreEqual(1, loggerErrors.Count());
            var errorMessage = loggerErrors.First().Arguments.First();
            Assert.IsTrue(errorMessage.ToString().Contains("Unable to send email because setting 'EmailSenderAddress'"), "Unable to find the target error message!");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Test_EmailSend_MissingRecipientAddress_Fail(string address)
        {
            var rs = _defaultObjs.RuntimeSettingsMock;
            rs.Setup(x => x.EmailRecipientAddress).Returns(address);
            _sut = new SmtpHelper(rs.Object, _logger.Object, _shouldSendEmail);
            Assert.DoesNotThrow(() => _sut.Send("TestSubject", "TestBody"));
            var loggerErrors = _logger.Invocations.Where(x => x.Method.Name == nameof(_logger.Object.Error));
            Assert.AreEqual(1, loggerErrors.Count());
            var errorMessage = loggerErrors.First().Arguments.First();
            Assert.IsTrue(errorMessage.ToString().Contains("Unable to send email because setting 'EmailRecipientAddress'"), "Unable to find the target error message!");
        }
    }
}