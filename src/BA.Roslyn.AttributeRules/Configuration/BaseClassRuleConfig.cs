using BA.Roslyn.AttributeRules.Core.Rules;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace BA.Roslyn.AttributeRules.Configuration
{
    internal class BaseClassRuleConfig : AttributeRuleBaseConfig
    {
        public TypeSpecificationConfig BaseClassType { get; set; } = null!;

        public TypeSpecificationConfig AttributeType { get; set; } = null!;

        public bool AnalyzeAbstractClasses { get; set; } = false;

        internal override RuleBuildResult BuildRule(string name, Compilation compilation)
        {
            var baseClass = BaseClassType.Build(compilation);
            var attributeClass = AttributeType.Build(compilation);

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
