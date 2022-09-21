// <copyright file="CSharpSourceGeneratorVerifier`1.cs" company="DevIT in Gründung">
// Copyright (c) DevIT. All rights reserved.
// </copyright>

namespace BA.Roslyn.ClassContextGenerator.Tests.Verifiers
{
	using System;
	using System.Collections.Generic;
	using System.Collections.Immutable;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using BA.Roslyn.ClassContextGenerator.Abstractions;
	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CSharp;
	using Microsoft.CodeAnalysis.CSharp.Testing;
	using Microsoft.CodeAnalysis.Testing;
	using Microsoft.CodeAnalysis.Testing.Verifiers;
	using static Microsoft.CodeAnalysis.Testing.ReferenceAssemblies;

	public class CSharpSourceGeneratorVerifier<TGenerator> where TGenerator : IIncrementalGenerator, new()
	{
		public class Test : SourceGeneratorTest<XUnitVerifier>
		{
			public Test()
			{
				ReferenceAssemblies = Net.Net60;
				TestState.AdditionalReferences.Add(typeof(ApplicationMode).Assembly);

				SolutionTransforms.Add((solution, projectId) =>
				{
					var project = solution.GetProject(projectId);
					var compilationOptions = solution.GetProject(projectId)?.CompilationOptions;
					compilationOptions = compilationOptions?.WithSpecificDiagnosticOptions(
						compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));
					solution = solution.WithProjectCompilationOptions(projectId, compilationOptions!);

					return solution;
				});
			}

            private static readonly LanguageVersion DefaultLanguageVersion =
			Enum.TryParse("Default", out LanguageVersion version) ? version : LanguageVersion.CSharp6;

			protected override IEnumerable<ISourceGenerator> GetSourceGenerators()
				=> new ISourceGenerator[] { new TGenerator().AsSourceGenerator() };

			protected override string DefaultFileExt => "cs";

			public override string Language => LanguageNames.CSharp;

			protected override GeneratorDriver CreateGeneratorDriver(Project project, ImmutableArray<ISourceGenerator> sourceGenerators)
			{
				return CSharpGeneratorDriver.Create(
					sourceGenerators,
					project.AnalyzerOptions.AdditionalFiles,
					(CSharpParseOptions)project.ParseOptions!,
					project.AnalyzerOptions.AnalyzerConfigOptionsProvider);
			}

			protected override CompilationOptions CreateCompilationOptions()
				=> new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true);

			protected override ParseOptions CreateParseOptions()
				=> new CSharpParseOptions(DefaultLanguageVersion, DocumentationMode.Diagnose);
		}
	}
}
