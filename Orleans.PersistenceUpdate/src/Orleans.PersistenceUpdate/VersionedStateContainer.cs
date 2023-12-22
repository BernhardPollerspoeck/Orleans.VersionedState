

using Newtonsoft.Json;

namespace Orleans.PersistenceUpdate;

public class VersionedStateContainer
{
	public Dictionary<int, VersionedState> States { get; set; }

	public VersionedStateContainer()
	{
		States = [];
	}
}

public class VersionedStateManager<TCurrentState>
	where TCurrentState : VersionedState, new()
{

	private TCurrentState? _convertedState;


	[JsonIgnore]
	public TCurrentState Current => _convertedState ?? ConvertStateToCurrentVersion();


	private readonly Dictionary<int, VersionedState> _states;

	public VersionedStateManager(Dictionary<int, VersionedState> states)
	{
		_states = states;
	}



	private TCurrentState ConvertStateToCurrentVersion()
	{
		var newState = new TCurrentState();
		//TODO: try to convert all '_states' starting with the clostest (until first success)
		newState.ConvertState(_states.Values.FirstOrDefault());
		if (!_states.TryAdd(newState.Version, newState))
		{
			_states[newState.Version] = newState;
		}
		_convertedState = newState;


		return _convertedState;
	}


}