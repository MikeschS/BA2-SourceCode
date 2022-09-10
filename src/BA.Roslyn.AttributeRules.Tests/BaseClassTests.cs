// <copyright file="TenantAdminRequirementTests.cs" company="DevIT in Gründung">
// Copyright (c) DevIT. All rights reserved.
// </copyright>

namespace Powermatch2.Application.Analyzers.Test
{
    using System.ComponentModel;
	using System.Threading.Tasks;
    using BA.Roslyn.AttributeRules;
	using BA.Roslyn.AttributeRules.Tests.Rules;
	using BA.Roslyn.AttributeRules.Tests.Verifiers;
	using Microsoft.CodeAnalysis.Testing;
	using Xunit;

	public class BaseClassTests
	{
        [Fact]
        public async Task ZZZZLastTest()
        {
            var testCase =
@"namespace AttributeRules.Test
{
	using System;
	using System.Threading.Tasks;

	public class TestCommand
	{
	}
}";

			var assemblyAttribute =
@"using BA.Roslyn.AttributeRules.Tests.Rules;
[assembly: ZeroParamRule()]
";

			await CSharpAnalyzerVerifier<AttributeRuleAnalyzer>.VerifyAnalyzerAsync(
			new string[] { testCase, assemblyAttribute },new DiagnosticResult[] { CSharpAnalyzerVerifier<AttributeRuleAnalyzer> .Diagnostic(AttributeDiagnostics.AttributeConventionNotMet).WithSpan(6, 15, 6, 26).WithArguments("ZeroParamRule") }, new[] { typeof(MyCustomRule).Assembly } );
        }
    }
}