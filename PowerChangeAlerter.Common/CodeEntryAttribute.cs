using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PowerChangeAlerter.Common
{
    /// <summary>
    /// Decorator adds a DEBUG level statement when the block of code is entered
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Method)]
    public class CodeEntryAttribute : Attribute
    {
        private int _hitCount;

        /// <remarks>
        /// <c>switch</c> logic built against this doc: https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.callermembernameattribute?view=net-6.0
        /// </remarks>
        public CodeEntryAttribute([CallerMemberName] string memberName = null)
        {
            string sourceType;
            switch (memberName?.ToLower())
            {
                case null:
                case "":
                    sourceType = "[Unknown]";
                    break;
                case ".ctor":
                    sourceType = "constructor";
                    break;
                case ".cctor":
                    sourceType = "static constructor";
                    break;
                case "finalize":
                    sourceType = "destructor";
                    break;
                default:
                    sourceType = "method or attribute";
                    break;
            }
            var message = $"Entered '{memberName ?? "[Unknown]"}' of type '{sourceType}'";
            Debug.WriteLine(message);
            RegisterHit();
        }

        private void RegisterHit()
        {
            if (_hitCount == int.MaxValue)
                _hitCount = 0;

            _hitCount++;
        }

        /// <summary>
        /// Gets the number of times this attribute instance was hit
        /// </summary>
        /// <remarks>
        /// REFACTOR: This property is really only present for unit test purposes.  If a better way is found, kill this and its backing member.
        /// </remarks>
        internal int HitCount
        {
            get { return _hitCount; }
        }
    }
}