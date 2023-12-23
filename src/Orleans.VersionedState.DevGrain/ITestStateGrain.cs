namespace Orleans.PersistenceUpdate.DevGrain;

public interface ITestStateGrain : IGrainWithStringKey
{
	Task SetState(string first, string last);
	Task<string> GetState();

}
