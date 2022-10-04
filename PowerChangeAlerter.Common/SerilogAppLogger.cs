using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace PowerChangeAlerter.Common
{
    /// <summary>
    /// <para>Implementation of <see cref="IAppLogger"/> using Serilog.</para>
    /// <para>This relies on finding the "settings.serilog" file in the executing assembly's path.</para>
    /// </summary>
    /// <remarks>
    /// <para>All logged messages are also written to the DBG output stream (as long as the build includes the DEBUG constant).</para>
    /// </remarks>
    public sealed class SerilogAppLogger : IAppLogger
    {
        private readonly ILogger _serilog = null;

        /// <summary>
        /// Default ctor
        /// </summary>
        /// <param name="runtimeSettings">Runtime settings object</param>
        public SerilogAppLogger(IRuntimeSettings runtimeSettings, IFileManager fileManager)
        {
            if (runtimeSettings == null)
                throw new ArgumentNullException("Runtime settings was null!!");

            var serilogConfigFileLocation = fileManager.PathCombine(fileManager.PathGetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.serilog");
            var isConfigFound = fileManager.FileExists(serilogConfigFileLocation);
            System.Diagnostics.Debug.WriteLine($"Looking for Serilog config here: {serilogConfigFileLocation}");
            System.Diagnostics.Debug.WriteLine($"Serilog config was {(isConfigFound ? string.Empty : "**NOT** ")}found");
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(serilogConfigFileLocation)
                .Build();

            // decide output log file
            // - for within visual studio, write to "logs" folder in runtime directory
            // - for executing as process or windows service, write to vendor/app folder in CommonApplicationData directory
            var logDirectory = Utility.IsDebugging
                ? "./logs"
                : fileManager.PathCombine(fileManager.EnvironmentGetFolderPath(Environment.SpecialFolder.CommonApplicationData), StaticVariables.ProductName, "logs");

            logDirectory = logDirectory.Replace(@"\\", "/").Replace(@"\", "/");
            if (logDirectory.EndsWith("/"))
                logDirectory = logDirectory.TrimEnd('/');

            System.Diagnostics.Debug.WriteLine($"Log directory = {logDirectory}");

            _serilog = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .WriteTo.Map("ApplicationName", "MyOtherAppName", (name, wt) => wt.File($"{logDirectory}/log-{name}.txt"))
                .CreateLogger();

            Info($"{nameof(SerilogAppLogger)} exiting.");
        }

        /// <inheritdoc />
        public void Info(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[{nameof(Info)}] - {message}");
            _serilog.Information(message);
        }

        /// <inheritdoc />
        public void Warn(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[{nameof(Warn)}] - {message}");
            _serilog.Warning(message);
        }

        /// <inheritdoc />
        public void Error(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[{nameof(Error)}] - {message}");
            _serilog.Error(message);
        }

        /// <inheritdoc />
        public void Critical(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[{nameof(Critical)}] - {message}");
            _serilog.Fatal(message);
        }

        /// <inheritdoc />
        public void Debug(string message)
        {
            _serilog.Debug(message);
        }
    }
}