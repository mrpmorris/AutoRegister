using Fody;
using System.Text;

namespace Morris.AutoInjectTests.Extensions;

internal static class FodyTestResultExtensions
{
	public static void AssertNoDiagnostics(this Fody.TestResult result)
	{
		if (result.Errors.Count + result.Warnings.Count + result.Messages.Count == 0)
			return;

		var builder = new StringBuilder();
		AddMessages(builder, "Error", result.Errors);
		AddMessages(builder, "Warning", result.Warnings);
		Assert.Fail($"Expected no Fody diagnostics but encountered the following\r\n{builder}");
	}

	private static void AddMessages(
		StringBuilder builder,
		string messageType,
		IReadOnlyList<SequencePointMessage> messages)
	{
		foreach (SequencePointMessage message in messages)
			builder.AppendLine($"{messageType}: {message}");
	}
}
