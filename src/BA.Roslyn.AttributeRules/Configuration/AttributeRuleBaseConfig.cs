using BA.Roslyn.AttributeRules.Core.Rules;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace BA.Roslyn.AttributeRules.Configuration
{
    public abstract class AttributeRuleBaseConfig
    {
        internal abstract RuleBuildResult BuildRule(string name, Compilation compilation);
    }
}
