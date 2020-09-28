namespace PowerChangeAlerter.Tests
{
    /// <summary>
    /// Collection of category names that can be used for the <seealso cref="NUnit.Framework.CategoryAttribute"/>'s parameter name on decorating test methods/functions.
    /// </summary>
    public static class CategoryNames
    {
        /// <summary>
        /// These tests will send a SMTP message
        /// </summary>
        public const string SendsEmail = "SendsEmail";

        /// <summary>
        /// These tests directly interact with the Windows file system
        /// </summary>
        public const string WindowsFileSystem = "WindowsFileSystem";
    }
}