// <copyright file="TenantAdminRequirementTests.cs" company="DevIT in Gründung">
// Copyright (c) DevIT. All rights reserved.
// </copyright>

namespace Powermatch2.Application.Analyzers.Test
{
    using System.ComponentModel;
    using System.Threading.Tasks;
    using BA.Roslyn.AttributeRules;
    using BA.Roslyn.AttributeRules.Tests.Verifiers;
	using Xunit;

	public class Tests
	{
		private readonly string attributeSource =
@"namespace AttributeRules.Test
{
	using System;
	using System.Threading.Tasks;

	public class RequiredAttribute : Attribute
	{
	}
}";		
		private readonly string baseSource =
@"namespace AttributeRules.Test
{
	using System;
	using System.Threading.Tasks;

	public class MyCommand
	{
	}
}";

		[Fact]
		public async Task AcceptsCorrectConfig()
		{
			AdditionalDocument config = new AdditionalDocument("attributeRules.json",
@"{
	""Rules"" : {
		""TestRule"" : {
			""Type"" : ""BaseClass"",
			""AttributeTypeName"" : ""AttributeRules.Test.RequiredAttribute"",
			""BaseClassTypeName"" : ""AttributeRules.Test.MyCommand""
		}
	}
}");

			await CSharpAnalyzerVerifier<AttributeRuleAnalyzer>.VerifyAnalyzerAsync(
				new string[] { attributeSource, baseSource }, 
				config);
		}

		[Fact]
		public async Task ConfigFileDoesNotExist()
		{
			await CSharpAnalyzerVerifier<AttributeRuleAnalyzer>.VerifyAnalyzerAsync(
				new string[] { attributeSource, baseSource }, 
				null,
				CSharpAnalyzerVerifier<AttributeRuleAnalyzer>.Diagnostic(AttributeDiagnostics.MissingRuleConfigFile));
		}

		[Fact]
		public async Task ConfigFileInvalidName()
		{
			AdditionalDocument config = new AdditionalDocument("attributeRuless.json",
@"{
	""Rules"" : {
		""TestRule"" : {
			""Type"" : ""BaseClass"",
			""AttributeTypeName"" : ""AttributeRules.Test.RequiredAttribute"",
			""BaseClassTypeName"" : ""AttributeRules.Test.MyCommand""
		}
	}
}");

			await CSharpAnalyzerVerifier<AttributeRuleAnalyzer>.VerifyAnalyzerAsync(
				new string[] { attributeSource, baseSource },
				config,
				CSharpAnalyzerVerifier<AttributeRuleAnalyzer>.Diagnostic(AttributeDiagnostics.MissingRuleConfigFile));
		}

		[Fact]
		public async Task InvalidJson()
		{
			AdditionalDocument config = new AdditionalDocument("attributeRules.json",
@"{
	""Rules"" : {
		""TestRule"" : {
			""Type"" : ""BaseClass"",
			""AttributeTypeName"" : ""AttributeRules.Test.RequiredAttribute""
	}
}");

			await CSharpAnalyzerVerifier<AttributeRuleAnalyzer>.VerifyAnalyzerAsync(
				new string[] { attributeSource, baseSource },
				config,
				CSharpAnalyzerVerifier<AttributeRuleAnalyzer>.Diagnostic(AttributeDiagnostics.InvalidRuleConfigJson).WithArguments("Expected depth to be zero at the end of the JSON payload. There is an open JSON object or array that should be closed. Path: $ | LineNumber: 6 | BytePositionInLine: 1."));
		}
	}
}