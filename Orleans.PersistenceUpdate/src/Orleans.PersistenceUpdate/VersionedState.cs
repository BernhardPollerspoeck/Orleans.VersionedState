namespace Orleans.PersistenceUpdate;

public abstract class VersionedState
{
	public abstract int Version { get; }

	public virtual bool ConvertState(VersionedState? source)
	{
		return true;
	}
}
