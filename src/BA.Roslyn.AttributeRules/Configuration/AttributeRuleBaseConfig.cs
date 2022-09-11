using BA.Roslyn.AttributeRules.Core.Rules;
using CompactJson;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace BA.Roslyn.AttributeRules.Configuration
{
    [JsonTypeName(typeof(ClassRequiresAttributeRuleConfig), "ClassRequiresAttribute")]
    [JsonCustomConverter(typeof(TypedConverterFactory), "Type")]
    public abstract class AttributeRuleBaseConfig
    {
        internal abstract RuleBuildResult BuildRule(string name, Compilation compilation);
    }
}
