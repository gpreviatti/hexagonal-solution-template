namespace Application.Common.Services;

public interface IHybridCacheService
{
    ValueTask<TResult> GetOrCreateAsync<TResult>(
        string key,
        Func<CancellationToken, ValueTask<TResult>> factory,
        CancellationToken cancellationToken
    );
}
