namespace Orleans.PersistenceUpdate;

public sealed class VersionedStateContainer
{
	public Dictionary<int, VersionedState> States { get; set; }

	public VersionedStateContainer()
	{
		States = [];
	}
}
