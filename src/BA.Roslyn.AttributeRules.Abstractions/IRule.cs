using Microsoft.CodeAnalysis;
using System;

namespace BA.Roslyn.AttributeRules.Abstractions
{
    public interface IRule
    {
        void Init(AttributeRuleInitialisationContext context);

        void Check(AttributeRuleExecutionContext context);

        SymbolKind TargetSymbolKind { get; }
    }
}
