using BA.Roslyn.AttributeRules.Abstractions;
using BA.Roslyn.AttributeRules.Core.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace BA.Roslyn.AttributeRules.Configuration
{
    internal class RuleBuildResult
    {
        private RuleBuildResult(IRule rule)
        {
            Rule = rule;
        }

        private RuleBuildResult(string error)
        {
            Error = error;
        }

        public IRule? Rule { get; }

        public string? Error { get; }

        internal static RuleBuildResult Success(IRule rule)
        {
            return new RuleBuildResult(rule);
        }

        internal static RuleBuildResult Fail(string error)
        {
            return new RuleBuildResult(error);
        }
    }
}
