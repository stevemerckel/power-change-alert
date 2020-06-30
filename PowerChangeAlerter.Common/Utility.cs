using System;
using System.Diagnostics;

namespace PowerChangeAlerter.Common
{
    /// <summary>
    /// Collection of random helper functions
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Logic to determine whether we are debugging the runtime or not
        /// </summary>
        public static bool IsDebugging => Debugger.IsAttached && Environment.UserInteractive;
    }
}