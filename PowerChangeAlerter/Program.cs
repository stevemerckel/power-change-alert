using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
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
            IFileManager fm = new LocalFileManager();
            IAppLogger logger = new SerilogAppLogger(rs, fm);
            var targetService = new AlerterService(rs, logger, fm);
            const string CommandlineSwitchArg = "CLI";

            if (rs.IsLocalDebugging  || (args.Length == 1 && args[0].Trim().ToUpper().Contains(CommandlineSwitchArg)))
            {
                // running as console app, and perhaps with a debugger attached
                var startupMessage = rs.IsLocalDebugging
                    ? "Running with debugging within Visual Studio"
                    : "Running as commandline application manually";
                logger.Info($"  ***  {startupMessage}  ***  ");
                targetService.StartService();
                Task.Run(() =>
                {
                    targetService.TriggerStandby();
                    Thread.Sleep(5000);
                    targetService.TriggerResume();
                    Thread.Sleep(5000);
                    targetService.TriggerAcToBattery();
                    Thread.Sleep(5000);
                    targetService.TriggerBatteryToAc();
                    logger.Info("Done with Power tests");
                });
                logger.Info("  ***  Press ENTER key to stop program  ***  ");
                Console.Read();
                targetService.StopService();
                return;
            }

            if (args.Length > 0)
            {
                logger.Error($"Unsure how to start.  Either run as an established Windows Service, or use the '{CommandlineSwitchArg}' switch (without quotes) to run as console application.");
                return;
            }

            // running normally as windows service
            var servicesToRun = new ServiceBase[] { targetService };
            ServiceBase.Run(servicesToRun);
        }
    }
}