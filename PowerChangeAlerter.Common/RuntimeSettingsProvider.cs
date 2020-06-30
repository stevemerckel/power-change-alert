using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace PowerChangeAlerter.Common
{
    /// <inheritdoc />
    /// <remarks>
    /// Singleton pattern based on Jon Skeet's 4th Singleton implementation: https://csharpindepth.com/articles/singleton
    /// </remarks>
    public sealed class RuntimeSettingsProvider : IRuntimeSettingsProvider
    {
        private static readonly string RuntimeSettingsFileName = "settings.runtime";
        private static readonly IFileManager _fm = new LocalFileManager();
        private readonly IRuntimeSettings _settings;

        /// <remarks>
        /// explicit ctor required for C# compiler to not mark type as beforefieldinit
        /// </remarks>
        static RuntimeSettingsProvider()
        {
        }

        private RuntimeSettingsProvider()
        {
            // validate presence of settings file
            var executingDirectory = _fm.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var runtimeSettingsFileLocation = _fm.PathCombine(executingDirectory, RuntimeSettingsFileName);

            // check runtime directory first for settings file
            var isSettingsFileFound = _fm.FileExists(runtimeSettingsFileLocation);

            if (!isSettingsFileFound)
            {
                // check any sub-folders from the runtime directory for the settings file
                Debug.WriteLine($"Could not find '{RuntimeSettingsFileName}' in '{executingDirectory}' -- Going to look in any possible sub-directories...");
                runtimeSettingsFileLocation = _fm.GetFilesByPattern(executingDirectory, RuntimeSettingsFileName).FirstOrDefault();
                isSettingsFileFound = runtimeSettingsFileLocation != null;
                var scanMessage = $"The '{RuntimeSettingsFileName}' file was {(isSettingsFileFound ? string.Empty : "NOT")} found.";
                if (isSettingsFileFound)
                    scanMessage += $" -- using file found here:  {runtimeSettingsFileLocation}";
                Debug.WriteLine(scanMessage);
            }

            if (!isSettingsFileFound)
                throw new Exception($"Could not find file '{RuntimeSettingsFileName}' at '{executingDirectory}'  (or any sub-directories)");

            // read JSON content and hydrate properties
            var contents = _fm.ReadAllText(runtimeSettingsFileLocation);
            if (string.IsNullOrWhiteSpace(contents))
                throw new Exception($"The '{RuntimeSettingsFileName}' file was found but had no content! -- file location = {runtimeSettingsFileLocation}");

            var target = JsonConvert.DeserializeObject<RuntimeSettings>(contents);
            _settings = target;
        }

        /// <summary>
        /// Returns the single instance of this object
        /// </summary>
        public static RuntimeSettingsProvider Instance { get; } = new RuntimeSettingsProvider();

        /// <inheritdoc />
        public IRuntimeSettings GetRuntimeSettings()
        {
            return _settings;
        }
    }
}