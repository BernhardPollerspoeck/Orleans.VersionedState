# Orleans.VersionedState

This project allows to Transform GrainState between versions as needed. Migrating states is done with Transforms from one to another version. An A* Algoritm looks for the most efficient Transformation path even for complex transformation chains.

## Packages
| Package | Version |
|---------|---------|
| Orleans.VersionedState | ![Nuget](https://img.shields.io/nuget/v/Orleans.VersionedState?logo=NuGet&color=00aa00) |



## Sample Explaination

### Working with grain state
The injected state for Grains that use this feature now all have to be of type: 'IPersistentState<VersionedStateContainer>'. This implies now, that all States for grains inherit from 'VersionedState'.
State is Accessed through a StateManager (see Grain setup). The supplied storeHasChanged ref variable is to indicate if the state retrieval did change the state and writing state is suggested.
```c#
public async Task<string> GetState()
{
	var hasChanged = false;
	var name = _stateManager.GetState(ref hasChanged).FullName;
	if (hasChanged)
	{
		await _stateManager.WriteStateAsync();
	}
	return name;
}
```

### Grain setup
To work with state we now dont hold on the state, but we instanciate the appropriate StateManager.
```c#
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

```

### Transformations
The generic Argument always represents the currently used type. Transforms are simple definitions that contain the source and target version and also a 'Func<VersionedState, VersionedState>' to define how the transformation between the defined versions is done.
```c#
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
```

## Contribution
Contributions in any way are appechiated (More providers, improvements, documentation or anything else). I Kindly ask you to create a Issue to talk about the planned changes or contact me directly on the Orleans Discord.
