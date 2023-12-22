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
	private readonly IPersistentState<VersionedStateContainer> _state;
	private VersionedStateManager<V1TestState> _stateManager;

	public TestStateGrain(
		[PersistentState("TestState", "STORE_NAME")]
		IPersistentState<VersionedStateContainer> state)
	{
		_state = state;
		_stateManager = default!;//TODO: meh...
	}

	public override Task OnActivateAsync(CancellationToken cancellationToken)
	{
		_stateManager = new(_state.State.States);
		return base.OnActivateAsync(cancellationToken);
	}

	public Task SetState(string first, string last)
	{
		var current = _stateManager.Current;
		current.FirstName = first;
		current.LastName = last;
		return _state.WriteStateAsync();
	}

	public Task<string> GetState()
	{
		return Task.FromResult($"{_stateManager.Current.FirstName}:::{_stateManager.Current.LastName}");
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