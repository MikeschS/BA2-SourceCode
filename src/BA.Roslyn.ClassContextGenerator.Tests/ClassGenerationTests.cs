using BA.Roslyn.AttributeRules;
using BA.Roslyn.ClassContextGenerator.Tests.Verifiers;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace BA.Roslyn.ClassContextGenerator.Tests
{
    public class ClassGenerationTests
    {
        [Fact]
        public async Task CorrectlyDetectOneMode()
        {
            var handlerClass = @"namespace BA.Roslyn.ClassContextGenerator.Tests
{
    using BA.Roslyn.ClassContextGenerator.Abstractions;

    [ApplicationMode(ApplicationMode.Free)]
	public class HandlerClass
	{
        public void HandleRequest()
        {
            var someClass = new TargetClass();
        }
	}
}";

            var targetClass = @"namespace BA.Roslyn.ClassContextGenerator.Tests
{
    using BA.Roslyn.ClassContextGenerator.Abstractions;

	public partial class TargetClass : BaseClass
	{
	}
}";

            var expectedOutput = @"namespace BA.Roslyn.ClassContextGenerator.Tests
{
	[BA.Roslyn.ClassContextGenerator.Abstractions.ApplicationMode(BA.Roslyn.ClassContextGenerator.Abstractions.ApplicationMode.Free)]
	public partial class TargetClass
	{
	}
}";

            var tester = new CSharpSourceGeneratorVerifier<ApplicationModeGenerator>.Test()
            {
                TestState =
                {
                    Sources = { handlerClass, targetClass },
                    GeneratedSources =
                    {
                        (typeof(ApplicationModeGenerator), "TargetClass.g.cs", SourceText.From(expectedOutput, Encoding.UTF8)),
                    },
                },
            };

            await tester.RunAsync();
        }

        [Fact]
        public async Task CorrectlyDetectTwoModes()
        {
            var handlerClass = @"namespace BA.Roslyn.ClassContextGenerator.Tests
{
    using BA.Roslyn.ClassContextGenerator.Abstractions;

    [ApplicationMode(ApplicationMode.Free, ApplicationMode.Premium)]
	public class HandlerClass
	{
        public void HandleRequest()
        {
            var someClass = new TargetClass();
        }
	}
}";

            var targetClass = @"namespace BA.Roslyn.ClassContextGenerator.Tests
{
    using BA.Roslyn.ClassContextGenerator.Abstractions;

	public partial class TargetClass : BaseClass
	{
	}
}";

            var expectedOutput = @"namespace BA.Roslyn.ClassContextGenerator.Tests
{
	[BA.Roslyn.ClassContextGenerator.Abstractions.ApplicationMode(BA.Roslyn.ClassContextGenerator.Abstractions.ApplicationMode.Free, BA.Roslyn.ClassContextGenerator.Abstractions.ApplicationMode.Premium)]
	public partial class TargetClass
	{
	}
}";

            var tester = new CSharpSourceGeneratorVerifier<ApplicationModeGenerator>.Test()
            {
                TestState =
                {
                    Sources = { handlerClass, targetClass },
                    GeneratedSources =
                    {
                        (typeof(ApplicationModeGenerator), "TargetClass.g.cs", SourceText.From(expectedOutput, Encoding.UTF8)),
                    },
                },
            };

            await tester.RunAsync();
        }

        [Fact]
        public async Task CorrectlyDetectTwoDifferentClassesDifferentMode()
        {
            var handlerClass1 = @"namespace BA.Roslyn.ClassContextGenerator.Tests
{
    using BA.Roslyn.ClassContextGenerator.Abstractions;

    [ApplicationMode(ApplicationMode.Free)]
	public class HandlerClass1
	{
        public void HandleRequest()
        {
            var someClass = new TargetClass();
        }
	}
}";

            var handlerClass2 = @"namespace BA.Roslyn.ClassContextGenerator.Tests
{
    using BA.Roslyn.ClassContextGenerator.Abstractions;

    [ApplicationMode(ApplicationMode.Premium)]
	public class HandlerClass2
	{
        public void HandleRequest()
        {
            var someClass = new TargetClass();
        }
	}
}";

            var targetClass = @"namespace BA.Roslyn.ClassContextGenerator.Tests
{
    using BA.Roslyn.ClassContextGenerator.Abstractions;

	public partial class TargetClass : BaseClass
	{
	}
}";

            var expectedOutput = @"namespace BA.Roslyn.ClassContextGenerator.Tests
{
	[BA.Roslyn.ClassContextGenerator.Abstractions.ApplicationMode(BA.Roslyn.ClassContextGenerator.Abstractions.ApplicationMode.Free, BA.Roslyn.ClassContextGenerator.Abstractions.ApplicationMode.Premium)]
	public partial class TargetClass
	{
	}
}";

            var tester = new CSharpSourceGeneratorVerifier<ApplicationModeGenerator>.Test()
            {
                TestState =
                {
                    Sources = { handlerClass1, handlerClass2, targetClass },
                    GeneratedSources =
                    {
                        (typeof(ApplicationModeGenerator), "TargetClass.g.cs", SourceText.From(expectedOutput, Encoding.UTF8)),
                    },
                },
            };

            await tester.RunAsync();
        }

        [Fact]
        public async Task CorrectlyDetectTwoDifferentClassesSameMode()
        {
            var handlerClass1 = @"namespace BA.Roslyn.ClassContextGenerator.Tests
{
    using BA.Roslyn.ClassContextGenerator.Abstractions;

    [ApplicationMode(ApplicationMode.Free)]
	public class HandlerClass1
	{
        public void HandleRequest()
        {
            var someClass = new TargetClass();
        }
	}
}";

            var handlerClass2 = @"namespace BA.Roslyn.ClassContextGenerator.Tests
{
    using BA.Roslyn.ClassContextGenerator.Abstractions;

    [ApplicationMode(ApplicationMode.Free)]
	public class HandlerClass2
	{
        public void HandleRequest()
        {
            var someClass = new TargetClass();
        }
	}
}";

            var targetClass = @"namespace BA.Roslyn.ClassContextGenerator.Tests
{
    using BA.Roslyn.ClassContextGenerator.Abstractions;

	public partial class TargetClass : BaseClass
	{
	}
}";

            var expectedOutput = @"namespace BA.Roslyn.ClassContextGenerator.Tests
{
	[BA.Roslyn.ClassContextGenerator.Abstractions.ApplicationMode(BA.Roslyn.ClassContextGenerator.Abstractions.ApplicationMode.Free)]
	public partial class TargetClass
	{
	}
}";

            var tester = new CSharpSourceGeneratorVerifier<ApplicationModeGenerator>.Test()
            {
                TestState =
                {
                    Sources = { handlerClass1, handlerClass2, targetClass },
                    GeneratedSources =
                    {
                        (typeof(ApplicationModeGenerator), "TargetClass.g.cs", SourceText.From(expectedOutput, Encoding.UTF8)),
                    },
                },
            };

            await tester.RunAsync();
        }

        [Fact]
        public async Task NotifiesOfMissingApplicationModeAttribute()
        {
            var handlerClass = @"namespace BA.Roslyn.ClassContextGenerator.Tests
{
    using BA.Roslyn.ClassContextGenerator.Abstractions;

	public class HandlerClass
	{
        public void HandleRequest()
        {
            var someClass = new TargetClass();
        }
	}
}";

            var targetClass = @"namespace BA.Roslyn.ClassContextGenerator.Tests
{
    using BA.Roslyn.ClassContextGenerator.Abstractions;

	public partial class TargetClass : BaseClass
	{
	}
}";

            var expectedOutput = @"namespace BA.Roslyn.ClassContextGenerator.Tests
{
	[BA.Roslyn.ClassContextGenerator.Abstractions.ApplicationMode()]
	public partial class TargetClass
	{
	}
}";

            var tester = new CSharpSourceGeneratorVerifier<ApplicationModeGenerator>.Test()
            {
                TestState =
                {
                    Sources = { handlerClass, targetClass },
                    GeneratedSources =
                    {
                        (typeof(ApplicationModeGenerator), "TargetClass.g.cs", SourceText.From(expectedOutput, Encoding.UTF8)),
                    },
                    ExpectedDiagnostics =
                    {
                        new Microsoft.CodeAnalysis.Testing.DiagnosticResult(GeneratorDiagnostics.ApplicationModeNotDefined).WithSpan(5, 15, 5, 27).WithArguments("HandlerClass", "TargetClass"),
                    },
                },
            };

            await tester.RunAsync();
        }

        [Fact]
        public async Task NotifiesOfMissingApplicationModeAttributeInOneClassWhileOtherClassProvidesMode()
        {
            var handlerClass1 = @"namespace BA.Roslyn.ClassContextGenerator.Tests
{
    using BA.Roslyn.ClassContextGenerator.Abstractions;

	public class HandlerClass1
	{
        public void HandleRequest()
        {
            var someClass = new TargetClass();
        }
	}
}";

            var handlerClass2 = @"namespace BA.Roslyn.ClassContextGenerator.Tests
{
    using BA.Roslyn.ClassContextGenerator.Abstractions;

    [ApplicationMode(ApplicationMode.Free)]
	public class HandlerClass2
	{
        public void HandleRequest()
        {
            var someClass = new TargetClass();
        }
	}
}";

            var targetClass = @"namespace BA.Roslyn.ClassContextGenerator.Tests
{
    using BA.Roslyn.ClassContextGenerator.Abstractions;

	public partial class TargetClass : BaseClass
	{
	}
}";

            var expectedOutput = @"namespace BA.Roslyn.ClassContextGenerator.Tests
{
	[BA.Roslyn.ClassContextGenerator.Abstractions.ApplicationMode(BA.Roslyn.ClassContextGenerator.Abstractions.ApplicationMode.Free)]
	public partial class TargetClass
	{
	}
}";

            var tester = new CSharpSourceGeneratorVerifier<ApplicationModeGenerator>.Test()
            {
                TestState =
                {
                    Sources = { handlerClass1, handlerClass2, targetClass },
                    GeneratedSources =
                    {
                        (typeof(ApplicationModeGenerator), "TargetClass.g.cs", SourceText.From(expectedOutput, Encoding.UTF8)),
                    },
                    ExpectedDiagnostics =
                    {
                        new Microsoft.CodeAnalysis.Testing.DiagnosticResult(GeneratorDiagnostics.ApplicationModeNotDefined).WithSpan(5, 15, 5, 28).WithArguments("HandlerClass1", "TargetClass"),
                    },
                },
            };

            await tester.RunAsync();
        }
    }
}