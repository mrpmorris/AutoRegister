using Morris.AutoInject;
using Morris.AutoInject.Fody;

namespace Morris.AutoInjectTests.ModuleWeaverTests;

public abstract class TestBase
{
	private string ProjectFilePath = null!;
	protected ModuleWeaver Subject = null!;

	[TestInitialize]
	public void Initialize()
	{
		ProjectFilePath = Path.Combine(
			Path.GetTempPath(),
			$"{Guid.NewGuid()}.csproj"
		);

		Subject = new ModuleWeaver() {
			ProjectFilePath = ProjectFilePath
		};
	}

	[TestCleanup]
	public void Cleanup()
	{
		string manifestFilePath = Path.ChangeExtension(ProjectFilePath, "Morris.AutoInject.manifest");
		if (File.Exists(manifestFilePath))
			File.Delete(manifestFilePath);
	}

}

[AutoInject(Find.DescendantsOf, typeof(object), RegisterAs.FirstDiscoveredInterface, WithLifetime.Scoped)]
public partial class MyModule
{

}
