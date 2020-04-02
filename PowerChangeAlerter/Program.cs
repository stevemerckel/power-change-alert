using System;
using System.ServiceProcess;
using PowerChangeAlerter.Common;

namespace PowerChangeAlerter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            IRuntimeSettings rs = RuntimeSettingsProvider.Instance.GetRuntimeSettings();
            IAppLogger logger = new SerilogAppLogger(rs);
            IFileManager fm = new LocalFileManager();
            var targetService = new AlerterService(rs, logger, fm);

            if (rs.IsLocalDebugging)
            {
                logger.Info("  ***  Running with debugging within Visual Studio  ***  ");
                targetService.StartService();
                logger.Info("  ***  Press ENTER key to stop program  ***  ");
                Console.Read();
                targetService.StopService();
                return;
            }

            if (args.Length == 1 && args[0].Trim().ToUpper().Contains("CLI"))
            {
                logger.Info("  ***  Running as commandline application manually  ***  ");
                targetService.StartService();
                logger.Info("  ***  Press ENTER key to stop program  ***  ");
                Console.Read();
                targetService.StopService();
                return;
            }

            if (args.Length > 0)
            {
                logger.Info(@"Unsure how to start.  Either run as an established Windows Service, or use the ""-cli"" switch (without quotes) to run as console application.");
                Console.ReadLine();
                return;
            }

            // running normally as windows service
            var servicesToRun = new ServiceBase[] { targetService };
            ServiceBase.Run(servicesToRun);
        }
    }
}