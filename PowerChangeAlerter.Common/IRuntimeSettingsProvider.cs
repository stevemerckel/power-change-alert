namespace PowerChangeAlerter.Common
{
    /// <summary>
    /// Functions for interacting with runtime settings
    /// </summary>
    public interface IRuntimeSettingsProvider
    {
        /// <summary>
        /// Fetches the runtime settings
        /// </summary>
        IRuntimeSettings GetRuntimeSettings();
    }
}