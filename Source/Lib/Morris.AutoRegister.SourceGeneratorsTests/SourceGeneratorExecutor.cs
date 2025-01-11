using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Morris.AutoRegister.SourceGenerators;
using System.Collections.Immutable;
using System.Text;
using Morris.AutoRegister.TestsShared;

namespace Morris.AutoRegister.SourceGeneratorsTests;

internal static class SourceGeneratorExecutor
{
	private static readonly MetadataReference AutoRegisterMetadataReference =
		MetadataReference
		.CreateFromFile(typeof(AutoRegisterAttribute).Assembly.Location);

	private static readonly MetadataReference MSDependencyInjectionMetadataReference =
		MetadataReference
		.CreateFromFile(typeof(ServiceLifetime).Assembly.Location);

	public static void AssertGeneratedCodeMatches(
		string sourceCode,
		string expectedGeneratedCode)
	{
		var unitTestSyntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

		var compilation = CSharpCompilation.Create(
			assemblyName: "Test",
			syntaxTrees: [unitTestSyntaxTree],
			references: Basic.Reference.Assemblies.Net90.References
				.All
				.Union([AutoRegisterMetadataReference, MSDependencyInjectionMetadataReference]),
			options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, reportSuppressedDiagnostics: true)
		);

		AssertNoDiagnostics(compilation);

		var subject = new RegisterSourceGenerator().AsSourceGenerator();
		var driver = CSharpGeneratorDriver
			.Create(subject)
			.RunGenerators(compilation);

		GeneratorDriverRunResult runResult = driver.GetRunResult();
		GeneratorRunResult result = runResult.Results.Single();

		GeneratedSourceResult generatedSource = result.GeneratedSources.Single();
		Assert.AreEqual("Morris.AutoRegister.RegisterSourceGenerator.g.cs", generatedSource.HintName);
		string generatedCode = generatedSource.SyntaxTree.ToString();
		Assert.AreEqual(expectedGeneratedCode.StandardizeLines(), generatedCode.StandardizeLines());
	}

	private static void AssertNoDiagnostics(CSharpCompilation compilation)
	{
		ImmutableArray<Diagnostic> diagnostics = compilation.GetDiagnostics();
		if (!diagnostics.Any())
			return;

		var builder = new StringBuilder();
		foreach (var diagnostic in diagnostics)
			builder.AppendLine(diagnostic.ToString());

		Assert.Fail("The following compiler errors were found:\r\n" + builder.ToString());
	}
}
