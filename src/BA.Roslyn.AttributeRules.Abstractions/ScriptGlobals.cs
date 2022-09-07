using BA.Roslyn.AttributeRules.Abstractions;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace BA.Roslyn.AttributeRules
{
    public class ScriptGlobals
    {
        private readonly Action<string> addReference;
        private readonly Action<IRule> addRule;

        public ScriptGlobals(Action<string> addReference, Action<IRule> addRule)
        {
            this.addReference = addReference;
            this.addRule = addRule;
        }

        public void AddReference(string reference) => addReference(reference);

        public void AddRule(IRule rule) => addRule(rule);
    }

    public class AttributeRuleExecutionContext
    {
        // TODO emit by callback
        public List<string> Messages { get; set; } = new List<string>();

        public ISymbol Symbol { get; set; }

        public Compilation Compilation { get; set; }

        public void EmitMessage(string message)
        {
            Messages.Add(message);
        }
    }

    public class AttributeRuleInitialisationContext
    {
        // TODO emit by callback
        public List<string> Messages { get; set; } = new List<string>();

        public Compilation Compilation { get; set; }

        public void EmitMessage(string message)
        {
            Messages.Add(message);
        }
    }
}
