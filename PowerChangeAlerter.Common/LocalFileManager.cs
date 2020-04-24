using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Trinet.Core.IO.Ntfs;

namespace PowerChangeAlerter.Common
{
    /// <inheritdoc />
    public sealed class LocalFileManager : IFileManager
    {
        /// <inheritdoc />
        public bool FileExists(string fileLocation)
        {
            if (string.IsNullOrWhiteSpace(fileLocation))
                throw new ArgumentNullException(nameof(fileLocation), "Value received was null/empty!");

            return File.Exists(fileLocation);
        }

        /// <inheritdoc />
        public string ReadAllText(string fileLocation)
        {
            if (string.IsNullOrWhiteSpace(fileLocation))
                throw new ArgumentNullException(nameof(fileLocation), "Value received was null/empty!");

            return File.ReadAllText(fileLocation.Trim());
        }

        /// <inheritdoc />
        public void UnblockFile(string fileLocation)
        {
            if (!FileExists(fileLocation))
                throw new FileNotFoundException("Cannot unblock a file that cannot be found!");

            const string AlternateDataStreamNameToRemove = "Zone.Identifier";
            new FileInfo(fileLocation).DeleteAlternateDataStream(AlternateDataStreamNameToRemove);
        }

        /// <inheritdoc />
        public string CombinePath(string first, string second)
        {
            return Path.Combine(first, second);
        }

        /// <inheritdoc />
        public string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        /// <inheritdoc />
        public Version GetCurrentAssemblyVersion()
        {
            return GetAssemblyVersion(Assembly.GetExecutingAssembly().Location);
        }

        /// <inheritdoc />
        public Version GetAssemblyVersion(string fileLocation)
        {
            if (!File.Exists(fileLocation))
                return null;

            Version version;
            try
            {
                version = Version.Parse(FileVersionInfo.GetVersionInfo(fileLocation).FileVersion);
            }
            catch
            {
                version = null;
            }

            return version;
        }
    }
}