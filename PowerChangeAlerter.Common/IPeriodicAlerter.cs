using System;
using System.Threading;

namespace PowerChangeAlerter.Common
{
    /// <summary>
    /// Contracts for sending periodic messages based on a predetermined condition to check
    /// </summary>
    public interface IPeriodicAlerter
    {
        /// <summary>
        /// After the (optional) delay, it runs the condition check to ensure we should proceed.  If that evaluates to <c>true</c>, it will run the received action.  Between each invocation of the action, it will wait the specified interval.
        /// </summary>
        /// <param name="startupDelay">Optional delay to place between calling this async method and performing the <paramref name="conditionToCheck"/> check</param>
        /// <param name="delayBetweenChecks">Optional delay between each evaluation/invocation of <paramref name="performIfConditionIsMet"/>, as long as <paramref name="conditionToCheck"/> still evaluates to <c>true</c></param>
        void StartAsync(TimeSpan startupDelay, TimeSpan delayBetweenChecks);

        /// <summary>
        /// Stops the running implementation, regardless of its current processing state
        /// </summary>
        void Stop();

        /// <summary>
        /// Whether the async logic from <see cref="StartAsync(Func{CancellationToken, bool}, Action{CancellationToken}, TimeSpan, TimeSpan)"/> is in the middle of running
        /// </summary>
        /// <returns>Returns <c>true</c> if running, otherwise returns <c>false</c></returns>
        bool IsRunning();

        /// <summary>
        ///     <para>What to do if <paramref name="ConditionToCheck"/> evaluates to <c>true</c></para>
        ///     <para>The action parameter takes in a <seealso cref="CancellationToken"/> object, which (internally) would be used to identify an early exit request from the external runner</para>
        /// </summary>
        /// <param name="token">Cancellation token to monitor internally</param>
        void ActionToPerform(CancellationToken token);

        /// <summary>
        ///     <para>Logic for deciding whether to run the <paramref name="ActionToPerform"/></para>
        ///     <para>The function parameter takes in a <seealso cref="CancellationToken"/> object, which (internally) would be used to identify an early exit request form the external runner</para>
        ///     <para>The function would return a boolean, indicating whether the result was <c>true</c> or <c>false</c></para>
        /// </summary>
        /// <param name="token">Cancellation token to monitor internally</param>
        /// <returns>Boolean indicating whether to run <see cref="ActionToPerform"/> -- <c>true</c> runs the action, or <c>false</c> skips the action</returns>
        bool ConditionToCheck(CancellationToken token);
    }
}