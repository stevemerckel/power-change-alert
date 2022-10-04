namespace PowerChangeAlerter.Common
{
    /// <summary>
    /// Application logging interface
    /// </summary>
    public interface IAppLogger
    {
        /// <summary>
        /// Writes a DEBUG level message
        /// </summary>
        /// <param name="message">Message to write</param>
        void Debug(string message);

        /// <summary>
        /// Writes an INFO level message
        /// </summary>
        /// <param name="message">Message to write</param>
        void Info(string message);

        /// <summary>
        /// Writes an WARNING level message
        /// </summary>
        /// <param name="message">Message to write</param>
        void Warn(string message);

        /// <summary>
        /// Writes an ERROR level message
        /// </summary>
        /// <param name="message">Message to write</param>
        /// <remarks>
        /// Any ERROR level issues that require immediate notification should be routed through <see cref="Critical(string)"/> instead.
        /// </remarks>
        void Error(string message);

        /// <summary>
        /// Writes an CRITICAL/FATAL level message
        /// </summary>
        /// <param name="message">Message to write</param>
        /// <remarks>
        /// Any ERROR level issues that require immediate notification to software maintainers should be routed through this method.
        /// </remarks>
        void Critical(string message);
    }
}