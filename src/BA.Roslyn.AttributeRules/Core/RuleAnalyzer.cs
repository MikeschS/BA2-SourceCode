using BA.Roslyn.AttributeRules.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BA.Roslyn.AttributeRules.Core
{
    public class RuleAnalyzer
    {
        private readonly List<RuleBase> rules;

        internal RuleAnalyzer(IEnumerable<RuleBase> rules)
        {
            this.rules = rules.ToList();
        }

        internal void AnalyzeContext(SymbolAnalysisContext context, SymbolKind symbolKind)
        {
            foreach (var rule in this.rules.Where(t => t.TargetSymbolKind == symbolKind))
            {
                var executionContext = new AttributeRuleExecutionContext() { Compilation = context.Compilation, Symbol = context.Symbol };
                rule.Check(executionContext);

                if (executionContext.Messages.Any())
                {
                    foreach (var message in executionContext.Messages)
                    {
                        var diagnostic = Diagnostic.Create(AttributeDiagnostics.AttributeConventionNotMet, context.Symbol.Locations.FirstOrDefault(), message);

                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}
