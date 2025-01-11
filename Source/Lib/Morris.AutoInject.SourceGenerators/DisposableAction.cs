namespace Morris.AutoRegister.SourceGenerators;

internal class DisposableAction : IDisposable
{
	private readonly Action Action;

	public DisposableAction(Action action)
	{
		Action = action;
	}

	void IDisposable.Dispose()
	{
		Action();
	}
}
