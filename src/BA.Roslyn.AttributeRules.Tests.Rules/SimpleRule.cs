﻿using BA.Roslyn.AttributeRules.Abstractions;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace BA.Roslyn.AttributeRules.Tests.Rules
{
    public class SimpleRule : RuleBase
    {
        public SimpleRule(bool param)
        {

        }

        public override SymbolKind TargetSymbolKind => throw new NotImplementedException();

        public override void Check(AttributeRuleExecutionContext context)
        {
        }

        public override void Init(AttributeRuleInitialisationContext context)
        {
        }
    }
}
