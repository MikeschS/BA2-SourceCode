using BA.Roslyn.AttributeRules.Abstractions;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace BA.Roslyn.AttributeRules.Tests.Rules
{
    public class ZeroParamRule : RuleBase
    {
        public override SymbolKind TargetSymbolKind => SymbolKind.NamedType;

        public override void Check(AttributeRuleExecutionContext context)
        {
            context.EmitMessage("ZeroParamRule");
        }

        public override void Init(AttributeRuleInitialisationContext context)
        {
            context.EmitMessage("ZeroParamRule");
        }
    }
}
