﻿// <copyright file="TenantAdminRequirementTests.cs" company="DevIT in Gründung">
// Copyright (c) DevIT. All rights reserved.
// </copyright>

namespace Powermatch2.Application.Analyzers.Test
{
    using System.ComponentModel;
    using System.Threading.Tasks;
    using BA.Roslyn.AttributeRules;
    using BA.Roslyn.AttributeRules.Tests.Verifiers;
	using Xunit;

	public class BaseClassTests
	{
		private readonly AdditionalDocument config = new AdditionalDocument("attributeRules.json",
@"{
	""Rules"" : {
		""TestRule"" : {
			""Type"" : ""BaseClass"",
			""AttributeTypeName"" : ""AttributeRules.Test.RequiredAttribute"",
			""BaseClassTypeName"" : ""AttributeRules.Test.MyCommand""
		}
	}
}");

		[Fact]
		public async Task NotifiesBaseClassOneLevel()
		{
			var testCase = 
@"namespace AttributeRules.Test
{
	using System;
	using System.Threading.Tasks;

	public class TestCommand : MyCommand
	{
	}
}";

			var baseClass = 
@"namespace AttributeRules.Test
{
	using System;
	using System.Threading.Tasks;

	public class MyCommand
	{
	}
}";

			var requiredAttribute = 
@"namespace AttributeRules.Test
{
	using System;
	using System.Threading.Tasks;

	public class RequiredAttribute : Attribute
	{
	}
}";

			await CSharpAnalyzerVerifier<AttributeRuleAnalyzer>.VerifyAnalyzerAsync(
				new string[] {  testCase, baseClass, requiredAttribute }, 
				config,
				CSharpAnalyzerVerifier<AttributeRuleAnalyzer>.Diagnostic(AttributeDiagnostics.AttributeConventionNotMet).WithSpan(6, 15, 6, 26).WithArguments("Class misses the RequiredAttribute attribute"));
		}

		[Fact]
		public async Task NotifiesNothingWhenCorrect()
		{
			var testCase = 
@"namespace AttributeRules.Test
{
	using System;
	using System.Threading.Tasks;

	[RequiredAttribute]
	public class TestCommand : MyCommand
	{
	}
}";

			var baseClass = 
@"namespace AttributeRules.Test
{
	using System;
	using System.Threading.Tasks;

	public class MyCommand
	{
	}
}";

			var requiredAttribute = 
@"namespace AttributeRules.Test
{
	using System;
	using System.Threading.Tasks;

	public class RequiredAttribute : Attribute
	{
	}
}";

			await CSharpAnalyzerVerifier<AttributeRuleAnalyzer>.VerifyAnalyzerAsync(
				new string[] { testCase, baseClass, requiredAttribute },
				config);
		}

		[Fact]
		public async Task NotifiesBaseClassTwoLevels()
		{
			var testCase = 
@"namespace AttributeRules.Test
{
	using System;
	using System.Threading.Tasks;

	public class TestCommand : IntermediateClass
	{
	}
}";

			var intermediateClass = 
@"namespace AttributeRules.Test
{
	using System;
	using System.Threading.Tasks;

	public class IntermediateClass : MyCommand
	{
	}
}";

			var baseClass = 
@"namespace AttributeRules.Test
{
	using System;
	using System.Threading.Tasks;

	public class MyCommand
	{
	}
}";

			var requiredAttribute = 
@"namespace AttributeRules.Test
{
	using System;
	using System.Threading.Tasks;

	public class RequiredAttribute : Attribute
	{
	}
}";

			await CSharpAnalyzerVerifier<AttributeRuleAnalyzer>.VerifyAnalyzerAsync(
				new string[] { testCase, intermediateClass, baseClass, requiredAttribute },
				config,
				CSharpAnalyzerVerifier<AttributeRuleAnalyzer>.Diagnostic(AttributeDiagnostics.AttributeConventionNotMet).WithSpan(6, 15, 6, 26).WithArguments("Class misses the RequiredAttribute attribute"),
				CSharpAnalyzerVerifier<AttributeRuleAnalyzer>.Diagnostic(AttributeDiagnostics.AttributeConventionNotMet).WithSpan("/0/Test1.cs", 6, 15, 6, 32).WithArguments("Class misses the RequiredAttribute attribute"));
		}

