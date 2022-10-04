using NUnit.Framework;
using PowerChangeAlerter.Common;
using System;
using System.Linq;

namespace PowerChangeAlerter.Tests.Unit
{
    /// <summary>
    /// Tests the <seealso cref="Utility"/> functions
    /// </summary>
    [TestFixture]
    public sealed class UtilityTests
    {
        /// <remarks>
        /// For the string length, make sure you use a very large pool size!
        /// </remarks>
        [TestCase(300, false, false)]
        [TestCase(300, false, true)]
        [TestCase(300, true, false)]
        [TestCase(300, true, true)]
        public void Test_MakeRandomString_ValidateInputFlags_Success(int stringLength, bool includeLowerCase, bool includeNumbers)
        {
            // create the random string
            var generated = Utility.MakeRandomString(stringLength, includeLowerCase, includeNumbers);
            Console.WriteLine($"Generated string: {generated}");
            
            // check lengths
            Assert.AreEqual(stringLength, generated.Length);

            // generate character array for subsequent checks
            var characters = generated.ToCharArray();

            // ensure at least one uppercase
            Assert.IsTrue(characters.Any(x => char.IsUpper(x)), $"No upper-case characters found at all!! -- {generated}");

            // check presence of lower-case against param
            if (includeLowerCase)
                Assert.IsTrue(characters.Any(x => char.IsLower(x)), $"No lower-case characters found at all!! -- {generated}");
            else
                Assert.IsTrue(characters.All(x => !char.IsLower(x)), $"Found lower-case character, but was told not to include them!! -- {generated}");

            // check presence of numbers against param
            if (includeNumbers)
                Assert.IsTrue(characters.Any(x => char.IsDigit(x)), $"No numbers found at all!! -- {generated}");
            else
                Assert.IsTrue(characters.All(x => !char.IsDigit(x)), $"Found numbers, but was told not to include them!! -- {generated}");
        }

        [TestCase(0, false, false)]
        [TestCase(0, true, false)]
        [TestCase(0, false, true)]
        [TestCase(0, true, true)]
        [TestCase(-1, false, false)]
        [TestCase(-1, true, false)]
        [TestCase(-1, false, true)]
        [TestCase(-1, true, true)]
        [TestCase(int.MinValue, false, false)]
        [TestCase(int.MinValue, true, false)]
        [TestCase(int.MinValue, false, true)]
        [TestCase(int.MinValue, true, true)]
        public void Test_MakeRandomString_LessThanOneCharThrows_Fail(int stringLength, bool includeLowerCase, bool includeNumbers)
        {
            Assert.Throws<ArgumentException>(() => Utility.MakeRandomString(stringLength, includeLowerCase, includeNumbers));
        }

        [Test]
        public void Test_CodeEntryAttribute_Success()
        {
            var codeEntryAttribute = new CodeEntryAttribute();
            Assert.AreEqual(1, codeEntryAttribute.HitCount);
        }
    }
}