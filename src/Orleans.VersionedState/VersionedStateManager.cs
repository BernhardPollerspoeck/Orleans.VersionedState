using Orleans.Core;

namespace Orleans.PersistenceUpdate;

public abstract class VersionedStateManager<TCurrentState> : IStorage
	where TCurrentState : VersionedState, new()
{
	#region fields
	private readonly IStorage<VersionedStateContainer> _stateContainer;
	private TCurrentState? _convertedState;
	#endregion

	#region properties
	protected abstract VersionTransform[] Transforms { get; }
	#endregion

	#region ctor
	public VersionedStateManager(IStorage<VersionedStateContainer> stateContainer)
	{
		_stateContainer = stateContainer;
	}
	#endregion

	public TCurrentState GetState(ref bool storeHasChanged)
	{
		storeHasChanged = false;
		return _convertedState ?? ConvertStateToCurrentVersion(ref storeHasChanged);
	}
	public TCurrentState GetState()
	{
		var storeHasChanged = false;
		return _convertedState ?? ConvertStateToCurrentVersion(ref storeHasChanged);
	}

	#region IStorageMember
	public string Etag => _stateContainer.Etag;
	public bool RecordExists => _stateContainer.RecordExists;
	public Task ClearStateAsync() => _stateContainer.ClearStateAsync();
	public Task WriteStateAsync() => _stateContainer.WriteStateAsync();
	public Task ReadStateAsync() => _stateContainer.ReadStateAsync();
	#endregion

	private TCurrentState ConvertStateToCurrentVersion(ref bool storeHasChanged)
	{
		//in case there is nothing in state, we do not tranform=> we create and return
		if (_stateContainer.State.States.Count == 0)
		{
			_convertedState = new TCurrentState();
			_stateContainer.State.States.Add(_convertedState.Version, _convertedState);
			storeHasChanged = true;
			return _convertedState;
		}

		//in case we already have the correct item in Store return it without tranforming
		if (_stateContainer.State.States.Any(s => s.Value is TCurrentState currentState))
		{
			_convertedState = (TCurrentState)_stateContainer.State.States.First(s => s.Value is TCurrentState).Value;
			return _convertedState;
		}




		//TODO once we have the chance to get the current version for node values, we dont need this instance
		var newState = new TCurrentState();

		var nodes = Transforms
			.SelectMany(t => new int[] { t.SourceVersion, t.TargetVersion })
			.Distinct()
			.Select(v => new Node(v, Transforms.Where(t => t.SourceVersion == v).Select(t => t.TargetVersion)))
			.ToArray();

		var path = FindShortestPath(nodes, [.. _stateContainer.State.States.Keys], newState.Version)
			.Select(p => p.Value)
			.ToArray();
		if (path.Length == 0)
		{
			_convertedState = new TCurrentState();
			_stateContainer.State.States.Add(_convertedState.Version, _convertedState);
			storeHasChanged = true;
			return _convertedState;
		}
		else if (path.Length == 1)
		{
			_convertedState = (TCurrentState)_stateContainer.State.States[path.First()];
			return _convertedState;
		}
		else
		{
			VersionedState? result = null;
			int? previousValue = null;
			foreach (var segment in path)
			{
				if (result is null)
				{//initialization step
					result = _stateContainer.State.States[segment];
					previousValue = segment;
					continue;
				}

				var transform = Transforms
					.First(t => t.SourceVersion == previousValue!.Value && t.TargetVersion == segment);
				if (!result.KeepIfOutdated)
				{
					_stateContainer.State.States.Remove(result.Version);
				}
				result = transform.Transform(result);
			}
			_convertedState = (TCurrentState)result!;
			_stateContainer.State.States.Add(_convertedState.Version, _convertedState);
			storeHasChanged = true;
			return _convertedState;
		}
	}

	private static List<Node> FindShortestPath(Node[] nodes, int[] startValues, int targetValue)
	{
		var nodeDictionary = nodes.ToDictionary(node => node.Value);
		var closedSet = new HashSet<int>();
		var openSet = new HashSet<int>(startValues);
		var cameFrom = new Dictionary<int, int>();
		var gScore = nodes.ToDictionary(node => node.Value, _ => int.MaxValue);
		var fScore = nodes.ToDictionary(node => node.Value, _ => int.MaxValue);

		foreach (var startValue in startValues)
		{
			gScore[startValue] = 0;
			fScore[startValue] = HeuristicCostEstimate(startValue, targetValue);
		}

		while (openSet.Count > 0)
		{
			var current = openSet.OrderBy(node => fScore[node]).First();

			if (current == targetValue)
			{
				return ReconstructPath(cameFrom, targetValue, nodeDictionary);
			}

			openSet.Remove(current);
			closedSet.Add(current);

			foreach (var neighborValue in nodeDictionary[current].Targets)
			{
				if (closedSet.Contains(neighborValue))
				{
					continue;
				}

				var tentativeGScore = gScore[current] + 1; // Assuming a cost of 1 for each step

				if (!openSet.Add(neighborValue)
					&& tentativeGScore >= gScore[neighborValue])
				{
					continue; // This is not a better path
				}

				cameFrom[neighborValue] = current;
				gScore[neighborValue] = tentativeGScore;
				fScore[neighborValue] = gScore[neighborValue] + HeuristicCostEstimate(neighborValue, targetValue);
			}
		}

		// If the loop completes without finding a path, return an empty list
		return [];
	}

	private static int HeuristicCostEstimate(int start, int target)
	{
		// This is a simple heuristic, you might want to improve it based on your specific graph
		return Math.Abs(start - target);
	}

	private static List<Node> ReconstructPath(Dictionary<int, int> cameFrom, int current, Dictionary<int, Node> nodes)
	{
		var path = new List<Node> { nodes[current] };

		while (cameFrom.ContainsKey(current))
		{
			current = cameFrom[current];
			path.Insert(0, nodes[current]);
		}

		return path;
	}
}
