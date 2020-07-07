using System;
using System.Linq;
using System.ServiceProcess;
using System.Text.RegularExpressions;
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

            // write initialization info
            var startupMessage = rs.IsLocalDebugging
                ? "Running with debugging within Visual Studio"
                : "Running as commandline application manually";
            logger.Info($"  ***  {startupMessage}  ***  ");

            if (!rs.IsLocalDebugging && args.Length == 0)
            {
                // run as windows service
                var servicesToRun = new ServiceBase[] { targetService };
                ServiceBase.Run(servicesToRun);
                return;
            }

            // allowed args
            const string CommandlineSwitchArg = "CLI";
            const string HelpShortSwitch = "H";
            const string HelpLongSwitch = "HELP";
            var defaultErrorMessage = $"Unable to start.  Use switch '{CommandlineSwitchArg}' to run as commandline, or use '{HelpShortSwitch}' or '{HelpLongSwitch}' for help.";

            if (!rs.IsLocalDebugging && args.Length != 1)
            {
                // not in local debugging mode and arg list is missing or greater than 1 --> exit with error
                logger.Error(defaultErrorMessage);
                Environment.ExitCode = 1;
                return;
            }

            // grab first arg if it exists
            var receivedArg = args.Any()
                ? Regex.Replace(args[0].Trim().ToUpper(), "[^a-zA-Z]", string.Empty)
                : null;

            // decide behavior from arg value
            switch (receivedArg)
            {
                case CommandlineSwitchArg:
                case null:
                    RunCommandline(targetService, logger);
                    break;
                case HelpShortSwitch:
                case HelpLongSwitch:
                    logger.Info($"You can run from commandline with '{CommandlineSwitchArg}' param, or register and run this as a Windows Service.");
                    break;
                default:
                    logger.Error($"Did not recognize argument '{receivedArg}'.  Use switch '{HelpLongSwitch}' for more info.");
                    Environment.ExitCode = 2;
                    break;
            }
        }

        private static void RunCommandline(AlerterService targetService, IAppLogger logger)
        {
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
    }
}