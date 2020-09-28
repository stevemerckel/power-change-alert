using Moq;
using NUnit.Framework;
using PowerChangeAlerter.Common;
using System;
using System.IO;
using System.Linq;
using System.Net;

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
        public void Test_DownloadTextFile_ValidateContentInspection_Success()
        {
            const string Url = "https://raw.githubusercontent.com/stevemerckel/power-change-alert/master/README.md";
            Console.WriteLine($"Url = {Url}");
            var downloadFile = Path.Combine(_workDirectory, new Uri(Url).Segments.Last());
            Console.WriteLine($"{nameof(downloadFile)} = {downloadFile}");
            WebClient wc = null;
            try
            {
                wc = new WebClient();
                var startOn = DateTime.Now;
                wc.DownloadFile(Url, downloadFile);
                var downloadTimeNeeded = DateTime.Now.Subtract(startOn);
                Console.WriteLine($"Finished downloading file after {downloadTimeNeeded.TotalMilliseconds} milliseconds");
            }
            finally
            {
                wc?.Dispose();
            }

            Assert.IsTrue(_sut.FileExists(downloadFile));
            Assert.IsTrue(new FileInfo(downloadFile).Length > 0, $"No content was found in '{downloadFile}'");
            Assert.IsFalse(_sut.IsFileBlocked(downloadFile));
            const string KnownSnipInTextFile = "Before Running or Debugging";
            var fileContent = _sut.ReadAllText(downloadFile);
            Assert.IsTrue(fileContent.Length > 0);
            Assert.IsTrue(fileContent.Contains(KnownSnipInTextFile, StringComparison.InvariantCulture), $"Could not find known text of '{KnownSnipInTextFile}'");
        }

        [TestCase("https://raw.githubusercontent.com/stevemerckel/power-change-alert/master/README.md", false)]
        [TestCase("https://raw.githubusercontent.com/stevemerckel/power-change-alert/master/resources/naudio.1.10.0.nupkg.zip", true)]
        public void Test_DownloadFile_UnblockIfSpecified_Success(string url, bool shouldRequireUnblock)
        {
            Console.WriteLine($"{nameof(url)} = {url}");
            Console.WriteLine($"{nameof(shouldRequireUnblock)} = {shouldRequireUnblock}");
            var downloadFile = Path.Combine(_workDirectory, new Uri(url).Segments.Last());
            Console.WriteLine($"{nameof(downloadFile)} = {downloadFile}");
            WebClient wc = null;
            try
            {
                wc = new WebClient();
                var startOn = DateTime.Now;
                wc.DownloadFile(url, downloadFile);
                var downloadTimeNeeded = DateTime.Now.Subtract(startOn);
                Console.WriteLine($"Finished downloading file after {downloadTimeNeeded.TotalMilliseconds} milliseconds");
            }
            finally
            {
                wc?.Dispose();
            }

            // initial download validations
            Assert.IsTrue(_sut.FileExists(downloadFile));
            Assert.AreEqual(shouldRequireUnblock, _sut.IsFileBlocked(downloadFile));

            // now we can unblock file if it is needed
            if (_sut.IsFileBlocked(downloadFile))
                _sut.UnblockFile(downloadFile);
            Assert.IsFalse(_sut.IsFileBlocked(downloadFile));

            // secondary validations
            Assert.IsTrue(new FileInfo(downloadFile).Length > 0, $"No content was found in '{downloadFile}'");
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