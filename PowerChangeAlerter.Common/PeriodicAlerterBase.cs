using System;
using System.Threading;

namespace PowerChangeAlerter.Common
{
    /// <summary>
    /// Abstract set of features for periodic alerts.  All implementations of <seealso cref="IPeriodicAlerter"/> should instead inherit from this base class.
    /// </summary>
    [Obsolete("Any implementations are not ready for primetime... yet")]
    public abstract class PeriodicAlerterBase : IPeriodicAlerter
    {
        private readonly IAppLogger _logger;
        private CancellationTokenSource _cts;
        private Timer _timer;
        private volatile int _startupDelayInMilliseconds;
        private volatile int _intervalDelayInMilliseconds;
        private volatile bool _isRunning; // important to init as "false"
        private readonly object _lock = new object();

        /// <summary>
        /// Base ctor
        /// </summary>
        /// <param name="logger">Logger implementation</param>
        protected PeriodicAlerterBase(IAppLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            throw new NotImplementedException($"{nameof(PeriodicAlerterBase)} has not yet been approved for primetime.");
        }

        /// <inheritdoc />
        public void StartAsync(TimeSpan startupDelay, TimeSpan delayBetweenChecks)
        {
            if (IsRunning())
                throw new InvalidOperationException("Cannot start a service until the other has been stopped!");

            const int MaxDelayAllowedInMinutes = 30;
            if (startupDelay != null && startupDelay.TotalMinutes > MaxDelayAllowedInMinutes)
                throw new ArgumentException($"{nameof(startupDelay)} must be less than {MaxDelayAllowedInMinutes} minutes!");

            if (delayBetweenChecks != null && delayBetweenChecks.TotalMinutes > MaxDelayAllowedInMinutes)
                throw new ArgumentException($"{nameof(delayBetweenChecks)} must be less than {MaxDelayAllowedInMinutes} minutes!");

            _cts = new CancellationTokenSource();

            _startupDelayInMilliseconds = startupDelay == null || startupDelay.TotalSeconds < 1
                ? (int)TimeSpan.FromSeconds(1).TotalMilliseconds
                : (int)startupDelay.TotalMilliseconds;
            _intervalDelayInMilliseconds = delayBetweenChecks == null || delayBetweenChecks.TotalSeconds < 1
                ? (int)TimeSpan.FromMinutes(5).TotalMilliseconds
                : (int)delayBetweenChecks.TotalMilliseconds;

            _timer = new Timer(BootstrapProcess, null, _startupDelayInMilliseconds, Timeout.Infinite);
        }

        /// <inheritdoc />
        public abstract void ActionToPerform(CancellationToken token);

        /// <inheritdoc />
        public abstract bool ConditionToCheck(CancellationToken token);

        /// <summary>
        /// Handles basics of halting timer, running main logic (synchronously), and resetting the timer
        /// </summary>
        private void BootstrapProcess(object state)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            RunProcess(ConditionToCheck, ActionToPerform);
            _timer.Change(_intervalDelayInMilliseconds, Timeout.Infinite);
        }

        /// <inheritdoc />
        public void Stop()
        {
            var subclassTypeName = GetType().Name;
            _logger.Info($"{nameof(Stop)} called on {subclassTypeName}");
            var cancelRequestedAt = DateTime.Now;
            
            // declare cancellation, wait for all clear
            _cts.Cancel();
            const int MaxWaitInSeconds = 10;
            var exitBy = DateTime.Now.AddSeconds(MaxWaitInSeconds);
            while (DateTime.Now <= exitBy)
            {
                if (!IsRunning())
                    break;

                Thread.Sleep(500); // brief delay before re-check
            }

            if (IsRunning())
                _logger.Warn($"Tried to call {nameof(Stop)} for '{GetType().Name}', but could not shut it down within {TimeSpan.FromSeconds(MaxWaitInSeconds).TotalMilliseconds} milliseconds!");
            else
                _logger.Info($"Finished {nameof(Stop)} for '{GetType().Name}' in {Math.Abs(exitBy.Subtract(DateTime.Now).TotalMilliseconds)} milliseconds");
        }

        /// <summary>
        /// Main workflow for processing the <seealso cref="conditionToCheck"/> and <seealso cref="performIfConditionIsMet"/>
        /// </summary>
        /// <param name="conditionToCheck"></param>
        /// <param name="performIfConditionIsMet"></param>
        private void RunProcess(Func<CancellationToken, bool> conditionToCheck, Action<CancellationToken> performIfConditionIsMet)
        {
            var typeName = GetType().Name;
            var token = _cts.Token;
            if (token.IsCancellationRequested)
            {
                _logger.Warn($"Cancellation token was set on '{typeName}' before {nameof(conditionToCheck)} fired.  Exiting early.");
                return;
            }

            var shouldRun = conditionToCheck(token);
            if (!shouldRun)
            {
                _logger.Info($"Was going to start {typeName}, but the {nameof(conditionToCheck)} evaluated to {shouldRun}");
                SetIsRunning(false);
                return;
            }

            if (token.IsCancellationRequested)
            {
                _logger.Warn($"Cancellation token was set on '{typeName}' before {nameof(performIfConditionIsMet)} fired.  Exiting early.");
                return;
            }

            _logger.Info($"Starting '{typeName}'");
            try
            {
                SetIsRunning(true);
                performIfConditionIsMet(token);
            }
            catch(Exception ex)
            {
                _logger.Error($"Error thrown while running '{typeName}' -- Details: {ex}");
            }
            finally
            {
                SetIsRunning(false);
            }

            _logger.Info($"Exiting from {nameof(RunProcess)} for '{typeName}' -- cancellation token's {nameof(token.IsCancellationRequested)} was {token.IsCancellationRequested}");
        }

        /// <summary>
        /// Wrapper with simple lock for updating <see cref="IsRunning"/>
        /// </summary>
        /// <param name="value">Value to assign</param>
        private void SetIsRunning(bool value)
        {
            lock (_lock)
            {
                _isRunning = value;
            }
        }

        /// <inheritdoc />
        public bool IsRunning()
        {
            lock(_lock)
            {
                return _isRunning;
            }
        }
    }
}