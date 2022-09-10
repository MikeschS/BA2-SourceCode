// <copyright file="CSharpAnalyzerVerifier`1+Test.cs" company="DevIT in Gründung">
// Copyright (c) DevIT. All rights reserved.
// </copyright>

namespace BA.Roslyn.AttributeRules.Tests.Verifiers
{
	using System.Collections.Immutable;
	using System.Linq;
    using System.Text;
    using System.Threading;
	using System.Threading.Tasks;
    using BA.Roslyn.AttributeRules.Abstractions;
    using BA.Roslyn.AttributeRules.Tests.Rules;
    using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CSharp.Testing;
	using Microsoft.CodeAnalysis.Diagnostics;
	using Microsoft.CodeAnalysis.Testing;
	using Microsoft.CodeAnalysis.Testing.Model;
	using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Microsoft.CodeAnalysis.Text;
    using static Microsoft.CodeAnalysis.Testing.ReferenceAssemblies;

	public static partial class CSharpAnalyzerVerifier<TAnalyzer>
		where TAnalyzer : DiagnosticAnalyzer, new()
	{
		public class Test : CSharpAnalyzerTest<TAnalyzer, XUnitVerifier>
		{
			public Test(IEnumerable<string> sources)
			{
				ReferenceAssemblies = Net.Net60;
                TestState.AdditionalReferences.Add(typeof(RuleBase).Assembly);

                foreach (var source in sources)
                {
					TestState.Sources.Add(source);
                }

				SolutionTransforms.Add((solution, projectId) =>
				{
					var project = solution.GetProject(projectId);
					if (project != null)
					{
						var compilationOptions = project.CompilationOptions;
						if (compilationOptions != null)
						{
							compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(
								compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));
							solution = solution.WithProjectCompilationOptions(projectId, compilationOptions);
						}
					}

					return solution;
				});
			}
		}
	}

	public record AdditionalDocument(string Name, string Source);
}