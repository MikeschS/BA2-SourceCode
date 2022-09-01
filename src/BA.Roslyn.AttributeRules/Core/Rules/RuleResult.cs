using System;
using System.Collections.Generic;
using System.Text;

namespace BA.Roslyn.AttributeRules.Core.Rules
{
    internal class RuleResult
    {
        private RuleResult(bool success, string? errorMessage)
        {
            IsSuccess = success;
            ErrorMessage = errorMessage;
        }

        public bool IsSuccess { get; }

        public string? ErrorMessage { get; }

        internal static RuleResult Fail(string errorMessage)
        {
            return new RuleResult(false, errorMessage);
        }

        internal static RuleResult Success()
        {
            return new RuleResult(true, null);
        }
    }
}
