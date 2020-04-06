using Serilog.Core;
using Serilog.Events;

namespace PowerChangeAlerter.Common
{
    /// <summary>
    /// Adds variable injection logic to Serilog implementation
    /// </summary>
    public sealed class SerilogVariableEnricher : ILogEventEnricher
    {
        private readonly IRuntimeSettings _runtimeSettings;
        public SerilogVariableEnricher(IRuntimeSettings runtimeSettings)
        {
            _runtimeSettings = runtimeSettings;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            
        }
    }
}