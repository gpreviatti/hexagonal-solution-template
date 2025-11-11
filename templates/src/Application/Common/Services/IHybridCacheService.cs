namespace Application.Common.Services;

public interface IHybridCacheService
{
    ValueTask<TResult> GetOrCreateAsync<TResult>(
        string key,
        Func<CancellationToken, ValueTask<TResult>> factory,
        CancellationToken cancellationToken
    );

    ValueTask CreateAsync<TResult>(
        string key,
        Func<CancellationToken, ValueTask<TResult>> factory,
        CancellationToken cancellationToken
    );

    ValueTask DeleteAsync(string key, CancellationToken cancellationToken);
}
