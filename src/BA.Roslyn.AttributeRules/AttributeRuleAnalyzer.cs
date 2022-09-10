using BA.Roslyn.AttributeRules.Abstractions;
using BA.Roslyn.AttributeRules.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

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
			// context.EnableConcurrentExecution();

            context.RegisterCompilationAction(context => {
                var diagnostic = Diagnostic.Create(AttributeDiagnostics.AttributeConventionNotMet, null, "Test");
                context.ReportDiagnostic(diagnostic);
            });

            context.RegisterCompilationAction(context => {
                var found = AppDomain.CurrentDomain.GetAssemblies().Any(t => t.FullName.Contains("AttributeRules.Abstractions"));
                var diagnostic = Diagnostic.Create(AttributeDiagnostics.AttributeConventionNotMet, null, found ? "exists" : "dontexists");
                context.ReportDiagnostic(diagnostic);

                if (!found)
                {
                    var baseAssembly = Assembly.Load(typeof(RuleBase).Assembly.GetName());

                    var diagnostic2 = Diagnostic.Create(AttributeDiagnostics.AttributeConventionNotMet, null, baseAssembly == null ? "null" : "notnull");
                    context.ReportDiagnostic(diagnostic2);
                }
            });

            // TODO notify if script is valid context.RegisterAdditionalFileAction
            
   ////         context.RegisterCompilationStartAction(compilationContext =>
			////{
   ////             context.RegisterCompilationAction(context => {
   ////                 var diagnostic = Diagnostic.Create(AttributeDiagnostics.AttributeConventionNotMet, null, "Step1");
   ////                 context.ReportDiagnostic(diagnostic);
   ////             });

   ////             var allAttributes = compilationContext.Compilation.Assembly.GetAttributes().ToList();
   ////             var attribute = allAttributes.First();

   ////             if (attribute.AttributeClass?.BaseType?.Name != "RuleBase")
   ////             {
   ////                 return;
   ////             }

   ////             var identity = attribute.AttributeClass.ContainingAssembly.Identity;
   ////             var idName = identity.GetDisplayName();

   ////             var reference = compilationContext.Compilation.References.Where(t => t.Display?.Contains(identity.Name) ?? false).FirstOrDefault();

   ////             context.RegisterCompilationAction(context => {
   ////                     var diagnostic = Diagnostic.Create(AttributeDiagnostics.AttributeConventionNotMet, null, "Entered");
   ////                     context.ReportDiagnostic(diagnostic);
   ////                 });

   ////             // var abstractions = Assembly.Load(typeof(RuleBase).Assembly.GetName());

   ////             context.RegisterCompilationAction(context => {
   ////                 var diagnostic = Diagnostic.Create(AttributeDiagnostics.AttributeConventionNotMet, null, idName);
   ////                 context.ReportDiagnostic(diagnostic);
   ////             });
   ////             var assembly = Assembly.LoadFile(reference.Display);

   ////             context.RegisterCompilationAction(context => {
   ////                 var diagnostic = Diagnostic.Create(AttributeDiagnostics.AttributeConventionNotMet, null, assembly == null ? "null" : "notnull");
   ////                 context.ReportDiagnostic(diagnostic);
   ////             });

   ////             // TODO load referenced assemblies!!


   ////             var metadataName = attribute.AttributeClass.ToDisplayString();

   ////             var type = assembly.ExportedTypes.FirstOrDefault(t => t.FullName == metadataName);

   ////             var namedArguments = attribute.NamedArguments.ToList();
   ////             // TODO handle arrays
   ////             var parameters = attribute.ConstructorArguments.Select(t => t.Value).ToArray();
   ////             var rule = (RuleBase)Activator.CreateInstance(type, (object[])parameters);

   ////             var rules = new List<RuleBase>();

   ////             rules.Add(rule);

   ////             foreach (var rule2 in rules)
   ////             {
   ////                 var executionContext = new AttributeRuleInitialisationContext() { Compilation = compilationContext.Compilation };
   ////                 rule2.Init(executionContext);

   ////                 if (executionContext.Messages.Any())
   ////                 {
   ////                     foreach (var message in executionContext.Messages)
   ////                     {
   ////                         // TODO implement proper diagnostic
   ////                         ////var diagnostic = Diagnostic.Create(AttributeDiagnostics.AttributeConventionNotMet, context.Symbol.Locations.FirstOrDefault(), message);

   ////                         ////compilationContext.RegisterCompilationEndAction(context => context.ReportDiagnostic(diagnostic));
   ////                     }
   ////                 }
   ////             }

   ////             var analyzer = new RuleAnalyzer(rules);

   ////             foreach (var symbolKind in rules.Select(t => t.TargetSymbolKind).Distinct())
   ////             {
			////		compilationContext.RegisterSymbolAction(ctx => analyzer.AnalyzeContext(ctx, symbolKind), symbolKind);
			////	}
			////});
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
