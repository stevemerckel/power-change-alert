using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PowerChangeAlerter.Common
{
    /// <summary>
    /// Abstract set of features for periodic alerts.  All implementations of <seealso cref="IPeriodicAlerter"/> should instead inherit from this base class.
    /// </summary>
    internal abstract class PeriodicAlerterBase : IPeriodicAlerter
    {
        private readonly IAppLogger _logger;
        private Task _asyncAction;
        private Func<bool> _conditionToCheck;
        private CancellationTokenSource _cts;

        protected PeriodicAlerterBase(IAppLogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public void StartAsync(Func<bool> conditionToCheck, Action performIfConditionIsMet, TimeSpan startupDelay, TimeSpan delayBetweenChecks)
        {
            _cts = new CancellationTokenSource();
            _conditionToCheck = conditionToCheck;
            _asyncAction = new Task(performIfConditionIsMet, TaskCreationOptions.LongRunning);
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Stop()
        {
            var subclassTypeName = GetType().Name;
            _logger.Info($"{nameof(Stop)} called on {subclassTypeName}");
            var cancelRequestedAt = DateTime.Now;
            _cts.Cancel();
            _asyncAction.Wait(TimeSpan.FromSeconds(30));
            Debug.WriteLine($"Task cancellation status on {subclassTypeName} is {_asyncAction.Status} -- Time waited was approximately {DateTime.Now.Subtract(cancelRequestedAt).TotalMilliseconds} milliseconds");
            var isStopped = _asyncAction.Status == (TaskStatus.Canceled | TaskStatus.Faulted | TaskStatus.RanToCompletion);
            Debug.WriteLine($"{nameof(isStopped)} in {subclassTypeName} evaluated to {isStopped}");
        }
    }
}
