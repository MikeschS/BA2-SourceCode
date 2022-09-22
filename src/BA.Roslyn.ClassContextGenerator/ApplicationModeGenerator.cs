namespace BA.Roslyn.ClassContextGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using BA.Roslyn.AttributeRules;
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
                predicate: static (s, _) => IsSyntaxForTargetUsage(s),
                transform: static (ctx, cancellationToken) => GetSemanticTargetForTargetUsage(ctx, cancellationToken))
                .Where(t => t is not null)
                .Select((t, _) => t!.Value).WithComparer(new ClassContextTypesComparer());

            var messageUsagesProvider = messageReferenceProvider
                .Select(GetApplicationModes).WithComparer(new ClassContextUsageComparer());

            var messageTypeProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxForType(s),
                transform: static (ctx, cancellationToken) => GetSemanticTargetForTargetType(ctx, cancellationToken))
                .Where(t => t is not null)
                .Select((t, _) => t!.Value).WithComparer(new MessageTypeOutputComparer());

            var resultProvider = messageTypeProvider.Combine(messageUsagesProvider.Collect()).Select((t, cancellationToken) => new GeneratorInput { ClassType = t.Left.MessageType, MessageUsages = t.Right });

            context.RegisterSourceOutput(resultProvider, GenerateOutput);
        }

        private void GenerateOutput(SourceProductionContext context, GeneratorInput input)
        {
            var usages = input.MessageUsages.Where(t => SymbolEqualityComparer.Default.Equals(t.ClassType, input.ClassType)).ToList();

            var missingApplicationTypes = usages.Where(t => t.ApplicationTypes.Length == 0).ToList();

            foreach (var errorType in missingApplicationTypes)
            {
                var diagnostic = Diagnostic.Create(GeneratorDiagnostics.ApplicationModeNotDefined, errorType.ContainingClass.Locations.First(), errorType.ContainingClass.Name, errorType.ClassType.Name);

                context.ReportDiagnostic(diagnostic);
            }

            var usageResult = usages.SelectMany(t => t.ApplicationTypes).Select(t => $"BA.Roslyn.ClassContextGenerator.Abstractions.ApplicationMode.{t}").Distinct().ToList();

            var usageArguments = string.Join(", ", usageResult);

            var sb = new StringBuilder();
            sb.Append(
            $@"namespace {input.ClassType.ContainingNamespace}
{{
	[BA.Roslyn.ClassContextGenerator.Abstractions.ApplicationMode({usageArguments})]
	public partial class {input.ClassType.Name}
	{{
	}}
}}");

            context.AddSource($"{input.ClassType.Name}.g.cs", sb.ToString());
        }

        private ClassContextUsage GetApplicationModes(ClassContextTypes messageReference, CancellationToken arg2)
        {
            var attributes = messageReference.ContaintingClassTypeSymbol.GetAttributes();
            var atributesArgumentsValues = attributes.Where(t => t.AttributeClass?.Name.StartsWith("ApplicationMode") ?? false).SelectMany(t => t.ConstructorArguments).Where(t => t.Kind == TypedConstantKind.Array).SelectMany(t => t.Values);
            var list = atributesArgumentsValues.Where(t => t.Type.Name == "ApplicationMode")
                .Where(t => t.Value != null).Select(t => (ApplicationMode)t.Value!).Distinct().ToArray();

            return new ClassContextUsage { ClassType = messageReference.ClassType, ApplicationTypes = list, ContainingClass = messageReference.ContaintingClassTypeSymbol };
        }

        private static ClassContextTypes? GetSemanticTargetForTargetUsage(GeneratorSyntaxContext ctx, CancellationToken cancellationToken)
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
                throw new InvalidOperationException("Constructors should never have null ReceiverTypes.");
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

            return new ClassContextTypes { ContaintingClassTypeSymbol = classDeclarationTypeSymbol, ClassType = createdType };
        }

        private static MessageTypeOutput? GetSemanticTargetForTargetType(GeneratorSyntaxContext ctx, CancellationToken cancellationToken)
        {
            var node = ctx.Node as ClassDeclarationSyntax;

            if (node == null)
            {
                throw new InvalidOperationException("Other syntaxes are not supported in this method.");
            }

            var symbol = ctx.SemanticModel.GetDeclaredSymbol(node, cancellationToken);

            if (symbol is not INamedTypeSymbol typeSymbol)
            {
                return null;
            }

            if(!HasBase(typeSymbol, "BA.Roslyn.ClassContextGenerator.Abstractions.BaseClass"))
            {
                return null;
            }

            return new MessageTypeOutput { MessageType = typeSymbol };
        }

        private static bool IsSyntaxForTargetUsage(SyntaxNode node)
        {
            if (node is BaseObjectCreationExpressionSyntax)
            {
                return true;
            }

            return false;
        }

        private static bool IsSyntaxForType(SyntaxNode node)
        {
            if (node is ClassDeclarationSyntax)
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

        private struct ClassContextTypes
        {
            public INamedTypeSymbol ClassType { get; set; }

            public INamedTypeSymbol ContaintingClassTypeSymbol { get; set; }
        }

        private class ClassContextTypesComparer : EqualityComparer<ClassContextTypes>
        {
            public override bool Equals(ClassContextTypes x, ClassContextTypes y)
            {
                if (!x.ContaintingClassTypeSymbol.ToDisplayString().Equals(y.ContaintingClassTypeSymbol.ToDisplayString()))
                {
                    return false;
                }

                if (!x.ClassType.ToDisplayString().Equals(y.ClassType.ToDisplayString()))
                {
                    return false;
                }

                return true;
            }

            public override int GetHashCode(ClassContextTypes obj) => obj.GetHashCode();
        }

        private struct ClassContextUsage
        {
            public INamedTypeSymbol ClassType { get; set; }

            public INamedTypeSymbol ContainingClass { get; set; }

            public ApplicationMode[] ApplicationTypes { get; set; }
        }

        private class ClassContextUsageComparer : EqualityComparer<ClassContextUsage>
        {
            public override bool Equals(ClassContextUsage x, ClassContextUsage y)
            {
                if (!x.ApplicationTypes.SequenceEqual(y.ApplicationTypes))
                {
                    return false;
                }

                if (!x.ContainingClass.ToDisplayString().Equals(y.ContainingClass.ToDisplayString()))
                {
                    return false;
                }

                if (!x.ClassType.ToDisplayString().Equals(y.ClassType.ToDisplayString()))
                {
                    return false;
                }

                return true;
            }

            public override int GetHashCode(ClassContextUsage obj) => obj.GetHashCode();
        }

        private struct GeneratorInput
        {
            public INamedTypeSymbol ClassType { get; set; }

            public ImmutableArray<ClassContextUsage> MessageUsages { get; set; }
        }

        private struct MessageTypeOutput
        {
            public INamedTypeSymbol MessageType { get; set; }
        }

        private class MessageTypeOutputComparer : EqualityComparer<MessageTypeOutput>
        {
            public override bool Equals(MessageTypeOutput x, MessageTypeOutput y)
            {
                if (!x.MessageType.ToDisplayString().Equals(y.MessageType.ToDisplayString()))
                {
                    return false;
                }

                return true;
            }

            public override int GetHashCode(MessageTypeOutput obj) => obj.GetHashCode();
        }
    }
}