using System;

namespace PowerChangeAlerter.Common
{
    /// <summary>
    /// Contracts for sending periodic messages based on a predetermined condition to check
    /// </summary>
    internal interface IPeriodicAlerter
    {
        /// <summary>
        /// Rules for starting an asynchronous background worker <paramref name="performIfConditionIsMet"/> that will stop once the <paramref name="conditionToCheck"/> evaluates to <c>false</c>.
        /// </summary>
        /// <param name="conditionToCheck">Logic for deciding whether to perform <paramref name="performIfConditionIsMet"/></param>
        /// <param name="performIfConditionIsMet">What to do if <paramref name="conditionToCheck"/> evaluates to <c>true</c></param>
        /// <param name="startupDelay">Optional delay to place between calling this async method and performing the <paramref name="conditionToCheck"/> check</param>
        /// <param name="delayBetweenChecks">Optional delay between each evaluation/invocation of <paramref name="performIfConditionIsMet"/>, as long as <paramref name="conditionToCheck"/> still evaluates to <c>true</c></param>
        void StartAsync(Func<bool> conditionToCheck, Action performIfConditionIsMet, TimeSpan startupDelay, TimeSpan delayBetweenChecks);

        /// <summary>
        /// Stops the running implementation, regardless of its current processing state
        /// </summary>
        void Stop();
    }
}