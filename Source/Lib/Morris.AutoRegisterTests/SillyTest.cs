using Morris.AutoRegister.Fody;

namespace Morris.AutoRegisterTests;

[TestClass]
public class SillyTest
{
	[TestMethod]
	public void Test()
	{
		Assert.AreEqual("Morris.AutoRegister.Fody.ModuleWeaver", typeof(ModuleWeaver).FullName);
	}
}
