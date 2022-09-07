using BA.Roslyn.AttributeRules.Configuration;
using BA.Roslyn.AttributeRules.Core;
using BA.Roslyn.AttributeRules.Core.Rules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
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
			context.EnableConcurrentExecution();

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

				var globals = new ScriptGlobals();
				CSharpScript.EvaluateAsync(text.ToString(), ScriptOptions.Default.WithReferences(typeof(SymbolKind).Assembly).WithImports("System", "Microsoft.CodeAnalysis"), globals: globals).Wait();

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

				var rules = new List<IRule>();

				rules.Add(new CustomRule(globals.SymbolKind, globals.RuleDelegate));

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

				var analyzer = new RuleAnalyzer(rules);

                foreach (var symbolKind in rules.Select(t => t.TargetSymbolKind).Distinct())
                {
					compilationContext.RegisterSymbolAction(ctx => analyzer.AnalyzeContext(ctx, symbolKind), symbolKind);
				}
			});
		}
	}
}
