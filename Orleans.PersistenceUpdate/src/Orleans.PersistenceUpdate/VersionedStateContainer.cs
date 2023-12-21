

using Newtonsoft.Json;

namespace Orleans.PersistenceUpdate;

[GenerateSerializer]
public class VersionedStateContainer<TCurrentState>
	where TCurrentState : VersionedState, new()
{
	private readonly Dictionary<int, VersionedState> _states;
	private TCurrentState? _convertedState;

	[JsonIgnore]
	public TCurrentState Current => _convertedState ?? ConvertStateToCurrentVersion();

	[Id(0)]
	public IReadOnlyDictionary<int, VersionedState> States => _states;

	public VersionedStateContainer(
		VersionedState[]? states)
	{
		_states = states?.ToDictionary(s => s.Version, s => s) ?? [];
	}
	public VersionedStateContainer()
	{
		_states = [];
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
