using Fody;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using Morris.AutoInject;
using Morris.AutoInject.Fody;
using Morris.AutoInject.SourceGenerators;
using Morris.AutoInjectTests.Extensions;
using System.Collections.Immutable;
using System.Text;

namespace Morris.AutoInjectTests.ModuleWeaverTests;

internal static class WeaverExecutor
{
	private static readonly MetadataReference AutoInjectMetadataReference =
		MetadataReference
		.CreateFromFile(typeof(AutoInjectAttribute).Assembly.Location);

	private static readonly MetadataReference MSDependencyInjectionMetadataReference =
		MetadataReference
		.CreateFromFile(typeof(ServiceLifetime).Assembly.Location);


	public static void Execute(
		string sourceCode,
		out Fody.TestResult testResult,
		out string? manifest,
		bool assertNoDiagnosticsOutput = true)
	{
		Guid uniqueId = Guid.NewGuid();
		var unitTestSyntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
		var compilation = CSharpCompilation.Create(
			assemblyName: $"Test{uniqueId}",
			syntaxTrees: [unitTestSyntaxTree],
			references: Basic.Reference.Assemblies.Net90.References
				.All
				.Union([AutoInjectMetadataReference, MSDependencyInjectionMetadataReference]),
			options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, reportSuppressedDiagnostics: true)
		);
		AssertNoCompileDiagnostics(compilation);

		compilation = AddGeneratedSource(unitTestSyntaxTree, compilation, uniqueId);

		string projectFilePath = Path.Combine(
			Path.GetTempPath(),
			$"{uniqueId}.csproj"
		);
		string manifestFilePath = Path.ChangeExtension(projectFilePath, "Morris.AutoInject.manifest");
		string assemblyFilePath = Path.ChangeExtension(projectFilePath, "Morris.AutoInject.Tests.dll");

		try
		{
			compilation.Emit(assemblyFilePath);
			var weaver = new ModuleWeaver() {
				ProjectFilePath = projectFilePath
			};
			testResult = weaver.ExecuteTestRun(assemblyFilePath);
			manifest =
				!File.Exists(manifestFilePath)
				? null
				: File.ReadAllText(manifestFilePath);
			if (assertNoDiagnosticsOutput)
				testResult.AssertNoDiagnosticsOutput();
		}
		finally
		{
			if (File.Exists(manifestFilePath))
				File.Delete(manifestFilePath);
			if (File.Exists(assemblyFilePath))
				File.Delete(assemblyFilePath);
		}
	}

	private static CSharpCompilation AddGeneratedSource(
		SyntaxTree unitTestSyntaxTree,
		CSharpCompilation compilation,
		Guid uniqueId)
	{
		var registerSourceGenerator = new RegisterSourceGenerator().AsSourceGenerator();
		var driver = CSharpGeneratorDriver
			.Create(registerSourceGenerator)
			.RunGenerators(compilation);
		GeneratorDriverRunResult driverRunResult = driver.GetRunResult();
		GeneratorRunResult runResult = driverRunResult.Results.Single();
		var generatedSyntaxTrees = runResult.GeneratedSources.Select(x => x.SyntaxTree);
		compilation = CSharpCompilation.Create(
			assemblyName: $"TestWithGenerators{uniqueId}",
			syntaxTrees: generatedSyntaxTrees.Append(unitTestSyntaxTree),
			references: Basic.Reference.Assemblies.Net90.References
				.All
				.Union([AutoInjectMetadataReference, MSDependencyInjectionMetadataReference]),
			options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, reportSuppressedDiagnostics: true)
		);
		AssertNoCompileDiagnostics(compilation);
		return compilation;
	}

	private static void AssertNoCompileDiagnostics(CSharpCompilation compilation)
	{
		ImmutableArray<Diagnostic> diagnostics =
			compilation
			.GetDiagnostics()
			.Where(x => x.DefaultSeverity != DiagnosticSeverity.Hidden)
			.ToImmutableArray();
		if (!diagnostics.Any())
			return;

		var builder = new StringBuilder();
		foreach (var diagnostic in diagnostics)
			builder.AppendLine(diagnostic.ToString());

		Assert.Fail("The following compiler errors were found:\r\n" + builder.ToString());
	}

}


