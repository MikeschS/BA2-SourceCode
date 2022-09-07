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
                var file = compilationContext.Options.AdditionalFiles.FirstOrDefault(t => Path.GetFileName(t.Path) == "attributeRules.json");

                if (file == null)
                {
					var missingFileDiagnostic = Diagnostic.Create(AttributeDiagnostics.MissingRuleConfigFile, null);
					compilationContext.RegisterCompilationEndAction(context => context.ReportDiagnostic(missingFileDiagnostic));

                    return;
                }

                var text = file.GetText(compilationContext.CancellationToken);

                if (text == null)
                {
					var missingRuleConfigText = Diagnostic.Create(AttributeDiagnostics.MissingRuleConfigText, null);
					compilationContext.RegisterCompilationEndAction(context => context.ReportDiagnostic(missingRuleConfigText));

					return;
                }

                var rules = new List<IRule>();

                try
                {
                    var references = new List<string>();
                    var globals = new ScriptGlobals(reference => references.Add(reference), rule => rules.Add(rule));
                    ////CSharpScript.EvaluateAsync(text.ToString(), ScriptOptions.Default.WithReferences(typeof(SymbolKind).Assembly, typeof(ScriptGlobals).Assembly).WithImports("System", "Microsoft.CodeAnalysis", "BA.Roslyn.AttributeRules.Core.Rules", "BA.Roslyn.AttributeRules"), globals: globals).Wait();

                    ////var allReferences = compilationContext.Compilation.Assembly.Modules.SelectMany(t => t.ReferencedAssemblySymbols);

                    ////var metadata = compilationContext.Compilation.GetMetadataReference(allReferences.First());

                    // TODO get all with dependency on abstractions

                    var ruleType = typeof(IRule).Assembly.GetName().Name;
                    ////compilationContext.Compilation.Assembly.Modules.SelectMany(t => t.ReferencedAssemblySymbols).First().Modules.SelectMany(t => t.ReferencedAssemblySymbols);

                    var dependentReferences = GetDependentAssemblies(compilationContext.Compilation.Assembly.Modules.SelectMany(t => t.ReferencedAssemblySymbols));

                    var targets = dependentReferences.Select(t => compilationContext.Compilation.GetMetadataReference(t)).ToList();

                    var input = text.Lines.Select(t => t.ToString()).ToList();

                    ScriptState? state = null;
                    foreach (var line in input)
                    {
                        // var tempReferences = compilationContext.Compilation.References.Where(t => references.Any(r => t.Display?.Contains(r) ?? false)).ToArray();

                        if (state == null)
                        {
                            state = CSharpScript.RunAsync(line, ScriptOptions.Default.AddReferences(targets).AddReferences(typeof(SymbolKind).Assembly, typeof(ScriptGlobals).Assembly, typeof(IRule).Assembly).AddImports("System", "Microsoft.CodeAnalysis", "BA.Roslyn.AttributeRules"), globals: globals).Result;
                        }
                        else
                        {
                            state = state.ContinueWithAsync(line, ScriptOptions.Default.AddReferences(targets).AddReferences(typeof(SymbolKind).Assembly, typeof(ScriptGlobals).Assembly, typeof(IRule).Assembly).AddImports("System", "Microsoft.CodeAnalysis", "BA.Roslyn.AttributeRules")).Result;
                        }
                    }

                    // CSharpScript.EvaluateAsync(text.ToString(), ScriptOptions.Default.AddReferences(compilationContext.Compilation.References.Where(t => t.Display.Contains("Rule"))).AddReferences(typeof(SymbolKind).Assembly, typeof(ScriptGlobals).Assembly).AddImports("System", "Microsoft.CodeAnalysis", "BA.Roslyn.AttributeRules"), globals: globals).Wait();
                }
                catch (Exception e)
                {
                    // TODO diagnostic for exception
                }

                // TODO check if globals are provided!!
                ////var options = new JsonSerializerOptions(JsonSerializerDefaults.General);
                ////options.Converters.Add(new RuleJsonConverter());


                ////AttributeRulesConfig? config = null; 

                ////try
                ////            {
                ////	config = JsonSerializer.Deserialize<AttributeRulesConfig>(text.ToString(), options);
                ////}
                ////catch (JsonException e)
                ////            {
                ////	var jsonExceptionDiagnostic = Diagnostic.Create(AttributeDiagnostics.InvalidRuleConfigJson, null, e.Message);
                ////	compilationContext.RegisterCompilationEndAction(context => context.ReportDiagnostic(jsonExceptionDiagnostic));
                ////	return;
                ////}

                ////            if (config == null)
                ////            {
                ////	throw new InvalidOperationException("Config is null - this should not happen");
                ////            }

                ////rules.Add(new CustomRule(globals.SymbolKind, globals.RuleDelegate));

                ////            foreach (var ruleConfig in config.Rules)
                ////            {
                ////	var configBuilderResult = ruleConfig.Value.BuildRule(ruleConfig.Key, compilationContext.Compilation);

                ////	if (configBuilderResult.Error != null)
                ////                {
                ////		var missingFileDiagnostic = Diagnostic.Create(AttributeDiagnostics.InvalidRuleConfig, null, configBuilderResult.Error);
                ////		compilationContext.RegisterCompilationEndAction(context => context.ReportDiagnostic(missingFileDiagnostic));

                ////		continue;
                ////	}

                ////	if (configBuilderResult.Rule == null)
                ////                {
                ////		throw new InvalidOperationException();
                ////                }

                ////	rules.Add(configBuilderResult.Rule);
                ////}
                ///

                foreach (var rule in rules)
                {
                    var executionContext = new AttributeRuleInitialisationContext() { Compilation = compilationContext.Compilation };
                    rule.Init(executionContext);

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

                if (references.Any(t => t.Name.Contains(typeof(IRule).Assembly.GetName().Name)))
                {
                    yield return item;
                }
            }
        }
	}
}
