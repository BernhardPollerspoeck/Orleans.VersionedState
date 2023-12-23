using Orleans.Core;
using Orleans.PersistenceUpdate.DevGrain;
using Orleans.Runtime;

namespace Orleans.PersistenceUpdate.DevServer;
public class TestStateGrain : Grain, ITestStateGrain
{
	#region fields
	private readonly TestStateManager _stateManager;
	#endregion

	#region ctor
	public TestStateGrain(
		[PersistentState("TestState", "STORE_NAME")]
		IPersistentState<VersionedStateContainer> state)
	{
		_stateManager = new(state);
	}
	#endregion

	#region ITestStateGrain
	public Task SetState(string first, string last)
	{
		//var current = _stateManager.Current;
		//current.FirstName = first;
		//current.LastName = last;
		return _stateManager.WriteStateAsync();
	}

	public async Task<string> GetState()
	{
		var hasChanged = false;
		var name = _stateManager.GetState(ref hasChanged).FullName!;
		if (hasChanged)
		{
			await _stateManager.WriteStateAsync();
		}
		return name;
	}
	#endregion
}


public class TestStateManager : VersionedStateManager<V2TestState>
{
	#region ctor
	public TestStateManager(IStorage<VersionedStateContainer> stateContainer)
		: base(stateContainer)
	{
	}
	#endregion

	#region VersionedStateManager<V1TestState>
	protected override VersionTransform[] Transforms => [
		new VersionTransform(
			1,
			2,
			s => s is V1TestState v1
				? new V2TestState
				{
					FullName = $"{v1.FirstName} {v1.LastName}"
				}
				: throw new Exception("NO!!"))
	];
	#endregion
}

