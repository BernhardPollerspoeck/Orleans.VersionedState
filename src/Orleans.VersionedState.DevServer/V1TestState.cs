namespace Orleans.PersistenceUpdate.DevServer;

public class V1TestState : VersionedState
{
	#region properties
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	#endregion

	#region VersionedState
	public override int Version => 1;
	public override bool KeepIfOutdated => true;
	#endregion
}
