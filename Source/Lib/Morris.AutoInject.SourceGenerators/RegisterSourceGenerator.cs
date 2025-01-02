using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;
using System.Collections.Immutable;

namespace Morris.AutoInject.SourceGenerators;

[Generator]
public class RegisterSourceGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		IncrementalValueProvider<ImmutableArray<(string? NamespaceName, string Name)>> classesWithAutoInjectAttribute =
			context
			.SyntaxProvider
			.ForAttributeWithMetadataName(
				fullyQualifiedMetadataName: "Morris.AutoInject.AutoInjectAttribute",
				predicate: static (_, _) => true,
				transform: static (context, token) =>
				{
					ISymbol targetSymbol = context.TargetSymbol;
					string? namespaceName =
						targetSymbol.ContainingNamespace.IsGlobalNamespace
						? null
						: targetSymbol.ContainingNamespace.Name;
					return (namespaceName, targetSymbol.Name);
				}
			)
			.Collect()
			.Select(static (items, _) => items.Distinct().ToImmutableArray());

		context.RegisterSourceOutput(classesWithAutoInjectAttribute, static (context, items) =>
		{
			using var output = new StringWriter();
			using var writer = new IndentedTextWriter(output);

			writer.WriteLine("using Microsoft.Extensions.DependencyInjection;");
			foreach (var item in items)
				GenerateCodeForTarget(writer, item);

			writer.Flush();
			context.AddSource($"Morris.AutoInject.{nameof(RegisterSourceGenerator)}.g.cs", output.ToString());
		});
	}

	private static void GenerateCodeForTarget(
		IndentedTextWriter writer,
		(string? NamespaceName, string Name) item)
	{
		writer.WriteLine();
		if (item.NamespaceName is not null)
		{
			writer.WriteLine($"namespace {item.NamespaceName}");
			writer.WriteLine("{");
			writer.Indent++;
		}

		writer.WriteLine("static partial void AfterRegisterServices(IServiceCollection services);");
		writer.WriteLine("public static void RegisterServices(IServiceCollection services)");
		writer.WriteLine("{");
		{
			writer.Indent++;
			writer.WriteLine("throw new System.NotImplementedException(\"Fody weaver has not processed this assembly.\");");
			writer.Indent--;
		}
		writer.WriteLine("}");

		if (item.NamespaceName is not null)
		{
			writer.Indent--;
			writer.WriteLine("}");
		}
	}
}
