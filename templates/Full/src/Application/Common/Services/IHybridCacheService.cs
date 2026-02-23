namespace Application.Common.Services;

public interface IHybridCacheService
{
    ValueTask<TResult> GetOrCreateAsync<TResult>(
        Guid correlationId,
        string key,
        Func<CancellationToken, ValueTask<TResult>> factory,
        CancellationToken cancellationToken
    );

    ValueTask CreateAsync<TResult>(
        Guid correlationId,
        string key,
        TResult value,
        CancellationToken cancellationToken
    );

    ValueTask DeleteAsync(Guid correlationId, string key, CancellationToken cancellationToken);
}
