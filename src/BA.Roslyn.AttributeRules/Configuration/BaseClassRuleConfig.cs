using BA.Roslyn.AttributeRules.Core.Rules;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BA.Roslyn.AttributeRules.Configuration
{
    internal class BaseClassRuleConfig : AttributeRuleBaseConfig
    {
        public TypeSpecificationConfig Selector { get; set; } = null!;

        public TypeSpecificationConfig AttributeType { get; set; } = null!;

        public bool AnalyzeAbstractClasses { get; set; } = false;

        internal override RuleBuildResult BuildRule(string name, Compilation compilation)
        {
            var selector = Selector.Build(compilation);
            var attributeClass = AttributeType.Build(compilation);

            if (selector == null)
            {
                return RuleBuildResult.Fail("Selector type was not found");
            }

            if (attributeClass == null)
            {
                return RuleBuildResult.Fail("Attribute class was not found");
            }

            if (!new TypeKind[] { TypeKind.Interface, TypeKind.Class }.Contains(selector.TypeKind))
            {
                return RuleBuildResult.Fail("Selector is not the required typeKind");
            }

            if (!new TypeKind[] { TypeKind.Class }.Contains(attributeClass.TypeKind))
            {
                return RuleBuildResult.Fail("Attribute type is not a class");
            }

            var rule = new BaseClassRule(selector, attributeClass).WithAnalyzeAbstractClasses(AnalyzeAbstractClasses);

            return RuleBuildResult.Success(rule);
        }
    }
}
