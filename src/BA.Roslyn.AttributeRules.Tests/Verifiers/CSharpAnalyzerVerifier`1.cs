namespace BA.Roslyn.AttributeRules.Tests.Verifiers
{
	using System.Reflection;
	using System.Threading;
	using System.Threading.Tasks;
	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CSharp.Testing;
	using Microsoft.CodeAnalysis.Diagnostics;
	using Microsoft.CodeAnalysis.Testing;
	using Microsoft.CodeAnalysis.Testing.Verifiers;

	public static partial class CSharpAnalyzerVerifier<TAnalyzer>
		where TAnalyzer : DiagnosticAnalyzer, new()
	{
		/// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic()"/>
		public static DiagnosticResult Diagnostic()
			=> CSharpAnalyzerVerifier<TAnalyzer, XUnitVerifier>.Diagnostic();

		/// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(string)"/>
		public static DiagnosticResult Diagnostic(string diagnosticId)
			=> CSharpAnalyzerVerifier<TAnalyzer, XUnitVerifier>.Diagnostic(diagnosticId);

		/// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(DiagnosticDescriptor)"/>
		public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
			=> CSharpAnalyzerVerifier<TAnalyzer, XUnitVerifier>.Diagnostic(descriptor);

		/// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.VerifyAnalyzerAsync(string, DiagnosticResult[])"/>
		public static async Task VerifyAnalyzerAsync(IEnumerable<string> sources, IEnumerable<DiagnosticResult>? expected = null, IEnumerable<Assembly>? references = null)
		{
			var test = new Test(sources);

			foreach (var reference in references ?? new Assembly[0])
			{
                test.TestState.AdditionalReferences.Add(reference);
            }


			test.ExpectedDiagnostics.AddRange(expected ?? Array.Empty<DiagnosticResult>());
			await test.RunAsync(CancellationToken.None);
		}
    }
}