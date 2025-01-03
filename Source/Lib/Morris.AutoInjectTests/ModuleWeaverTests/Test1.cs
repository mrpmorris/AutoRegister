using Fody;

namespace Morris.AutoInjectTests.ModuleWeaverTests;

[TestClass]
public class Test1 : TestBase
{
	[TestMethod]
	public void X()
	{
		Fody.TestResult testResult = Subject.ExecuteTestRun(typeof(TestBase).Assembly.Location);
	}
}
