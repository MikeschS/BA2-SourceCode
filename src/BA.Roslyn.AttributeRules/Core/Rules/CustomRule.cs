using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace BA.Roslyn.AttributeRules.Core.Rules
{
    internal class CustomRule : IRule
    {
        private readonly Action<AttributeRuleExecutionContext> action;

        public CustomRule(SymbolKind targetSymbolKind, Action<AttributeRuleExecutionContext> action)
        {
            TargetSymbolKind = targetSymbolKind;
            this.action = action;
        }

        public SymbolKind TargetSymbolKind { get; private set; }

        public void Check(AttributeRuleExecutionContext context)
        {
            action.Invoke(context);
        }
    }
}
