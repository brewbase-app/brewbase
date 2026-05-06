namespace brewbase.server.Services;

public enum TastingSessionWriteStatus
{
    Success,
    Unauthorized,
    TastingSessionNotFound,
    CoffeeNotFound,
    CoffeeAlreadyAdded,
    CoffeeNotInSession
}

public sealed class TastingSessionWriteResult<T>
{
    public TastingSessionWriteStatus Status { get; }
    public T? Data { get; }

    public TastingSessionWriteResult(TastingSessionWriteStatus status, T? data = default)
    {
        Status = status;
        Data = data;
    }
}