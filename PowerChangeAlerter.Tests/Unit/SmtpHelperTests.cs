using NUnit.Framework;
using PowerChangeAlerter.Common;

namespace PowerChangeAlerter.Tests.Unit
{
    /// <summary>
    /// Unit tests for the <seealso cref="SmtpHelper"/> object
    /// </summary>
    [TestFixture]
    public sealed class SmtpHelperTests : SmtpHelperTestBase
    {
        public SmtpHelperTests() : base(false)
        {
        }
    }
}