namespace PowerChangeAlerter.Common
{
    /// <summary>
    /// Primary contracts for working with the alert system
    /// </summary>
    public interface IAlertManager
    {
        /// <summary>
        /// Notifies that a time change happened and will decide whether to act on the change
        /// </summary>
        ///// <param name="previous">The previous datetime</param>
        ///// <param name="adjusted">The new datetime</param>
        void NotifyTimeChange();
        //void NotifyTimeChange(DateTime previous, DateTime adjusted);

        /// <summary>
        /// Notifies alert manager that power changed to device battery
        /// </summary>
        void NotifyPowerOnBattery();

        /// <summary>
        /// Notifies alert manager that power changed to wall power
        /// </summary>
        void NotifyPowerFromWall();

        /// <summary>
        /// Starts the alert monitoring
        /// </summary>
        void ManagerStart();

        /// <summary>
        /// Stops the alert monitoring
        /// </summary>
        void ManagerStop();

        /// <summary>
        /// Notifies alert manager that the host is shutting down
        /// </summary>
        void NotifyHostShutdown();
    }
}