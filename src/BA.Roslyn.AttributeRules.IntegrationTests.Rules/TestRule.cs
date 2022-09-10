using BA.Roslyn.AttributeRules.Abstractions;
using Microsoft.CodeAnalysis;
using System;

namespace BA.Roslyn.AttributeRules.IntegrationTests.Rules
{
    public class TestRule : RuleBase
    {
        public override SymbolKind TargetSymbolKind => SymbolKind.NamedType;

        public override void Check(AttributeRuleExecutionContext context)
        {
            context.EmitMessage($"Some Message for {context.Symbol.ToDisplayString()}");
        }

        public override void Init(AttributeRuleInitialisationContext context)
        {
            context.EmitMessage("sctirnerstcien");
        }
    }
}
