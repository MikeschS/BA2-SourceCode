using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BA.Roslyn.AttributeRules.Core.Rules
{
    internal class BaseClassRule : IRule
    {
        private INamedTypeSymbol selector;
        private INamedTypeSymbol requiredAttributeClass;
        private bool analyzeabstractClasses;

        public BaseClassRule(INamedTypeSymbol selector, INamedTypeSymbol requiredAttributeClass)
        {
            this.selector = selector;
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
            if (symbol.Kind != TargetSymbolKind)
            {
                return RuleResult.Success();
            }

            var typeSymbol = symbol as INamedTypeSymbol;

            if (typeSymbol == null)
            {
                return RuleResult.Success();
            }

            if (!analyzeabstractClasses && typeSymbol.IsAbstract)
            {
                return RuleResult.Success();
            }

            if (!MatchesSelector(typeSymbol, selector))
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

        private bool MatchesSelector(INamedTypeSymbol typeSymbol, INamedTypeSymbol baseSymbol)
        {
            var currentSymbol = typeSymbol;
            while (currentSymbol != null)
            {
                if (selector.TypeKind == TypeKind.Interface)
                {
                    foreach (var @interface in currentSymbol.AllInterfaces)
                    {
                        if (baseSymbol.IsUnboundGenericType)
                        {
                            var tempComparer = @interface;

                            if (tempComparer.Arity > 0)
                            {
                                tempComparer = tempComparer.ConstructUnboundGenericType();
                            }

                            if (SymbolEqualityComparer.Default.Equals(tempComparer, baseSymbol))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            if (SymbolEqualityComparer.Default.Equals(@interface, baseSymbol))
                            {
                                return true;
                            }
                        }
                    }
                }
                else if (selector.TypeKind == TypeKind.Class)
                {
                    if (currentSymbol.BaseType == null)
                    {
                        return false;
                    }

                    if (baseSymbol.IsUnboundGenericType)
                    {
                        var tempComparer = currentSymbol.BaseType;

                        if (tempComparer.Arity > 0)
                        {
                            tempComparer = tempComparer.ConstructUnboundGenericType();
                        }

                        if (SymbolEqualityComparer.Default.Equals(tempComparer, baseSymbol))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (SymbolEqualityComparer.Default.Equals(currentSymbol.BaseType, baseSymbol))
                        {
                            return true;
                        }
                    }
                }

                currentSymbol = currentSymbol.BaseType;
            }

            return false;
        }
    }
}
