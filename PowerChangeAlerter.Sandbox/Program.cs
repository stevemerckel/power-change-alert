using PowerChangeAlerter.Common;
using System;

namespace PowerChangerAlerter.Sandbox
{
    sealed class Program
    {
        /// <summary>
        /// This EXE is only used for ad-hoc testing of various functionality.  It is not to be used for actual builds or MSI files.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // serilog
            IRuntimeSettings rs = RuntimeSettingsProvider.Instance.GetRuntimeSettings();
            IFileManager fm = new LocalFileManager();
            IAppLogger logger = new SerilogAppLogger(rs, fm);
            logger.Info("meh");

            Console.WriteLine();
            Console.WriteLine("Press ENTER to quit");
            Console.ReadLine();
        }
    }
}