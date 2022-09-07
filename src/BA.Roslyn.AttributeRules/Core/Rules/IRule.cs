using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace BA.Roslyn.AttributeRules.Core.Rules
{
    internal interface IRule
    {
        void Check(AttributeRuleExecutionContext context);

        SymbolKind TargetSymbolKind { get; }
    }
}
