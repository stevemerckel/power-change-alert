using Moq;
using NUnit.Framework;
using PowerChangeAlerter.Common;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;

namespace PowerChangeAlerter.Tests.Integration
{
    /// <summary>
    /// Collection of tests for working with the Windows file system (via <seealso cref="System.IO"/> namespace)
    /// </summary>
    [TestFixture]
    [Category(CategoryNames.WindowsFileSystem)]
    public sealed class LocalFileManagerTests
    {
        private readonly IFileManager _sut = new LocalFileManager();
        private readonly DefaultTestObjects _defaultMoqs = new DefaultTestObjects();
        private Mock<IAppLogger> _logger;
        private string _workDirectory;

        [SetUp]
        public void Setup()
        {
            _logger = _defaultMoqs.LoggerMock;
            var currentDirectory = TestContext.CurrentContext.TestDirectory;
            var randomSubDirectory = Utility.MakeRandomString(8);
            _workDirectory = Path.Combine(currentDirectory, randomSubDirectory);
            Directory.CreateDirectory(_workDirectory);
            Console.WriteLine($"{nameof(_workDirectory)} was set to: {_workDirectory}  (exists? {Directory.Exists(_workDirectory)})");
        }

        [Test]
        public void Test_CreateAndReadFile_Success()
        {
            var randomString = Utility.MakeRandomString(8, false, true);
            var randomFileLocation = Path.Combine(_workDirectory, $"dummy-file.{randomString}.txt");
            var content = $"Lorem ipsum, yada yada yada ... {DateTime.Now}";
            File.WriteAllText(randomFileLocation, content);
            var retrieved = _sut.ReadAllText(randomFileLocation);
            Assert.AreEqual(content, retrieved, "The content did not match!!");
        }

        [Test]
        public void Test_DownloadTextFile_Success()
        {
            const string Url = "https://raw.githubusercontent.com/stevemerckel/power-change-alert/master/README.md";
            Console.WriteLine($"Url = {Url}");
            var randomString = Utility.MakeRandomString(8, false, true);
            var randomFileLocation = Path.Combine(_workDirectory, $"dummy-download.{randomString}.txt");
            WebClient wc = null;
            try
            {
                wc = new WebClient();
                var startOn = DateTime.Now;
                wc.DownloadFile(Url, randomFileLocation);
                var downloadTimeNeeded = DateTime.Now.Subtract(startOn);
                Console.WriteLine($"Finished downloading file after {downloadTimeNeeded.TotalMilliseconds} milliseconds");
            }
            finally
            {
                wc?.Dispose();
            }

            Assert.IsTrue(_sut.FileExists(randomFileLocation));
            Assert.IsTrue(new FileInfo(randomFileLocation).Length > 0, $"No content was found in '{randomFileLocation}'");
            const string KnownSnipInTextFile = "Before Running or Debugging";
            var fileContent = _sut.ReadAllText(randomFileLocation);
            Assert.IsTrue(fileContent.Length > 0);
            Assert.IsTrue(fileContent.Contains(KnownSnipInTextFile, StringComparison.InvariantCulture), $"Could not find known text of '{KnownSnipInTextFile}'");
        }

        [Test]
        public void Test_DownloadDllFile_Unblock_Success()
        {
            const string Url = "https://raw.githubusercontent.com/stevemerckel/power-change-alert/master/resources/NAudio.dll";
            Console.WriteLine($"Url = {Url}");
            var randomString = Utility.MakeRandomString(8, false, true);
            var randomFileLocation = Path.Combine(_workDirectory, $"dummy-download.{randomString}.dll");
            WebClient wc = null;
            try
            {
                wc = new WebClient();
                var startOn = DateTime.Now;
                wc.DownloadFile(Url, randomFileLocation);
                var downloadTimeNeeded = DateTime.Now.Subtract(startOn);
                Console.WriteLine($"Finished downloading file after {downloadTimeNeeded.TotalMilliseconds} milliseconds");
            }
            finally
            {
                wc?.Dispose();
            }

            // validate existence
            Assert.IsTrue(_sut.FileExists(randomFileLocation));
            Assert.IsTrue(new FileInfo(randomFileLocation).Length > 0, $"No content was found in '{randomFileLocation}'");

            // validate file is blocked
            Assert.IsTrue(_sut.IsFileBlocked(randomFileLocation));

            // unblock file and validate we can read the file
            _sut.UnblockFile(randomFileLocation);
            Assert.IsFalse(_sut.IsFileBlocked(randomFileLocation));
            Console.WriteLine("Unblocked the file");
            var fileContent = _sut.ReadAllText(randomFileLocation);
            Assert.IsTrue(fileContent.Length > 0);
        }

        [TearDown]
        public void Cleanup()
        {
            if (!Directory.Exists(_workDirectory))
            {
                Console.Error.WriteLine("The work directory was not found, so exiting early ... but ... WTH ??");
                return;
            }

            var files = Directory.EnumerateFiles(_workDirectory).ToList();
            var successfullyDeletedCount = 0;
            files.ForEach(x =>
            {
                try
                {
                    File.Delete(x);
                    successfullyDeletedCount++;
                }
                catch (IOException ioEx)
                {
                    Console.Error.WriteLine($"{nameof(IOException)} thrown trying to delete '{x}' --- Details: {ioEx}");
                }
                catch (UnauthorizedAccessException uaEx)
                {
                    Console.Error.WriteLine($"{nameof(UnauthorizedAccessException)} thrown trying to delete '{x}' --- Details: {uaEx}");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"{nameof(Exception)} thrown trying to delete '{x}' --- Details: {ex}");
                }
            });

            if (successfullyDeletedCount == files.Count())
                Console.WriteLine($"Successfully deleted all {successfullyDeletedCount} files!");
            else
                Console.Error.WriteLine($"Only deleted {successfullyDeletedCount} files out of {files.Count()}, see previous log statements for more details.");

            var isWorkDirectoryDeleted = false;
            try
            {
                Directory.Delete(_workDirectory);
                isWorkDirectoryDeleted = true;
            }
            catch (IOException ioEx)
            {
                Console.Error.WriteLine($"{nameof(IOException)} thrown trying to delete work directory --- Details: {ioEx}");
            }
            catch (UnauthorizedAccessException uaEx)
            {
                Console.Error.WriteLine($"{nameof(UnauthorizedAccessException)} thrown trying to delete work directory --- Details: {uaEx}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{nameof(Exception)} thrown trying to delete work directory --- Details: {ex}");
            }

            if (isWorkDirectoryDeleted)
                Console.WriteLine($"Successfully deleted work directory!");
            else
                Console.Error.WriteLine($"Failed to delete work directory, see previous log statements for more details.");
        }
    }
}