using BA.Roslyn.ClassContextGenerator.Tests.Verifiers;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace BA.Roslyn.ClassContextGenerator.Tests
{
    public class ClassGenerationTests
    {
        [Fact]
        public async Task DefaultTest()
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
    }
}