using Mono.Cecil;
using System.Text.RegularExpressions;

namespace Morris.AutoInject.Fody;

internal class AutoInjectFilterAttributeData
{
	public string? ServiceImplementationRegex { get; private set; }

	private Regex Regex;

	public AutoInjectFilterAttributeData(string? serviceImplementationRegex)
	{
		ServiceImplementationRegex = serviceImplementationRegex;
		Regex = new Regex(serviceImplementationRegex, RegexOptions.Compiled);
	}

	public bool Matches(TypeDefinition typeDefinition) =>
		Regex.IsMatch(typeDefinition.FullName);
}
