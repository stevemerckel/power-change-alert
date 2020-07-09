using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        public string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        /// <inheritdoc />
        public Version GetExecutingAssemblyVersion()
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

        /// <inheritdoc />
        public List<string> GetFilesByPattern(string rootDirectory, string fileNamePattern)
        {
            return Directory.EnumerateFiles(rootDirectory, fileNamePattern, SearchOption.AllDirectories).ToList();
        }

        /// <inheritdoc />
        public string PathCombine(params string[] paths)
        {
            return Path.Combine(paths);
        }

        /// <inheritdoc />
        public string PathGetDirectoryName(string fileLocation)
        {
            return Path.GetDirectoryName(fileLocation);
        }

        /// <inheritdoc />
        public string EnvironmentGetFolderPath(Environment.SpecialFolder specialFolder)
        {
            return Environment.GetFolderPath(specialFolder);
        }
    }
}