using System;
using System.Diagnostics;
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
        public SerilogAppLogger(IRuntimeSettings runtimeSettings)
        {
            if (runtimeSettings == null)
                throw new ArgumentNullException("Runtime settings was null!!");

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("settings.serilog")
                .Build();

            _serilog = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.WithProperty("George", "WhoWhatTimmay")
                .CreateLogger();
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