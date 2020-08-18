using System;
using System.Threading;

namespace PowerChangeAlerter.Common
{
    /// <summary>
    /// POC implementaiton that dumps out console lines
    /// </summary>
    public sealed class DummyPeriodicAlerter : PeriodicAlerterBase
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        /// <param name="logger">Logger object</param>
        public DummyPeriodicAlerter(IAppLogger logger) : base(logger)
        {
        }

        /// <inheritdoc />
        public override void ActionToPerform(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override bool ConditionToCheck(CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}