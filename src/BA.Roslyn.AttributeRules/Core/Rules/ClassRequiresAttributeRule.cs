﻿using BA.Roslyn.AttributeRules.Abstractions;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BA.Roslyn.AttributeRules.Core.Rules
{
    internal class ClassRequiresAttributeRule : RuleBase
    {
        private INamedTypeSymbol selector;
        private INamedTypeSymbol requiredAttributeClass;
        private bool analyzeabstractClasses;

        public ClassRequiresAttributeRule(INamedTypeSymbol selector, INamedTypeSymbol requiredAttributeClass)
        {
            this.selector = selector;
            this.requiredAttributeClass = requiredAttributeClass;
        }

        public override SymbolKind TargetSymbolKind => SymbolKind.NamedType;

        public ClassRequiresAttributeRule WithAnalyzeAbstractClasses(bool analyzeAbstractClasses)
        {
            this.analyzeabstractClasses = analyzeAbstractClasses;
            return this;
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

        public override void Check(AttributeRuleExecutionContext context)
        {
            var symbol = context.Symbol;

            if (symbol.Kind != TargetSymbolKind)
            {
                return;
            }

            var typeSymbol = symbol as INamedTypeSymbol;

            if (typeSymbol == null)
            {
                return;
            }

            if (!analyzeabstractClasses && typeSymbol.IsAbstract)
            {
                return;
            }

            if (!MatchesSelector(typeSymbol, selector))
            {
                return;
            }

            var attributeTypes = typeSymbol.GetAttributes().Select(t => t.AttributeClass).Where(t => t != null).Select(t => t!).ToList();

            if (attributeTypes.Any(t => SymbolEqualityComparer.Default.Equals(t, requiredAttributeClass)))
            {
                return;
            }

            context.EmitMessage($"Class misses the {requiredAttributeClass.Name} attribute");
            return;
        }

        public override void Init(AttributeRuleInitialisationContext context)
        {
        }
    }
}
