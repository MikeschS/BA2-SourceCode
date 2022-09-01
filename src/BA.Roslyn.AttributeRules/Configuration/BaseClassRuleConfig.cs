using BA.Roslyn.AttributeRules.Core.Rules;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace BA.Roslyn.AttributeRules.Configuration
{
    internal class BaseClassRuleConfig : AttributeRuleBaseConfig
    {
        public string BaseClassTypeName { get; set; } = null!;

        public string AttributeTypeName { get; set; } = null!;

        public bool AnalyzeAbstractClasses { get; set; } = false;

        internal override RuleBuildResult BuildRule(string name, Compilation compilation)
        {
            var baseClass = compilation.GetTypeByMetadataName(BaseClassTypeName);
            var attributeClass = compilation.GetTypeByMetadataName(AttributeTypeName);

            if (baseClass == null)
            {
                return RuleBuildResult.Fail("Base class was not found");
            }

            if (attributeClass == null)
            {
                return RuleBuildResult.Fail("Attribute class was not found");
            }

            var rule = new BaseClassRule(baseClass, attributeClass).WithAnalyzeAbstractClasses(AnalyzeAbstractClasses);

            return RuleBuildResult.Success(rule);
        }
    }
}
