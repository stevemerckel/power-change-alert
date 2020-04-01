using System;
using System.Reflection;
using Newtonsoft.Json;

namespace PowerChangeAlerter.Common
{
    /// <inheritdoc />
    /// <remarks>
    /// Designed from Jon Skeet's 4th Singleton implementation: https://csharpindepth.com/articles/singleton
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
            var runtimeSettingsFileLocation = _fm.CombinePath(executingDirectory, RuntimeSettingsFileName);
            var isSettingsFileFound = _fm.FileExists(runtimeSettingsFileLocation);
            if (!isSettingsFileFound)
                throw new Exception($"Could not find file '{RuntimeSettingsFileName}' at this location: {runtimeSettingsFileLocation}");

            // read JSON content and hydrate properties
            var contents = _fm.ReadAllText(runtimeSettingsFileLocation);
            if (string.IsNullOrWhiteSpace(contents))
                throw new Exception($"The '{RuntimeSettingsFileName}' file was found but had no content! -- file location = {runtimeSettingsFileLocation}");

            var target = JsonConvert.DeserializeObject<RuntimeSettings>(contents);
            _settings = target;
        }

        public static RuntimeSettingsProvider Instance { get; } = new RuntimeSettingsProvider();

        /// <inheritdoc />
        public IRuntimeSettings GetRuntimeSettings()
        {
            return _settings;
        }
    }
}