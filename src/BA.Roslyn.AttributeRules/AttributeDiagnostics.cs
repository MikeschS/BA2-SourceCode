using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace BA.Roslyn.AttributeRules
{
    public class AttributeDiagnostics
    {
        public static readonly DiagnosticDescriptor AttributeConventionNotMet = new DiagnosticDescriptorBuilder()
            .WithTitle("Attribute convention not met")
            .WithCategory("AttributeConvention")
            .WithDiagnosticId("Att001")
            .WithMessageFormat("The attribute convention is not met: {0}")
            .WithDescription("Attribute convention is not met").Build();

        public static readonly DiagnosticDescriptor InvalidRuleConfigJson = new DiagnosticDescriptorBuilder()
            .WithTitle("Invalid json in rule config")
            .WithCategory("Configuration")
            .WithDiagnosticId("Att002")
            .WithMessageFormat("The json configuration for the attribute convention rules could not be deserialized: {0}")
            .WithDescription("The deserialization of the configuration for the attribute convention rules has encountered an error").Build();

        public static readonly DiagnosticDescriptor MissingRuleConfigFile = new DiagnosticDescriptorBuilder()
            .WithTitle("Missing rule config file")
            .WithCategory("Configuration")
            .WithDiagnosticId("Att003")
            .WithMessageFormat("The configuration file for the attribute convention is not found")
            .WithDescription("The configuration file for the attribute convention rules is not found").Build();

        public static readonly DiagnosticDescriptor MissingRuleConfigText = new DiagnosticDescriptorBuilder()
            .WithTitle("Missing rule config text")
            .WithCategory("Configuration")
            .WithDiagnosticId("Att004")
            .WithMessageFormat("The configuration text for the attribute convention is not found")
            .WithDescription("The configuration text for the attribute convention rules is not found").Build();

        public static readonly DiagnosticDescriptor InvalidRuleConfig = new DiagnosticDescriptorBuilder()
            .WithTitle("Invalid rule config")
            .WithCategory("Configuration")
            .WithDiagnosticId("Att005")
            .WithMessageFormat("The configuration for the attribute convention rules is invalid: {0}")
            .WithDescription("The configuration for the attribute convention rules has encountered an error").Build();
    }
}
