using Contracts.Common;
using Contracts.Orders;

namespace UnitTests.Common;

public sealed class BaseResponseTests
{
    [Fact(DisplayName = nameof(GivenABaseResponseWhenInstantiatedWithDefaultsThenShouldKeepDefaultValues))]
    public void GivenABaseResponseWhenInstantiatedWithDefaultsThenShouldKeepDefaultValues()
    {
        var response = new BaseResponse();

        Assert.False(response.Success);
        Assert.Null(response.Message);
    }

    [Fact(DisplayName = nameof(GivenABaseResponseWhenInstantiatedThenShouldAssignSuccessAndMessage))]
    public void GivenABaseResponseWhenInstantiatedThenShouldAssignSuccessAndMessage()
    {
        var response = new BaseResponse(true, "ok");

        Assert.True(response.Success);
        Assert.Equal("ok", response.Message);
    }

    [Fact(DisplayName = nameof(GivenABaseResponseOfTDataWhenInstantiatedThenShouldAssignSuccessMessageAndData))]
    public void GivenABaseResponseOfTDataWhenInstantiatedThenShouldAssignSuccessMessageAndData()
    {
        var data = new OrderDto
        {
            Id = 1,
            Description = "Order 1",
            Total = 99.9m
        };

        var response = new BaseResponse<OrderDto>(true, data, "created");

        Assert.True(response.Success);
        Assert.Equal("created", response.Message);
        Assert.Equal(data, response.Data);
    }

    [Fact(DisplayName = nameof(GivenABasePaginatedResponseWhenInstantiatedThenShouldAssignPaginationAndData))]
    public void GivenABasePaginatedResponseWhenInstantiatedThenShouldAssignPaginationAndData()
    {
        ItemDto[] data =
        [
            new()
            {
                Id = 1,
                Name = "Item 1",
                Description = "Desc 1",
                Value = 10m
            }
        ];

        var response = new BasePaginatedResponse<ItemDto>(
            success: true,
            totalPages: 4,
            totalRecords: 30,
            data,
            message: "ok"
        );

        Assert.True(response.Success);
        Assert.Equal("ok", response.Message);
        Assert.Equal(4, response.TotalPages);
        Assert.Equal(30, response.TotalRecords);
        Assert.Equal(data, response.Data);
    }
}
