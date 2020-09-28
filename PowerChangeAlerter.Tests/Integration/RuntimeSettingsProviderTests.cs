using Moq;
using NUnit.Framework;
using PowerChangeAlerter.Common;

namespace PowerChangeAlerter.Tests.Integration
{
    [TestFixture(Ignore = "true", IgnoreReason = "Need to first refactor the class from a singleton to instance, and adjust ctor so that mocks can be passed in for IFileManager and IAppLogger.")]
    public sealed class RuntimeSettingsProviderTests
    {
        private readonly DefaultTestObjects _defaultObjects = new DefaultTestObjects();
        private Mock<IAppLogger> _logger;
        private RuntimeSettingsProvider _sut;

        [SetUp]
        public void Setup()
        {
            _logger = _defaultObjects.LoggerMock;
        }

    }
}