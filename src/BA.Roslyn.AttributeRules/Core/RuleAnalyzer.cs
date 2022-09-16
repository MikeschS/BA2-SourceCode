using BA.Roslyn.AttributeRules.Core.Rules;
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
        private readonly List<IRule> rules;

        internal RuleAnalyzer(IEnumerable<IRule> rules)
        {
            this.rules = rules.ToList();
        }

        internal void AnalyzeContext(SymbolAnalysisContext context, SymbolKind symbolKind)
        {
            // TODO check for cancellationtoken
            foreach (var rule in this.rules.Where(t => t.TargetSymbolKind == symbolKind))
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                var result = rule.Check(context.Symbol);

                if (!result.IsSuccess)
                {
                    var diagnostic = Diagnostic.Create(AttributeDiagnostics.AttributeConventionNotMet, context.Symbol.Locations.FirstOrDefault(), result.ErrorMessage);

                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
