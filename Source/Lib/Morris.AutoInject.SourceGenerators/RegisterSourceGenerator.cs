using Microsoft.CodeAnalysis;
using Morris.AutoInject.SourceGenerators.Extensions;
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
			.Collect();

		IncrementalValueProvider<ImmutableArray<(string? NamespaceName, string Name)>> classesWithAutoInjectFilterAttribute =
			context
			.SyntaxProvider
			.ForAttributeWithMetadataName(
				fullyQualifiedMetadataName: "Morris.AutoInject.AutoInjectFilterAttribute",
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
			.Collect();

		IncrementalValueProvider<ImmutableArray<(string? NamespaceName, string Name)>> allClassesWithAutoInjectAttributes =
			classesWithAutoInjectAttribute
			.Combine(classesWithAutoInjectFilterAttribute)
			.Select(static (combined, _) =>
			{
				var (autoInjectClasses, autoInjectFilterClasses) = combined;
				return autoInjectClasses.Concat(autoInjectFilterClasses).Distinct().ToImmutableArray();
			});


		context.RegisterSourceOutput(allClassesWithAutoInjectAttributes, static (context, items) =>
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
		writer.AddBlankLine();
		IDisposable? namespaceBlock =
			item.NamespaceName is not null
			? writer.CodeBlock($"namespace {item.NamespaceName}")
			: null;

		using (writer.CodeBlock($"partial class {item.Name}"))
		{
			writer.WriteLine("static partial void AfterRegisterServices(IServiceCollection services);");
			using (writer.CodeBlock("public static void RegisterServices(IServiceCollection services)"))
			{
				writer.WriteLine("AfterRegisterServices(services);");
				writer.WriteLine("throw new System.NotImplementedException(\"Morris.AutoInject.Fody has not processed this assembly.\");");
			}
		}

		namespaceBlock?.Dispose();
	}
}
