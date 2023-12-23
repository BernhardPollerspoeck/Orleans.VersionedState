namespace Orleans.PersistenceUpdate.DevServer;

public class V2TestState : VersionedState
{
	#region properties
	public string? FullName { get; set; }
	#endregion

	#region VersionedState
	public override int Version => 2;
	public override bool KeepIfOutdated => true;
	#endregion
}