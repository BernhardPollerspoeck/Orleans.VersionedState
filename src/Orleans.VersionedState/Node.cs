namespace Orleans.PersistenceUpdate;

internal class Node
{
	#region properties
	public int Value { get; }
	public int[] Targets { get; }
	#endregion

	#region ctor
	public Node(int value, IEnumerable<int> targets)
	{
		Value = value;
		Targets = targets.ToArray();
	}
	#endregion
}
