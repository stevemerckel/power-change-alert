using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PowerChangeAlerter.Common
{
    /// <summary>
    /// Collection of random helper functions
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Logic to determine whether we are debugging the runtime or not
        /// </summary>
        public static bool IsDebugging => Debugger.IsAttached && Environment.UserInteractive;

        /// <summary>
        /// Generates a random string based on the length required.  Note that some characters that appear similar to numbers are excluded from results.
        /// </summary>
        /// <param name="length">The target string length</param>
        /// <param name="includeLowerCase">Whether to include lowercase with the (default) uppercase letters</param>
        /// <param name="includeNumbers">Whether to include numbers</param>
        /// <returns>The generated random string</returns>
        public static string MakeRandomString(int length, bool includeLowerCase = false, bool includeNumbers = false)
        {
            if (length < 0)
                throw new ArgumentException("Length must be greater than 0 !!");

            string[] capitals = { "A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "M", "N", "P", "R", "T", "U", "V", "W", "X", "Y" };
            string[] lowers = capitals.ToList().Select(x => x.ToLower()).ToArray();
            string[] numbers = { "3", "4", "6", "7", "9" };

            // initialize pool and fill default data
            var pool = new string[capitals.Length];
            Array.Copy(capitals, pool, capitals.Length);

            if (includeLowerCase)
            {
                var previousLength = pool.Length;
                Array.Resize(ref pool, pool.Length + lowers.Length);
                Array.Copy(lowers, 0, pool, previousLength, lowers.Length);
            }

            if (includeNumbers)
            {
                var previousLength = pool.Length;
                Array.Resize(ref pool, pool.Length + numbers.Length);
                Array.Copy(numbers, 0, pool, previousLength, numbers.Length);
            }

            // fetch and build "random" selection
            var result = new StringBuilder(length);
            for (var i = 0; i < length; i++)
            {
                result.Append(pool.OrderBy(x => Guid.NewGuid()).First());
            }

            return result.ToString();
        }
    }
}