using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace BA.Roslyn.Validation
{
    [Generator]
    public class ValidatorGenerator : IIncrementalGenerator
    {
        public const string DiagnosticId = "PWS0301";
        private const string Category = "DomainConvention";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly string title = "GeneratedValidators must have no constructors";

        private static readonly string messageFormat = "Validator '{0}' already has an constructor.";
        private static readonly string description = "Validators must have no constructor as it will be generated. Use marked methods for user code.";

        private static readonly DiagnosticDescriptor rule = new DiagnosticDescriptor(DiagnosticId, title, messageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: description);

        public const string DiagnosticId2 = "PWS0302";
        private const string Category2 = "DomainConvention";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly string title2 = "Property has more than one validator";

        private static readonly string messageFormat2 = "Property '{0}' has more than one validator.";
        private static readonly string description2 = "Properties must have no or exactly one validator. If more validators are available, specify with the attribute.";

        private static readonly DiagnosticDescriptor rule2 = new DiagnosticDescriptor(DiagnosticId2, title2, messageFormat2, Category2, DiagnosticSeverity.Error, isEnabledByDefault: true, description: description2);

        public const string DiagnosticId3 = "PWS0303";
        private const string Category3 = "DomainConvention";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly string title3 = "Attribute uses an invalid Validator";

        private static readonly string messageFormat3 = "Property '{0}' uses an invalid Validator: {1}.";
        private static readonly string description3 = "Properties must have no or exactly one validator. If more validators are available, specify with the attribute.";

        private static readonly DiagnosticDescriptor rule3 = new DiagnosticDescriptor(DiagnosticId3, title3, messageFormat3, Category3, DiagnosticSeverity.Error, isEnabledByDefault: true, description: description3);


        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var validatorBasesProvider = context.CompilationProvider.Select((t, token) =>
            {
                var myValidatorSymbol = t.GetTypeByMetadataName("FluentValidation.AbstractValidator`1");
                return myValidatorSymbol;
            });

            var allValidatorsProvider = context.CompilationProvider
                .SelectMany(GetTypes)
                .Combine(validatorBasesProvider)
                .Select((t, token) => new IntermediateInfo { TypeSymbol = t.Left, BaseValidator = t.Right })
                .Select(ExtractValidatorInfo).Where(t => t is not null).Select((t, token) => t!.Value).Collect();

            var localValidatorProvider = context.SyntaxProvider
                .CreateSyntaxProvider((node, _) => node is ClassDeclarationSyntax c && (c.BaseList?.Types.Any() ?? false), (ctx, _) => GetSemanticTargetForGeneration(ctx))
                .Where(m => m is not null)
                .Combine(validatorBasesProvider)
                .Select((t, token) => GetValidator(t.Right, t.Left))
                .Where(t => t is not null)
                .Select((t, token) => t!.Value)
                .Select((t, token) => GenerateLocalValidatorInput(t, token));

            context.RegisterSourceOutput(localValidatorProvider, NotifyOfExistingConstructors);

            var applicableValidatorsProvider = localValidatorProvider
                .Combine(allValidatorsProvider)
                .Select((t, token) => SelectAppropriateValidators(t.Left, t.Right, token));

            context.RegisterSourceOutput(applicableValidatorsProvider, GenerateBaseValidation);
        }

        private void NotifyOfExistingConstructors(SourceProductionContext context, LocalValidatorInput data)
        {
            foreach (var constructor in data.ExistingConstructors.Where(t => t.Parameters.Any()))
            {
                var diagnostic = Diagnostic.Create(rule, constructor.Locations.First(), data.Class.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }

        private void GenerateBaseValidation(SourceProductionContext context, GeneratorInfo data)
        {
            var sb = new StringBuilder();

            var dependencies = data.ValidatorRules.Where(t => t.Validator != null).Select(t => t.Validator).Concat(data.CustomValidationMethods.SelectMany(t => t.Method.Parameters.Select(t => t.Type.ToDisplayString()))).Distinct().Cast<string>().ToList();

            var dependencyNameLookup = new Dictionary<string, string>();

            foreach (var dependency in dependencies)
            {
                dependencyNameLookup.Add(dependency, dependency.Split('.').Last().ToLower());
            }

            var constructorDependencies = string.Join(", ", dependencyNameLookup.Select(t => $"{t.Key} {t.Value}"));

            foreach (var rule in data.ValidatorRules.Where(t => t.FailureReason != null))
            {
                switch (rule.FailureReason)
                {
                    case ValidatorLocatorFailureReason.MultipleValidators:
                        var diagnostic = Diagnostic.Create(rule2, data.ValidatorClass.Locations.First(), string.Join(".", rule.Path));
                        context.ReportDiagnostic(diagnostic);

                        break;

                    case ValidatorLocatorFailureReason.UseValidatorNoSymbolSelected:
                    case ValidatorLocatorFailureReason.UseValidatorDTONotMatching:
                    case ValidatorLocatorFailureReason.UseValidatorValidatorNotFound:
                        var diagnostic2 = Diagnostic.Create(rule3, data.ValidatorClass.Locations.First(), string.Join(".", rule.Path), rule.FailureReason.ToString());
                        context.ReportDiagnostic(diagnostic2);
                        break;
                }
            }

            var ruleSet = string.Join(Environment.NewLine, data.ValidatorRules.Where(t => t.Validator != null).Select(t => $"			RuleFor(m => m.{string.Join("!.", t.Path)}!).SetValidator({dependencyNameLookup[t.Validator]}).When(m => m.{string.Join("?.", t.Path)} != null);"));

            var customValidationCalls = string.Join(Environment.NewLine, data.CustomValidationMethods.Select(t =>
            {
                var parameters = string.Join(", ", t.Method.Parameters.Select(t => dependencyNameLookup[t.Type.ToDisplayString()]));

                return $"			this.{t.Method.Name}({parameters});";
            }));

            sb.Append(
$@"namespace {data.ValidatorClass.ContainingNamespace}
{{
	using FluentValidation;

	public partial class {data.ValidatorClass.Name}
	{{
		public {data.ValidatorClass.Name}({constructorDependencies})
		{{
{ruleSet}
{customValidationCalls}
		}}
	}}
}}");

            context.AddSource($"{data.ValidatorClass.Name}.g.cs", sb.ToString());
        }

        private GeneratorInfo SelectAppropriateValidators(LocalValidatorInput currentValidator, ImmutableArray<ValidatorInfo> availableValidators, CancellationToken cancellationToken)
        {
            var tempRules = GetPropertiesForValidation(Array.Empty<string>(), GetAllProperties(currentValidator.Dto), availableValidators, cancellationToken).ToImmutableArray();

            var customValidationMethods = currentValidator.Class.GetMembers().OfType<IMethodSymbol>().Where(t => !t.Name.Contains("ctor")).Select(t => new CustomValidationMethodInfo { Method = t });

            return new GeneratorInfo() { ValidatorRules = tempRules, ValidatorClass = currentValidator.Class, CustomValidationMethods = customValidationMethods.ToImmutableArray() };
        }

        private IEnumerable<(IEnumerable<string> Path, string? Validator, ValidatorLocatorFailureReason? FailureReason)> GetPropertiesForValidation(IEnumerable<string> path, IEnumerable<IPropertySymbol> properties, ImmutableArray<ValidatorInfo> availableValidators, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var property in properties.Where(t => t.Type.Name.EndsWith("DTO")))
            {
                var ignoreAttribute = property.GetAttributes().Where(t => t.AttributeClass?.Name == nameof(IgnorePropertyValidationAttribute)).Any();

                if (ignoreAttribute)
                {
                    yield return (path.Concat(new string[] { property.Name }), null, null);
                    break;
                }

                var applicableValidators = availableValidators.Where(t => SymbolEqualityComparer.Default.Equals(property.Type, t.DtoType)).ToList();

                switch (applicableValidators.Count)
                {
                    case 0:
                        foreach (var item in GetPropertiesForValidation(path.Concat(new string[] { property.Name }), GetAllProperties(property.Type), availableValidators, cancellationToken))
                        {
                            yield return item;
                        }

                        break;

                    case 1:
                        yield return (path.Concat(new string[] { property.Name }), applicableValidators.First().ValidatorType.ToDisplayString(), null);
                        break;

                    default:
                        var useValidatorAttribute = property.GetAttributes().Where(t => t.AttributeClass?.Name == nameof(UseValidatorAttribute)).SingleOrDefault();

                        if (useValidatorAttribute != null)
                        {
                            var selectedValidatorType = useValidatorAttribute.ConstructorArguments.FirstOrDefault().Value as INamedTypeSymbol;

                            if (selectedValidatorType == null)
                            {
                                yield return (path.Concat(new string[] { property.Name }), null, ValidatorLocatorFailureReason.UseValidatorNoSymbolSelected);
                                break;
                            }

                            var validator = applicableValidators.FirstOrDefault(t => SymbolEqualityComparer.Default.Equals(t.ValidatorType, selectedValidatorType));

                            if (validator.ValidatorType != null)
                            {
                                if (!SymbolEqualityComparer.Default.Equals(validator.DtoType, property.Type))
                                {
                                    yield return (path.Concat(new string[] { property.Name }), null, ValidatorLocatorFailureReason.UseValidatorDTONotMatching);
                                    break;
                                }

                                yield return (path.Concat(new string[] { property.Name }), validator.ValidatorType.ToDisplayString(), null);
                                break;
                            }

                            yield return (path.Concat(new string[] { property.Name }), null, ValidatorLocatorFailureReason.UseValidatorValidatorNotFound);
                        }
                        else
                        {
                            yield return (path.Concat(new string[] { property.Name }), null, ValidatorLocatorFailureReason.MultipleValidators);
                        }

                        break;

                }
            }
        }

        private LocalValidatorInput GenerateLocalValidatorInput(ValidatorInfo validator, CancellationToken token)
        {
            return new LocalValidatorInput
            {
                Class = validator.ValidatorType,
                Dto = validator.DtoType,
                ExistingConstructors = validator.ValidatorType.InstanceConstructors,
            };
        }

        private struct LocalValidatorInput
        {
            public ITypeSymbol Dto { get; set; }

            public INamedTypeSymbol Class { get; set; }

            public ImmutableArray<IMethodSymbol> ExistingConstructors { get; set; }

            public ImmutableArray<IMethodSymbol> CustomValidationMethods { get; set; }
        }

        private IEnumerable<IPropertySymbol> GetAllProperties(ITypeSymbol symbol)
        {
            foreach (var member in symbol.GetMembers().OfType<IPropertySymbol>())
            {
                yield return member;
            }

            if (symbol.BaseType != null)
            {
                foreach (var member in GetAllProperties(symbol.BaseType))
                {
                    yield return member;
                }
            }
        }

        private IEnumerable<InvocationExpressionSyntax> GetValidatedProperties(SyntaxList<StatementSyntax> statements)
        {
            foreach (var statement in statements)
            {
                var result = GetRuleForInvocation(((ExpressionStatementSyntax)statement).Expression);

                if (result != null)
                {
                    yield return result;
                }
            }
        }

        private InvocationExpressionSyntax? GetRuleForInvocation(ExpressionSyntax syntax)
        {
            switch (syntax)
            {
                case InvocationExpressionSyntax invocationExpressionSyntax:
                    if (invocationExpressionSyntax.Expression is IdentifierNameSyntax identifier && identifier.Identifier.ValueText == "RuleFor")
                    {
                        return invocationExpressionSyntax;
                    }

                    return GetRuleForInvocation(invocationExpressionSyntax.Expression);

                case MemberAccessExpressionSyntax memberAccessExpressionSyntax:
                    return GetRuleForInvocation(memberAccessExpressionSyntax.Expression);
            }

            return null;
        }

        private struct GeneratorInfo
        {
            public INamedTypeSymbol ValidatorClass { get; internal set; }

            public ImmutableArray<(IEnumerable<string> Path, string? Validator, ValidatorLocatorFailureReason? FailureReason)> ValidatorRules { get; internal set; }

            public ImmutableArray<CustomValidationMethodInfo> CustomValidationMethods { get; internal set; }
        }

        private struct CustomValidationMethodInfo
        {
            public IMethodSymbol Method { get; set; }
        }

        private struct IntermediateInfo
        {
            public INamedTypeSymbol TypeSymbol { get; internal set; }

            public INamedTypeSymbol BaseValidator { get; internal set; }
        }

        private struct ValidatorInfo
        {
            public ITypeSymbol DtoType { get; set; }

            public INamedTypeSymbol ValidatorType { get; set; }
        }

        private ValidatorInfo? ExtractValidatorInfo(IntermediateInfo data, CancellationToken cancellationToken)
        {
            if (data.TypeSymbol.BaseType == null)
            {
                return null;
            }

            if (!data.TypeSymbol.BaseType.IsGenericType || data.TypeSymbol.BaseType.IsUnboundGenericType)
            {
                return null;
            }

            if (!SymbolEqualityComparer.Default.Equals(data.TypeSymbol.BaseType.ConstructUnboundGenericType(), data.BaseValidator!.ConstructUnboundGenericType()))
            {
                return null;
            }

            var dtoType = (INamedTypeSymbol?)data.TypeSymbol.BaseType.TypeArguments.FirstOrDefault();

            if (dtoType == null)
            {
                return null;
            }

            return new ValidatorInfo { DtoType = dtoType, ValidatorType = data.TypeSymbol };
        }

        private ImmutableArray<INamedTypeSymbol> GetTypes(Compilation compilation, CancellationToken cancellationToken)
        {
            var result = GetAllTypesInAssembly(compilation.Assembly, cancellationToken).ToImmutableArray();

            return result;
        }

        private IEnumerable<INamedTypeSymbol> GetAllTypesInAssembly(IAssemblySymbol assembly, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!assembly.Modules.First().ReferencedAssemblies.Any(t => t.Name.Contains("FluentValidation")))
            {
                yield break;
            }

            foreach (var type in GetAllTypesInNamespace(assembly.GlobalNamespace, cancellationToken))
            {
                yield return type;
            }

            foreach (var referencedAssembly in assembly.Modules.First().ReferencedAssemblySymbols)
            {
                foreach (var type in GetAllTypesInAssembly(referencedAssembly, cancellationToken))
                {
                    yield return type;
                }
            }
        }

        private IEnumerable<INamedTypeSymbol> GetAllTypesInNamespace(INamespaceSymbol @namespace, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var type in @namespace.GetTypeMembers())
            {
                yield return type;
            }

            foreach (var type in @namespace.GetNamespaceMembers().SelectMany(t => GetAllTypesInNamespace(t, cancellationToken)))
            {
                yield return type;
            }
        }

        private static INamedTypeSymbol? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

            var symbolInfo = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);

            if (symbolInfo is not INamedTypeSymbol type)
            {
                // weird, we couldn't get the symbol, ignore it
                return null;
            }

            var baseType = type.BaseType;

            if (baseType == null)
            {
                return null;
            }

            if (!baseType.IsGenericType)
            {
                return null;
            }

            return type;
        }

        private ValidatorInfo? GetValidator(INamedTypeSymbol abstractValidator, INamedTypeSymbol symbol)
        {
            if (symbol.BaseType == null)
            {
                return null;
            }

            if (!symbol.BaseType.IsGenericType || symbol.BaseType.IsUnboundGenericType)
            {
                return null;
            }

            var unboundBase = symbol.BaseType.ConstructUnboundGenericType();

            if (SymbolEqualityComparer.Default.Equals(unboundBase, abstractValidator.ConstructUnboundGenericType()))
            {
                return new ValidatorInfo { DtoType = symbol.BaseType.TypeArguments.First(), ValidatorType = symbol };
            }

            return null;
        }

        private enum ValidatorLocatorFailureReason
        {
            MultipleValidators,
            UseValidatorNoSymbolSelected,
            UseValidatorDTONotMatching,
            UseValidatorValidatorNotFound
        }
    }
}