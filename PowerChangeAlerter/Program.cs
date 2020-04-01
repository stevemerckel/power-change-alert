using System;
using System.Diagnostics;
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
            Debug.WriteLine("DEBUG: FOOBAR __ FOOBAR __ FOOBAR");
            Trace.WriteLine("TRACE: GEORGE ___ GEORGE ___ GEORGE");
            IRuntimeSettings rs = RuntimeSettingsProvider.Instance.GetRuntimeSettings();
            IAppLogger logger = new SerilogAppLogger(rs);
            IFileManager fm = new LocalFileManager();

            var targetService = new AlerterService(rs, logger, fm);

            if (rs.IsLocalDebugging)
            {
                logger.Info("Running as console application within Visual Studio");
                targetService.StartService();
                logger.Info("  ***  Press ENTER key to stop program  ***  ");
                Console.Read();
                targetService.StopService();
            }
            else if (args.Length == 1 && args[0].Trim().ToUpper().Contains("CLI"))
            {
                logger.Info("Running as commandline application manually");
                targetService.StartService();
                logger.Info("  ***  Press ENTER key to stop program  ***  ");
                Console.Read();
                targetService.StopService();
            }
            else
            {
                logger.Info("Running as windows service");
                var servicesToRun = new ServiceBase[]
                {
                    targetService
                };
                ServiceBase.Run(servicesToRun);
            }
        }
    }
}