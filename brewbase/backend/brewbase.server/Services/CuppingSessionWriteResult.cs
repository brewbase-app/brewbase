namespace brewbase.server.Services;

public enum CuppingSessionWriteStatus
{
    Success,
    Unauthorized,
    CuppingSessionNotFound,
    CoffeeNotFound,
    CoffeeAlreadyAdded,
    CoffeeNotInSession,
	InvalidCoffeeData
}

public sealed class CuppingSessionWriteResult<T>
{
    public CuppingSessionWriteStatus Status { get; }
    public T? Data { get; }

    public CuppingSessionWriteResult(CuppingSessionWriteStatus status, T? data = default)
    {
        Status = status;
        Data = data;
    }
}