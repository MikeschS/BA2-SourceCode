using BA.Roslyn.AttributeRules.Abstractions;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace BA.Roslyn.AttributeRules.Tests.Rules
{
    // TODO!!!
    public class MyCustomRule : RuleBase
    {
        private readonly string selector1;
        private readonly string attribute;
        private INamedTypeSymbol selector;
        private INamedTypeSymbol requiredAttributeClass;
        private bool analyzeabstractClasses;

        public MyCustomRule(string selector, string attribute, bool analyzeAbstractClasses)
        {
            selector1 = selector;
            this.attribute = attribute;
            this.analyzeabstractClasses = analyzeAbstractClasses;
        }

        public override SymbolKind TargetSymbolKind => SymbolKind.NamedType;

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

            var attributeTypes = typeSymbol.GetAttributes().Select(t => t.AttributeClass).Where(t => t != null).Select(t => t).ToList();

            if (attributeTypes.Any(t => SymbolEqualityComparer.Default.Equals(t, requiredAttributeClass)))
            {
                return;
            }

            context.EmitMessage($"Class misses the {requiredAttributeClass.Name} attribute");
            return;
        }

        public override void Init(AttributeRuleInitialisationContext context)
        {
            var selector = context.Compilation.GetTypeByMetadataName(selector1);
            var attributeClass = context.Compilation.GetTypeByMetadataName(attribute);

            if (selector == null)
            {
                context.EmitMessage("Selector type was not found");
                return;
            }

            if (attributeClass == null)
            {
                context.EmitMessage("Attribute class was not found");
                return;
            }

            if (!new TypeKind[] { TypeKind.Interface, TypeKind.Class }.Contains(selector.TypeKind))
            {
                context.EmitMessage("Selector is not the required typeKind");
                return;
            }

            if (!new TypeKind[] { TypeKind.Class }.Contains(attributeClass.TypeKind))
            {
                context.EmitMessage("Attribute type is not a class");
                return;
            }

            this.selector = selector;
            this.requiredAttributeClass = attributeClass;
            return;
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
