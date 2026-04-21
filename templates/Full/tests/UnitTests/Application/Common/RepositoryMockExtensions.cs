using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Application.Common.Repositories;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace UnitTests.Application.Common;

internal static class RepositoryMockExtensions
{
    public static void SetSuccessfulAddAsync<TEntity>(this Mock<IBaseRepository> mockRepository) where TEntity : DomainEntity => mockRepository
        .Setup(d => d.AddAsync(It.IsAny<TEntity>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);

    public static void SetFailedAddAsync<TEntity>(this Mock<IBaseRepository> mockRepository) where TEntity : DomainEntity => mockRepository
        .Setup(d => d.AddAsync(It.IsAny<TEntity>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(0);

    public static void SetupQueryable<TEntity>(this Mock<IBaseRepository> mockRepository, ICollection<TEntity> entities) where TEntity : DomainEntity => mockRepository
            .Setup(r => r.GetQueryable<TEntity>(It.IsAny<Guid>(), It.IsAny<bool?>(), It.IsAny<string>())).Returns(entities.BuildMock());

    public static void SetupQueryable<TEntity>(this Mock<IBaseRepository> mockRepository, Guid correlationId, bool? newContext, ICollection<TEntity> entities) where TEntity : DomainEntity => mockRepository
            .Setup(r => r.GetQueryable<TEntity>(correlationId, newContext, It.IsAny<string>())).Returns(entities.BuildMock());

    public static void VerifyAddAsync<TEntity>(this Mock<IBaseRepository> mockRepository, int times) where TEntity : DomainEntity => mockRepository.Verify(
        d => d.AddAsync(It.IsAny<TEntity>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
        Times.Exactly(times)
    );

    public static void VerifyUpdate<TEntity>(this Mock<IBaseRepository> mockRepository, int times) where TEntity : DomainEntity => mockRepository.Verify(
        d => d.UpdateAsync(It.IsAny<TEntity>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
        Times.Exactly(times)
    );

    public static void SetSuccessfulUpdate<TEntity>(this Mock<IBaseRepository> mockRepository) where TEntity : DomainEntity => mockRepository
        .Setup(d => d.UpdateAsync(It.IsAny<TEntity>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);

    public static void SetFailedUpdate<TEntity>(this Mock<IBaseRepository> mockRepository) where TEntity : DomainEntity => mockRepository
        .Setup(d => d.UpdateAsync(It.IsAny<TEntity>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(0);

    public static void SetValidGetAllPaginatedAsyncNoIncludes<TEntity, TResult>(
        this Mock<IBaseRepository> mockRepository,
        IEnumerable<TResult> data,
        int totalRecords
    ) where TEntity : DomainEntity where TResult : class => mockRepository.Setup(r => r.GetAllPaginatedAsync(
        It.IsAny<Guid>(),
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<Expression<Func<TEntity, TResult>>>(),
        It.IsAny<CancellationToken>(),
        It.IsAny<string?>(),
        It.IsAny<bool>(),
        It.IsAny<Dictionary<string, string>>(),
        It.IsAny<Expression<Func<TEntity, bool>>>()
    )).ReturnsAsync((data, totalRecords));

    public static void SetInvalidGetAllPaginatedAsync<TEntity, TResult>(this Mock<IBaseRepository> mockRepository) where TEntity : DomainEntity where TResult : class => mockRepository.Setup(r => r.GetAllPaginatedAsync(
        It.IsAny<Guid>(),
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<Expression<Func<TEntity, TResult>>>(),
        It.IsAny<CancellationToken>(),
        It.IsAny<string?>(),
        It.IsAny<bool>(),
        It.IsAny<Dictionary<string, string>?>(),
        It.IsAny<Expression<Func<TEntity, bool>>>()
    )).ReturnsAsync(([], 0));

    public static void VerifyGetAllPaginatedNoIncludes<TEntity, TResult>(this Mock<IBaseRepository> mockRepository, int times = 1) where TEntity : DomainEntity where TResult : class => mockRepository
        .Verify(r => r.GetAllPaginatedAsync(
            It.IsAny<Guid>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Expression<Func<TEntity, TResult>>>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<string?>(),
            It.IsAny<bool>(),
            It.IsAny<Dictionary<string, string>?>(),
            It.IsAny<Expression<Func<TEntity, bool>>>()
    ), Times.Exactly(times));

    public static void VerifyQueryable<TEntity>(this Mock<IBaseRepository> mockRepository, int times = 1) where TEntity : DomainEntity => mockRepository
        .Verify(r => r.GetQueryable<TEntity>(It.IsAny<Guid>(), It.IsAny<bool?>(), It.IsAny<string>()
    ), Times.Exactly(times));
}

/// <summary>
/// Provides extension methods to build EF Core-compatible async queryable over in-memory collections for testing purposes
/// </summary>
internal static class MockQueryableExtensions
{
    /// <summary>
    /// Builds an async EF Core-compatible <see cref="IQueryable{T}"/> over the supplied collection.
    /// </summary>
    /// <typeparam name="TEntity">Entity type exposed by the queryable.</typeparam>
    /// <param name="data">The in-memory data source.</param>
    /// <returns>An <see cref="IQueryable{T}"/> that supports EF Core async query operators.</returns>
    public static IQueryable<TEntity> BuildMock<TEntity>(this ICollection<TEntity> data)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(data);
        return new TestAsyncEnumerableEfCore<TEntity, TestExpressionVisitor>(data, entity => data.Remove(entity));
    }

    /// <summary>
    /// Builds an async EF Core-compatible <see cref="IQueryable{T}"/> over the supplied collection using a custom expression visitor.
    /// </summary>
    /// <typeparam name="TEntity">Entity type exposed by the queryable.</typeparam>
    /// <typeparam name="TExpressionVisitor">Custom expression visitor used to rewrite query expressions.</typeparam>
    /// <param name="data">The in-memory data source.</param>
    /// <returns>An <see cref="IQueryable{T}"/> that supports EF Core async query operators.</returns>
    public static IQueryable<TEntity> BuildMock<TEntity, TExpressionVisitor>(this ICollection<TEntity> data)
        where TEntity : class
        where TExpressionVisitor : ExpressionVisitor, new()
    {
        ArgumentNullException.ThrowIfNull(data);
        return new TestAsyncEnumerableEfCore<TEntity, TExpressionVisitor>(data, entity => data.Remove(entity));
    }
}

internal sealed class TestExpressionVisitor : ExpressionVisitor;

internal abstract class TestQueryProvider<T, TExpressionVisitor> : IOrderedQueryable<T>, IQueryProvider
    where TExpressionVisitor : ExpressionVisitor, new()
{
    private IEnumerable<T>? _enumerable;

    protected TestQueryProvider(Expression expression) => Expression = expression;

    protected TestQueryProvider(IEnumerable<T> enumerable)
    {
        _enumerable = enumerable;
        Expression = enumerable.AsQueryable().Expression;
    }

    public Type ElementType => typeof(T);

    public Expression Expression { get; }

    public IQueryProvider Provider => this;

    public IQueryable CreateQuery(Expression expression)
    {
        if (expression is MethodCallExpression methodCallExpression)
        {
            var resultType = methodCallExpression.Method.ReturnType;
            var elementType = resultType.GetGenericArguments().First();
            return (IQueryable) CreateInstance(elementType, expression);
        }

        return CreateQuery<T>(expression);
    }

    public IQueryable<TEntity> CreateQuery<TEntity>(Expression expression) => (IQueryable<TEntity>) CreateInstance(typeof(TEntity), expression);

    public object? Execute(Expression expression) => CompileExpressionItem<object?>(expression);

    public virtual TResult Execute<TResult>(Expression expression) => CompileExpressionItem<TResult>(expression);

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        _enumerable ??= CompileExpressionItem<IEnumerable<T>>(Expression);
        return _enumerable.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        _enumerable ??= CompileExpressionItem<IEnumerable<T>>(Expression);
        return _enumerable.GetEnumerator();
    }

    protected abstract object CreateInstance(Type elementType, Expression expression);

    private static TResult CompileExpressionItem<TResult>(Expression expression)
    {
        var visitor = new TExpressionVisitor();
        var visitedExpression = visitor.Visit(expression)
            ?? throw new InvalidOperationException("The visited expression cannot be null.");

        var lambda = Expression.Lambda<Func<TResult>>(visitedExpression, (IEnumerable<ParameterExpression>?) null);
        return lambda.Compile().Invoke();
    }
}

internal sealed class TestAsyncEnumerator<T>(IEnumerator<T> enumerator) : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));

    public T Current => _enumerator.Current;

    public ValueTask DisposeAsync()
    {
        _enumerator.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_enumerator.MoveNext());
}

internal sealed class TestAsyncEnumerableEfCore<T, TExpressionVisitor> : TestQueryProvider<T, TExpressionVisitor>, IAsyncEnumerable<T>, IAsyncQueryProvider
    where TExpressionVisitor : ExpressionVisitor, new()
{
    private readonly Action<T>? _removeCallback;

    public TestAsyncEnumerableEfCore(Expression expression, Action<T>? removeCallback)
        : base(expression) => _removeCallback = removeCallback;

    public TestAsyncEnumerableEfCore(IEnumerable<T> enumerable, Action<T>? removeCallback)
        : base(enumerable) => _removeCallback = removeCallback;

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethods()
            .First(method => method.Name == nameof(IQueryProvider.Execute) && method.IsGenericMethod)
            .MakeGenericMethod(expectedResultType)
            .Invoke(this, [expression]);

        return (TResult) typeof(Task)
            .GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(expectedResultType)
            .Invoke(null, [executionResult])!;
    }

    public override TResult Execute<TResult>(Expression expression)
    {
        if (expression is MethodCallExpression
            {
                Method.Name: nameof(EntityFrameworkQueryableExtensions.ExecuteUpdate)
                or nameof(EntityFrameworkQueryableExtensions.ExecuteDelete)
            } methodCallExpression && typeof(TResult) == typeof(int))
        {
            var affectedItems = base.Execute<IEnumerable<T>>(Expression).ToList();

            if (methodCallExpression.Method.Name == nameof(EntityFrameworkQueryableExtensions.ExecuteUpdate))
            {
                ApplyUpdateChanges(affectedItems, methodCallExpression);
            }

            if (methodCallExpression.Method.Name == nameof(EntityFrameworkQueryableExtensions.ExecuteDelete))
            {
                foreach (var item in affectedItems)
                {
                    _removeCallback?.Invoke(item);
                }
            }

            return (TResult) (object) affectedItems.Count;
        }

        return base.Execute<TResult>(expression);
    }

    protected override object CreateInstance(Type elementType, Expression expression)
    {
        var queryType = GetType().GetGenericTypeDefinition().MakeGenericType(elementType, typeof(TExpressionVisitor));
        return typeof(T) == elementType
            ? Activator.CreateInstance(queryType, expression, _removeCallback)!
            : Activator.CreateInstance(queryType, expression, null)!;
    }

    private static void ApplyUpdateChanges(IEnumerable<T> affectedItems, MethodCallExpression methodCallExpression)
    {
        if (methodCallExpression.Arguments[1] is not NewArrayExpression updateExpressions)
        {
            return;
        }

        foreach (var updateExpression in updateExpressions.Expressions.Cast<NewExpression>())
        {
            var propertyLambda = ExtractLambda(updateExpression.Arguments[0]);
            if (propertyLambda is null)
            {
                continue;
            }

            foreach (var item in affectedItems)
            {
                var value = ExtractValue(updateExpression.Arguments[1], item);
                SetProperty(item, propertyLambda, value);
            }
        }
    }

    private static object? ExtractValue(Expression expression, T item)
    {
        if (expression is UnaryExpression { NodeType: ExpressionType.Quote } quoteExpression)
        {
            expression = quoteExpression.Operand;
        }

        if (expression is ConstantExpression constantExpression)
        {
            return constantExpression.Value;
        }

        if (expression is LambdaExpression lambdaExpression)
        {
            var compiledLambda = lambdaExpression.Compile();
            return lambdaExpression.Parameters.Count switch
            {
                0 => compiledLambda.DynamicInvoke(),
                1 => compiledLambda.DynamicInvoke(item),
                _ => throw new InvalidOperationException("Only lambdas with zero or one parameter are supported.")
            };
        }

        var parameterlessLambda = Expression.Lambda(expression);
        return parameterlessLambda.Compile().DynamicInvoke();
    }

    private static LambdaExpression? ExtractLambda(Expression expression)
    {
        if (expression is UnaryExpression { NodeType: ExpressionType.Quote } quoteExpression)
            expression = quoteExpression.Operand;

        if (expression is UnaryExpression { NodeType: ExpressionType.Convert } convertExpression)
            expression = convertExpression.Operand;

        return expression as LambdaExpression;
    }

    private static void SetProperty(T item, LambdaExpression propertyLambda, object? value)
    {
        var body = propertyLambda.Body;
        if (body is UnaryExpression { NodeType: ExpressionType.Convert } convertExpression)
        {
            body = convertExpression.Operand;
        }

        if (body is not MemberExpression { Member: PropertyInfo propertyInfo })
        {
            return;
        }

        var convertedValue = value;
        if (value is not null && !propertyInfo.PropertyType.IsInstanceOfType(value))
        {
#pragma warning disable CA1305 // Specify IFormatProvider
            convertedValue = Convert.ChangeType(value, propertyInfo.PropertyType);
#pragma warning restore CA1305 // Specify IFormatProvider
        }

        propertyInfo.SetValue(item, convertedValue);
    }
}
