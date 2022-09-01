using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BA.Roslyn.AttributeRules.Core.Rules
{
    internal class BaseClassRule : IRule
    {
        private INamedTypeSymbol baseClass;
        private INamedTypeSymbol requiredAttributeClass;
        private bool analyzeabstractClasses;

        public BaseClassRule(INamedTypeSymbol baseClass, INamedTypeSymbol requiredAttributeClass)
        {
            this.baseClass = baseClass;
            this.requiredAttributeClass = requiredAttributeClass;
        }

        public SymbolKind TargetSymbolKind => SymbolKind.NamedType;

        public BaseClassRule WithAnalyzeAbstractClasses(bool analyzeAbstractClasses)
        {
            this.analyzeabstractClasses = analyzeAbstractClasses;
            return this;
        }

        public RuleResult Check(ISymbol symbol)
        {
            var typeSymbol = symbol as INamedTypeSymbol;

            if (typeSymbol == null)
            {
                return RuleResult.Success();
            }

            if (!analyzeabstractClasses && typeSymbol.IsAbstract)
            {
                return RuleResult.Success();
            }

            if (!HasTransientBase(typeSymbol, baseClass))
            {
                return RuleResult.Success();
            }

            var attributeTypes = typeSymbol.GetAttributes().Select(t => t.AttributeClass).Where(t => t != null).Select(t => t!).ToList();

            if (attributeTypes.Any(t => SymbolEqualityComparer.Default.Equals(t, requiredAttributeClass)))
            {
                return RuleResult.Success();
            }

            return RuleResult.Fail($"Class misses the {requiredAttributeClass.Name} attribute");
        }

        private bool HasTransientBase(INamedTypeSymbol typeSymbol, INamedTypeSymbol baseSymbol)
        {
            var currentSymbol = typeSymbol;

            while (currentSymbol != null)
            {
                if (currentSymbol.BaseType == null)
                {
                    return false;
                }

                if (SymbolEqualityComparer.Default.Equals(currentSymbol.BaseType, baseSymbol))
                {
                    return true;
                }

                currentSymbol = currentSymbol.BaseType;
            }

            return false;
        }
    }
}
