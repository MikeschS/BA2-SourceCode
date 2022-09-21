using BA.Roslyn.Shared;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace BA.Roslyn.AttributeRules
{
    public class GeneratorDiagnostics
    {
        public static readonly DiagnosticDescriptor ApplicationModeNotDefined = new DiagnosticDescriptorBuilder()
            .WithTitle("Application mode not defined")
            .WithCategory("DomainConvention")
            .WithDiagnosticId("CCG001")
            .WithMessageFormat("The class '{0}' uses the class '{1}', but does not define the current application mode.")
            .WithDescription("Class requires specification of the ApplicationMode-Attribute.").Build();
    }
}
