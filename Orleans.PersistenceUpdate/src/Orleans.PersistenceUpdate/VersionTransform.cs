namespace Orleans.PersistenceUpdate;

/// <summary>
/// TODO: doc
/// The Tranform is expected to always return the new versioned object. 
/// Source and Target Version get checked before Tranform is called.
/// </summary>
/// <param name="SourceVersion"></param>
/// <param name="TargetVersion"></param>
/// <param name="Transform"></param>
public record VersionTransform(
	int SourceVersion,
	int TargetVersion,
	Func<VersionedState, VersionedState> Transform);