using System;

namespace PowerChangeAlerter.Common
{
    public interface IAlertManager
    {
        void NotifyTimeChange(DateTime previous, DateTime adjusted);

        void NotifyPowerOnBattery();

        void NotifyPowerFromWall();

        void ManagerStart();

        void ManagerStop();

        void ManagerPause();

        void ManagerContinue();

        void NotifyHostShutdown();
    }
}