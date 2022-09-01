using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace BA.Roslyn.AttributeRules
{
    internal class DiagnosticDescriptorBuilder
    {
        private string? DiagnosticId { get; set; }

        private string? Category { get; set; }

        private string? Title { get; set; }

        private string? MessageFormat { get; set; }

        private string? Description { get; set; }

        internal DiagnosticDescriptorBuilder WithDiagnosticId(string id)
        {
            DiagnosticId = id;
            return this;
        }

        internal DiagnosticDescriptorBuilder WithCategory(string category)
        {
            Category = category;
            return this;
        }

        internal DiagnosticDescriptorBuilder WithTitle(string title)
        {
            Title = title;
            return this;
        }

        internal DiagnosticDescriptorBuilder WithMessageFormat(string messageFormat)
        {
            MessageFormat = messageFormat;
            return this;
        }

        internal DiagnosticDescriptorBuilder WithDescription(string description)
        {
            Description = description;
            return this;
        }

        internal DiagnosticDescriptor Build()
        {
            if( DiagnosticId == null ||
                Category == null ||
                Title == null ||
                MessageFormat == null)
            {
                throw new InvalidOperationException("Configuration is missing");
            }

            return new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);
        }
    }
}
