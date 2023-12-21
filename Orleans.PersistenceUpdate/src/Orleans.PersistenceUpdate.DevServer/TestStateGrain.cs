using Orleans.PersistenceUpdate.DevGrain;
using Orleans.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.PersistenceUpdate.DevServer;
internal class TestStateGrain : Grain, ITestStateGrain
{
	private readonly IPersistentState<VersionedStateContainer<V1TestState>> _state;

	public TestStateGrain(
		[PersistentState("TestState", "STORE_NAME")]
		IPersistentState<VersionedStateContainer<V1TestState>> state)
	{
		_state = state;
	}

	public Task SetState(string first, string last)
	{
		var current = _state.State.Current;
		current.FirstName = first;
		current.LastName = last;
		return _state.WriteStateAsync();
	}

	public Task<string> GetState()
	{
		return Task.FromResult($"{_state.State.Current.FirstName}:::{_state.State.Current.LastName}");
	}

}


[GenerateSerializer]
class V1TestState : VersionedState
{
	#region properties
	[Id(0)]
	public string? FirstName { get; set; }
	[Id(1)]
	public string? LastName { get; set; }
	#endregion

	#region VersionedState
	public override int Version => 1;
	#endregion
}

[GenerateSerializer]
class V2TestState : VersionedState
{
	#region properties
	[Id(0)]
	public string? FullName { get; set; }
	#endregion

	#region VersionedState
	public override int Version => 2;

	public override bool ConvertState(VersionedState? source)
	{
		if (source is not V1TestState v1)
		{
			return false;
		}
		FullName = $"{v1.FirstName} {v1.LastName}";
		return true;
	}
	#endregion
}