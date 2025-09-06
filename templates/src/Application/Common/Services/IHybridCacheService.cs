namespace Application.Common.Services;

public interface IHybridCacheService
{
    ValueTask<TResult?> GetAsync<TResult>(
        string key,
        CancellationToken cancellationToken
    );

    ValueTask SetAsync<TValue>(
        string key,
        TValue value,
        CancellationToken cancellationToken
    );

    ValueTask<TResult> GetOrCreateAsync<TResult>(
        string key,
        Func<CancellationToken, ValueTask<TResult>> factory,
        CancellationToken cancellationToken
    );
}
