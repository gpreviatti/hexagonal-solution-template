using Contracts.Common;

namespace UnitTests.Common;

public sealed class BaseRequestTests
{
    [Fact(DisplayName = nameof(GivenABaseRequestWhenInstantiatedThenShouldAssignCorrelationId))]
    public void GivenABaseRequestWhenInstantiatedThenShouldAssignCorrelationId()
    {
        var correlationId = Guid.NewGuid();

        var request = new BaseRequest(correlationId);

        Assert.Equal(correlationId, request.CorrelationId);
    }

    [Fact(DisplayName = nameof(GivenABasePaginatedRequestWhenUsingDefaultValuesThenShouldAssignExpectedDefaults))]
    public void GivenABasePaginatedRequestWhenUsingDefaultValuesThenShouldAssignExpectedDefaults()
    {
        var correlationId = Guid.NewGuid();

        var request = new BasePaginatedRequest(correlationId);

        Assert.Equal(correlationId, request.CorrelationId);
        Assert.Equal(1, request.Page);
        Assert.Equal(10, request.PageSize);
        Assert.Null(request.SortBy);
        Assert.False(request.SortDescending);
        Assert.Null(request.SearchByValues);
    }

    [Fact(DisplayName = nameof(GivenABasePaginatedRequestWhenInstantiatedThenShouldAssignCustomValues))]
    public void GivenABasePaginatedRequestWhenInstantiatedThenShouldAssignCustomValues()
    {
        var correlationId = Guid.NewGuid();
        var searchByValues = new Dictionary<string, string>
        {
            ["status"] = "active"
        };

        var request = new BasePaginatedRequest(
            correlationId,
            Page: 2,
            PageSize: 25,
            SortBy: "CreatedAt",
            SortDescending: true,
            SearchByValues: searchByValues
        );

        Assert.Equal(correlationId, request.CorrelationId);
        Assert.Equal(2, request.Page);
        Assert.Equal(25, request.PageSize);
        Assert.Equal("CreatedAt", request.SortBy);
        Assert.True(request.SortDescending);
        Assert.Equal(searchByValues, request.SearchByValues);
    }
}
