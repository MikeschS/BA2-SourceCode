using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace BA.Roslyn.AttributeRules
{
    public class ScriptGlobals
    {
        public SymbolKind SymbolKind { get; set; }

        public Action<AttributeRuleExecutionContext> RuleDelegate { get; set; }
    }

    public class AttributeRuleExecutionContext
    {
        internal List<string> Messages { get; set; } = new List<string>();

        public ISymbol Symbol { get; set; }
        public Compilation Compilation { get; set; }

        public void EmitMessage(string message)
        {
            Messages.Add(message);
        }
    }
}
