namespace BA.Roslyn.ClassContextGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using BA.Roslyn.ClassContextGenerator.Abstractions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;

    [Generator]
    public class ApplicationModeGenerator : IIncrementalGenerator
    {
        public const string DiagnosticId = "AM001";
        private const string Category = "DomainConvention";

        private static readonly string title = "MessageUsage must be declared.";

        private static readonly string messageFormat = "Message '{0}' is used in a class that does not describe ist ApplicationType.";
        private static readonly string description = "Message must be used in a class that does not describe ist ApplicationType.";

        private static readonly DiagnosticDescriptor ruleDescription = new DiagnosticDescriptor(DiagnosticId, title, messageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: description);

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var messageReferenceProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, cancellationToken) => GetSemanticTargetForGeneration(ctx, cancellationToken))
                .Where(t => t is not null)
                .Select((t, _) => t!.Value);

            var messageUsagesProvider = messageReferenceProvider
                .Select(GetApplicationModes);

            var messageTypeProvider = context.CompilationProvider
                .SelectMany(GetMessageTypes);

            var resultProvider = messageTypeProvider.Combine(messageUsagesProvider.Collect()).Select((t, cancellationToken) => new GeneratorInput { MessageType = t.Left.MessageType, MessageUsages = t.Right });

            context.RegisterSourceOutput(resultProvider, GenerateOutput);
        }

        private void GenerateOutput(SourceProductionContext context, GeneratorInput input)
        {
            var usages = input.MessageUsages.Where(t => SymbolEqualityComparer.Default.Equals(t.MessageType, input.MessageType)).ToList();

            var missingApplicationTypes = usages.Where(t => t.ApplicationTypes.Length == 0).ToList();

            foreach (var errorType in missingApplicationTypes)
            {
                var diagnostic = Diagnostic.Create(ruleDescription, errorType.UsageClass.Locations.First(), errorType.MessageType.Name);

                context.ReportDiagnostic(diagnostic);
            }

            var usageResult = usages.SelectMany(t => t.ApplicationTypes).Select(t => $"BA.Roslyn.ClassContextGenerator.Abstractions.ApplicationMode.{t}").Distinct().ToList();

            var usageArguments = string.Join(", ", usageResult);

            var sb = new StringBuilder();
            sb.Append(
            $@"namespace {input.MessageType.ContainingNamespace}
{{
	[BA.Roslyn.ClassContextGenerator.Abstractions.ApplicationMode({usageArguments})]
	public partial class {input.MessageType.Name}
	{{
	}}
}}");

            context.AddSource($"{input.MessageType.Name}.g.cs", sb.ToString());
        }

        private MessageUsage GetApplicationModes(MessageReference messageReference, CancellationToken arg2)
        {
            var attributes = messageReference.ContaintingClassTypeSymbol.GetAttributes();
            var atributesArgumentsValues = attributes.Where(t => t.AttributeClass?.Name.StartsWith("ApplicationMode") ?? false).SelectMany(t => t.ConstructorArguments).Where(t => t.Kind == TypedConstantKind.Array).SelectMany(t => t.Values);
            var list = atributesArgumentsValues.Where(t => t.Type.Name == "ApplicationMode")
                .Select(t => (ApplicationMode)t.Value).Where(t => t != null).Distinct().ToArray();

            return new MessageUsage { MessageType = messageReference.ClassType, ApplicationTypes = list, UsageClass = messageReference.ContaintingClassTypeSymbol };
        }

        // TODO rebuild to CascadingError class
        private static MessageReference? GetSemanticTargetForGeneration(GeneratorSyntaxContext ctx, CancellationToken cancellationToken)
        {
            var node = ctx.Node as BaseObjectCreationExpressionSyntax;

            if (node == null)
            {
                throw new InvalidOperationException("Other syntaxes are not supported in this method.");
            }

            var symbolInfo = ctx.SemanticModel.GetSymbolInfo(node, cancellationToken);

            if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            {
                return null;
            }

            var createdType = methodSymbol.ReceiverType as INamedTypeSymbol;

            if (createdType == null)
            {
                throw new InvalidOperationException("Constructors should never return non-named types.");
            }

            if (!HasBase(createdType, "BA.Roslyn.ClassContextGenerator.Abstractions.BaseClass"))
            {
                return null;
            }

            var classDeclaration = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();

            if (classDeclaration == null)
            {
                return null;
            }

            var classDeclarationSymbolInfo = ctx.SemanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken);

            if (classDeclarationSymbolInfo is not INamedTypeSymbol classDeclarationTypeSymbol)
            {
                return null;
            }

            return new MessageReference { ContaintingClassTypeSymbol = classDeclarationTypeSymbol, ClassType = createdType };
        }

        private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        {
            if (node is BaseObjectCreationExpressionSyntax)
            {
                return true;
            }

            return false;
        }

        private ImmutableArray<MessageTypeOutput> GetMessageTypes(Compilation compilation, CancellationToken cancellationToken)
        {
            var types = GetAllTypesInNamespace(compilation.Assembly.GlobalNamespace);

            var output = types.Where(t => HasBase(t, "BA.Roslyn.ClassContextGenerator.Abstractions.BaseClass")).Select(t => new MessageTypeOutput { MessageType = t });

            return output.ToImmutableArray();
        }

        private IEnumerable<INamedTypeSymbol> GetAllTypesInNamespace(INamespaceSymbol @namespace)
        {
            foreach (var type in @namespace.GetTypeMembers())
            {
                yield return type;
            }

            foreach (var type in @namespace.GetNamespaceMembers().SelectMany(t => GetAllTypesInNamespace(t)))
            {
                yield return type;
            }
        }

        private static bool HasBase (ITypeSymbol typeSymbol, string symbolDisplayName)
        {
            if (typeSymbol == null)
            {
                return false;
            }

            var current = typeSymbol.BaseType;

            while(current != null)
            {
                if (current.ToDisplayString() == symbolDisplayName)
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }

        private struct MessageReference
        {
            public INamedTypeSymbol ClassType { get; set; }

            public INamedTypeSymbol ContaintingClassTypeSymbol { get; set; }
        }

        private struct MessageUsage
        {
            public INamedTypeSymbol MessageType { get; set; }

            public INamedTypeSymbol UsageClass { get; set; }

            public ApplicationMode[] ApplicationTypes { get; set; }
        }

        private struct GeneratorInput
        {
            public INamedTypeSymbol MessageType { get; set; }

            public ImmutableArray<MessageUsage> MessageUsages { get; set; }
        }

        private struct MessageTypeOutput
        {
            public INamedTypeSymbol MessageType { get; set; }
        }
    }
}