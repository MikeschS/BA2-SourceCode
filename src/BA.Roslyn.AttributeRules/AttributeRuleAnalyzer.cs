using BA.Roslyn.AttributeRules.Abstractions;
using BA.Roslyn.AttributeRules.Configuration;
using BA.Roslyn.AttributeRules.Core;
using BA.Roslyn.AttributeRules.Core.Rules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace BA.Roslyn.AttributeRules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AttributeRuleAnalyzer : DiagnosticAnalyzer
    {
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get
			{
				return ImmutableArray.Create(new[] { AttributeDiagnostics.AttributeConventionNotMet, AttributeDiagnostics.InvalidRuleConfigJson, AttributeDiagnostics.MissingRuleConfigFile, AttributeDiagnostics.InvalidRuleConfig, AttributeDiagnostics.MissingRuleConfigText });
			}
		}

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
			////context.EnableConcurrentExecution();

            // TODO notify if script is valid context.RegisterAdditionalFileAction

			context.RegisterCompilationStartAction(compilationContext =>
			{
                var attribute = compilationContext.Compilation.Assembly.GetAttributes().First();

                var identity = attribute.AttributeClass.ContainingAssembly.Identity;
                var idName = identity.GetDisplayName();
                var assembly = Assembly.Load(idName);

                var metadataName = attribute.AttributeClass.ToDisplayString();

                var type = assembly.ExportedTypes.FirstOrDefault(t => t.FullName == metadataName);

                var rule = (RuleBase)Activator.CreateInstance(type, Array.Empty<object>());

                var rules = new List<RuleBase>();

                rules.Add(rule);

                foreach (var rule2 in rules)
                {
                    var executionContext = new AttributeRuleInitialisationContext() { Compilation = compilationContext.Compilation };
                    rule2.Init(executionContext);

                    if (executionContext.Messages.Any())
                    {
                        foreach (var message in executionContext.Messages)
                        {
                            // TODO implement proper diagnostic
                            ////var diagnostic = Diagnostic.Create(AttributeDiagnostics.AttributeConventionNotMet, context.Symbol.Locations.FirstOrDefault(), message);

                            ////compilationContext.RegisterCompilationEndAction(context => context.ReportDiagnostic(diagnostic));
                        }
                    }
                }

                var analyzer = new RuleAnalyzer(rules);

                foreach (var symbolKind in rules.Select(t => t.TargetSymbolKind).Distinct())
                {
					compilationContext.RegisterSymbolAction(ctx => analyzer.AnalyzeContext(ctx, symbolKind), symbolKind);
				}
			});
		}

        private IEnumerable<IAssemblySymbol> GetDependentAssemblies(IEnumerable<IAssemblySymbol> assemblies)
        {
            foreach (var item in assemblies)
            {
                var references = item.Modules.SelectMany(t => t.ReferencedAssemblySymbols);

                if (references.Any(t => t.Name.Contains(typeof(RuleBase).Assembly.GetName().Name)))
                {
                    yield return item;
                }
            }
        }
	}
}
