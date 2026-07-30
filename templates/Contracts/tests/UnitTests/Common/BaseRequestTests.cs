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

    [Fact(DisplayName = nameof(GivenABaseRequestWhenInstantiatedThenShouldAssignDefaultUserAndTimezone))]
    public void GivenABaseRequestWhenInstantiatedThenShouldAssignDefaultUserAndTimezone()
    {
        var request = new BaseRequest(Guid.NewGuid());

        Assert.Equal(string.Empty, request.User);
        Assert.Equal(string.Empty, request.TimezoneId);
    }

    [Fact(DisplayName = nameof(GivenABaseRequestWhenInstantiatedWithCustomUserAndTimezoneThenShouldAssignValues))]
    public void GivenABaseRequestWhenInstantiatedWithCustomUserAndTimezoneThenShouldAssignValues()
    {
        var request = new BaseRequest(Guid.NewGuid(), User: "alice", TimezoneId: "America/New_York");

        Assert.Equal("alice", request.User);
        Assert.Equal("America/New_York", request.TimezoneId);
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
        Assert.Equal(string.Empty, request.User);
        Assert.Equal(string.Empty, request.TimezoneId);
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
            SearchByValues: searchByValues,
            User: "bob",
            TimezoneId: "Europe/London"
        );

        Assert.Equal(correlationId, request.CorrelationId);
        Assert.Equal(2, request.Page);
        Assert.Equal(25, request.PageSize);
        Assert.Equal("CreatedAt", request.SortBy);
        Assert.True(request.SortDescending);
        Assert.Equal(searchByValues, request.SearchByValues);
        Assert.Equal("bob", request.User);
        Assert.Equal("Europe/London", request.TimezoneId);
    }

    [Fact(DisplayName = nameof(GivenABaseFullTextSearchPaginatedRequestWhenUsingDefaultValuesThenShouldAssignExpectedDefaults))]
    public void GivenABaseFullTextSearchPaginatedRequestWhenUsingDefaultValuesThenShouldAssignExpectedDefaults()
    {
        var correlationId = Guid.NewGuid();

        var request = new BaseFullTextSearchPaginatedRequest(correlationId);

        Assert.Equal(correlationId, request.CorrelationId);
        Assert.Equal(1, request.Page);
        Assert.Equal(10, request.PageSize);
        Assert.Null(request.SortBy);
        Assert.False(request.SortDescending);
        Assert.Equal(string.Empty, request.SearchValue);
        Assert.Equal(string.Empty, request.User);
        Assert.Equal(string.Empty, request.TimezoneId);
    }

    [Fact(DisplayName = nameof(GivenABaseFullTextSearchPaginatedRequestWhenInstantiatedThenShouldAssignCustomValues))]
    public void GivenABaseFullTextSearchPaginatedRequestWhenInstantiatedThenShouldAssignCustomValues()
    {
        var correlationId = Guid.NewGuid();

        var request = new BaseFullTextSearchPaginatedRequest(
            correlationId,
            Page: 3,
            PageSize: 20,
            SortBy: "Name",
            SortDescending: true,
            SearchValue: "laptop",
            User: "carol",
            TimezoneId: "Asia/Tokyo"
        );

        Assert.Equal(correlationId, request.CorrelationId);
        Assert.Equal(3, request.Page);
        Assert.Equal(20, request.PageSize);
        Assert.Equal("Name", request.SortBy);
        Assert.True(request.SortDescending);
        Assert.Equal("laptop", request.SearchValue);
        Assert.Equal("carol", request.User);
        Assert.Equal("Asia/Tokyo", request.TimezoneId);
    }

    [Fact(DisplayName = nameof(GivenABaseFullTextSearchPaginatedRequestThenShouldInheritFromBaseRequest))]
    public void GivenABaseFullTextSearchPaginatedRequestThenShouldInheritFromBaseRequest()
    {
        var request = new BaseFullTextSearchPaginatedRequest(Guid.NewGuid());

        Assert.IsAssignableFrom<BaseRequest>(request);
    }

    [Fact(DisplayName = nameof(GivenABasePaginatedRequestThenShouldInheritFromBaseRequest))]
    public void GivenABasePaginatedRequestThenShouldInheritFromBaseRequest()
    {
        var request = new BasePaginatedRequest(Guid.NewGuid());

        Assert.IsAssignableFrom<BaseRequest>(request);
    }
}
