using PowerChangeAlerter.Common;
using System;

namespace PowerChangerAlerter.Sandbox
{
    sealed class Program
    {
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