		[Fact]
		public async Task NotifiesBaseClassTwoLevelsWithAbstractClassAnalysisEnabled()
		{
			AdditionalDocument configWithAbstractClasses = new AdditionalDocument("attributeRules.json",
@"{
	""Rules"" : {
		""TestRule"" : {
			""Type"" : ""BaseClass"",
			""AttributeTypeName"" : ""AttributeRules.Test.RequiredAttribute"",
			""BaseClassTypeName"" : ""AttributeRules.Test.MyCommand"",
			""AnalyzeAbstractClasses"" : true
		}
	}
}");

		var testCase =
@"namespace AttributeRules.Test
{
	using System;
	using System.Threading.Tasks;

	public class TestCommand : IntermediateClass
	{
	}
}";

			var intermediateClass =
@"namespace AttributeRules.Test
{
	using System;
	using System.Threading.Tasks;

	public abstract class IntermediateClass : MyCommand
	{
	}
}";

			var baseClass =
@"namespace AttributeRules.Test
{
	using System;
	using System.Threading.Tasks;

	public abstract class MyCommand
	{
	}
}";

			var requiredAttribute =
@"namespace AttributeRules.Test
{
	using System;
	using System.Threading.Tasks;

	public class RequiredAttribute : Attribute
	{
	}
}";

			await CSharpAnalyzerVerifier<AttributeRuleAnalyzer>.VerifyAnalyzerAsync(
				new string[] { testCase, intermediateClass, baseClass, requiredAttribute },
				configWithAbstractClasses,
				CSharpAnalyzerVerifier<AttributeRuleAnalyzer>.Diagnostic(AttributeDiagnostics.AttributeConventionNotMet).WithSpan(6, 15, 6, 26).WithArguments("Class misses the RequiredAttribute attribute"),
				CSharpAnalyzerVerifier<AttributeRuleAnalyzer>.Diagnostic(AttributeDiagnostics.AttributeConventionNotMet).WithSpan("/0/Test1.cs", 6, 24, 6, 41).WithArguments("Class misses the RequiredAttribute attribute"));
		}

		[Fact]
		public async Task NotifiesBaseClassTwoLevelsWithAbstractClassAnalysisDisabled()
		{
			var testCase =
@"namespace AttributeRules.Test
{
	using System;
	using System.Threading.Tasks;

	public class TestCommand : IntermediateClass
	{
	}
}";

			var intermediateClass =
@"namespace AttributeRules.Test
{
	using System;
	using System.Threading.Tasks;

	public abstract class IntermediateClass : MyCommand
	{
	}
}";

			var baseClass =
@"namespace AttributeRules.Test
{
	using System;
	using System.Threading.Tasks;

	public abstract class MyCommand
	{
	}
}";

			var requiredAttribute =
@"namespace AttributeRules.Test
{
	using System;
	using System.Threading.Tasks;

	public class RequiredAttribute : Attribute
	{
	}
}";

			await CSharpAnalyzerVerifier<AttributeRuleAnalyzer>.VerifyAnalyzerAsync(
				new string[] { testCase, intermediateClass, baseClass, requiredAttribute },
				config,
				CSharpAnalyzerVerifier<AttributeRuleAnalyzer>.Diagnostic(AttributeDiagnostics.AttributeConventionNotMet).WithSpan(6, 15, 6, 26).WithArguments("Class misses the RequiredAttribute attribute"));
		}

		[Fact]
		public async Task NotifiesWhenAttributeClassNotFound()
		{
			AdditionalDocument invalidConfig = new AdditionalDocument("attributeRules.json",
@"{
	""Rules"" : {
		""TestRule"" : {
			""Type"" : ""BaseClass"",
			""AttributeTypeName"" : ""AttributeRules.Test.RequiredAttributes"",
			""BaseClassTypeName"" : ""AttributeRules.Test.MyCommand""
		}
	}
}");

			var testCase =
@"namespace AttributeRules.Test
{
	using System;
	using System.Threading.Tasks;

	public class TestCommand : MyCommand
	{
	}
}";

			var baseClass =
@"namespace AttributeRules.Test
{
	using System;
	using System.Threading.Tasks;

	public class MyCommand
	{
	}
}";

			var requiredAttribute =
@"namespace AttributeRules.Test
{
	using System;
	using System.Threading.Tasks;

	public class RequiredAttribute : Attribute
	{
	}
}";

			await CSharpAnalyzerVerifier<AttributeRuleAnalyzer>.VerifyAnalyzerAsync(
				new string[] { testCase, baseClass, requiredAttribute },
				invalidConfig,
				CSharpAnalyzerVerifier<AttributeRuleAnalyzer>.Diagnostic(AttributeDiagnostics.InvalidRuleConfig).WithArguments("Attribute class was not found"));
		}
	}
}