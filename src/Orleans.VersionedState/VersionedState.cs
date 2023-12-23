namespace Orleans.PersistenceUpdate;

public abstract class VersionedState
{
	public abstract int Version { get; }
	public abstract bool KeepIfOutdated { get; }
}
