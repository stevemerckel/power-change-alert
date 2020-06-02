using System;
using System.Diagnostics;
using System.IO;
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

            var serilogConfigFileLocation = fileManager.CombinePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.serilog");
            var isConfigFound = fileManager.FileExists(serilogConfigFileLocation);
            Debug.WriteLine($"Looking for Serilog config here: {serilogConfigFileLocation}");
            Debug.WriteLine($"Serilog config was {(isConfigFound ? string.Empty : "**NOT** ")}found");
            var ApplicationName = "_meh";
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(serilogConfigFileLocation)
                .Build();

            _serilog = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                //.MinimumLevel.Debug()
                //.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                //.Enrich.FromLogContext()
                .Enrich.WithProperty("George", "WhoWhatTimmay")
                .WriteTo.Map("ApplicationName", "MyOtherAppName", (name, wt) => wt.File($"./logs/log-fromcode-{name}.txt"))
                .CreateLogger();

            Info($"{nameof(SerilogAppLogger)} exiting.");
        }

        /// <inheritdoc />
        public void Info(string message)
        {
            Debug.WriteLine($"[{nameof(Info)}] - {message}");
            _serilog.Information(message);
        }

        /// <inheritdoc />
        public void Warn(string message)
        {
            Debug.WriteLine($"[{nameof(Warn)}] - {message}");
            _serilog.Warning(message);
        }

        /// <inheritdoc />
        public void Error(string message)
        {
            Debug.WriteLine($"[{nameof(Error)}] - {message}");
            _serilog.Error(message);
        }

        /// <inheritdoc />
        public void Critical(string message)
        {
            Debug.WriteLine($"[{nameof(Critical)}] - {message}");
            _serilog.Fatal(message);
        }
    }
}