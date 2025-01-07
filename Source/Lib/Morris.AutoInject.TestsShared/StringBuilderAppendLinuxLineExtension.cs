using System.Text;

namespace Morris.AutoInject.TestsShared;

public static class StringBuilderAppendLinuxLineExtension
{
	public static void AppendLinuxLine(this StringBuilder builder)
	{
		builder.Append('\n');
	}

	public static void AppendLinuxLine(this StringBuilder builder, string text)
	{
		builder.Append(text);
		builder.Append('\n');
	}
}
