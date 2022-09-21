using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace BA.Roslyn.Shared
{
    public class DiagnosticDescriptorBuilder
    {
        private string DiagnosticId { get; set; }

        private string Category { get; set; }

        private string Title { get; set; }

        private string MessageFormat { get; set; }

        private string Description { get; set; }

        private DiagnosticSeverity Severity { get; set; } = DiagnosticSeverity.Warning;

        public DiagnosticDescriptorBuilder WithDiagnosticId(string id)
        {
            DiagnosticId = id;
            return this;
        }

        public DiagnosticDescriptorBuilder WithCategory(string category)
        {
            Category = category;
            return this;
        }

        public DiagnosticDescriptorBuilder WithTitle(string title)
        {
            Title = title;
            return this;
        }

        public DiagnosticDescriptorBuilder WithMessageFormat(string messageFormat)
        {
            MessageFormat = messageFormat;
            return this;
        }

        public DiagnosticDescriptorBuilder WithDescription(string description)
        {
            Description = description;
            return this;
        }

        public DiagnosticDescriptorBuilder WithSeverity(DiagnosticSeverity severity)
        {
            Severity = severity;
            return this;
        }

        public DiagnosticDescriptor Build()
        {
            if (DiagnosticId == null ||
                Category == null ||
                Title == null ||
                MessageFormat == null)
            {
                throw new InvalidOperationException("Configuration is missing");
            }

            return new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, Severity, isEnabledByDefault: true, description: Description);
        }
    }
}
