using NUnit.Framework;
using PowerChangeAlerter.Common;

namespace PowerChangeAlerter.Tests.Integration
{
    /// <summary>
    /// Integration tests for the <seealso cref="SmtpHelper"/> object
    /// </summary>
    [TestFixture]
    [Category(CategoryNames.SendsEmail)]
    public sealed class SmtpHelperTests : SmtpHelperTestBase
    {
        public SmtpHelperTests() : base(true)
        {
        }
    }
}