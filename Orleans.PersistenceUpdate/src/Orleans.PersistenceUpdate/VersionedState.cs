namespace Orleans.PersistenceUpdate;

public abstract class VersionedState
{
	public abstract int Version { get; }
}
