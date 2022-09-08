using Microsoft.CodeAnalysis;
using System;

namespace BA.Roslyn.AttributeRules.Abstractions
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = true)]
    public abstract class RuleBase : Attribute
    {
        public abstract void Init(AttributeRuleInitialisationContext context);

        public abstract void Check(AttributeRuleExecutionContext context);

        public abstract SymbolKind TargetSymbolKind { get; }
    }
}
