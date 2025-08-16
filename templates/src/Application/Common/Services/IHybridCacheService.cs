namespace Application.Common.Services;

public interface IHybridCacheService
{
    Task<TResult> GetOrCreateAsync<TResult>(
        string key,
        Func<CancellationToken, ValueTask<TResult>> factory,
        CancellationToken cancellationToken = default
    );
}