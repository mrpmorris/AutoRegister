using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace Morris.AutoRegister.SourceGenerators.Extensions;

internal static class IndentedTextWriterAddBlankLineExtension
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void AddBlankLine(this IndentedTextWriter writer)
	{
		int originalIndent = writer.Indent;
		writer.Indent = 0;
		writer.WriteLine();
		writer.Indent = originalIndent;
	}
}
