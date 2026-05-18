using Application.Common.Messages;
using Application.Orders;
using Domain.Common.Enums;
using Domain.Orders;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders;

public sealed class CreateOrderUseCaseFixture : BaseApplicationFixture<CreateOrderRequest, CreateOrderUseCase>
{
    public CreateOrderUseCaseFixture() => UseCase = new(MockServiceProvider.Object);

    public CreateOrderRequest SetValidRequest()
    {
        var items = AutoFixture.Build<CreateOrderItemRequest>()
            .With(i => i.Name, "Test Item")
            .With(i => i.Description, "Test Description")
            .With(i => i.Value, 100m)
            .CreateMany(1);

        return new(Guid.NewGuid(), "AwesomeComputer", [.. items]);
    }

    public static CreateOrderRequest SetInvalidRequestWithNoItems() =>
        new(Guid.NewGuid(), "AwesomeComputer", []);

    public static CreateOrderRequest SetInvalidRequest() =>
        new(Guid.Empty, "AwesomeComputer", [new("Item1", "Desc1", 1m)]);
}

public sealed class CreateOrderUseCaseTest : IClassFixture<CreateOrderUseCaseFixture>
{
    private readonly CreateOrderUseCaseFixture _fixture;

    public CreateOrderUseCaseTest(CreateOrderUseCaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var request = _fixture.SetValidRequest();

        _fixture.MockRepository.SetSuccessfulAddAsync<Order>();

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Null(result.Message);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockLogger.VerifyWarning("Order must have at least one item.", 0);
        _fixture.MockLogger.VerifyWarning("Failed to create order.", 0);
        _fixture.MockRepository.VerifyAddAsync<Order>(1);
        _fixture.VerifyProduce<CreateNotificationMessage>();
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFails))]
    public async Task GivenAnInvalidRequestThenFails()
    {
        // Arrange
        var request = CreateOrderUseCaseFixture.SetInvalidRequest();

        // Act
        var result = await _fixture.UseCase.HandleAsync(
            request,
            _fixture.CancellationToken
        );

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation(0);
        _fixture.MockLogger.VerifyWarning("Order must have at least one item.", 0);
        _fixture.MockLogger.VerifyWarning("Failed to create order.", 0);
        _fixture.MockRepository.VerifyAddAsync<Order>(0);
        _fixture.VerifyProduce<CreateNotificationMessage>(0);
    }

    [Fact(DisplayName = nameof(GivenAInvalidRequestThenFailsWhenThereIsNoItems))]
    public async Task GivenAInvalidRequestThenFailsWhenThereIsNoItems()
    {
        // Arrange
        var request = CreateOrderUseCaseFixture.SetInvalidRequestWithNoItems();


        // Act
        var result = await _fixture.UseCase.HandleAsync(
            request,
            _fixture.CancellationToken
        );

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);
        Assert.Equal("Order must have at least one item.", result.Message);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockLogger.VerifyWarning("Order must have at least one item.", 1);
        _fixture.MockLogger.VerifyWarning("Failed to create order.", 0);
        _fixture.MockRepository.VerifyAddAsync<Order>(0);
        _fixture.VerifyProduce<CreateNotificationMessage>();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenFailsWhenRepositoryReturnsZero))]
    public async Task GivenAValidRequestThenFailsWhenRepositoryReturnsZero()
    {
        // Arrange
        var request = _fixture.SetValidRequest();

        _fixture.MockRepository.SetFailedAddAsync<Order>();

        // Act
        var result = await _fixture.UseCase.HandleAsync(
            request,
            _fixture.CancellationToken
        );

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);
        Assert.Equal("Failed to create order.", result.Message);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockLogger.VerifyWarning("Order must have at least one item.", 0);
        _fixture.MockLogger.VerifyWarning("Failed to create order.", 1);
        _fixture.MockRepository.VerifyAddAsync<Order>(1);
        _fixture.VerifyProduce<CreateNotificationMessage>();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenSuccessThenResponseContainsAllItemDtoFields))]
    public async Task GivenAValidRequestWhenSuccessThenResponseContainsAllItemDtoFields()
    {
        // Arrange
        var item1 = new CreateOrderItemRequest("Item1", "Description1", 100m);
        var item2 = new CreateOrderItemRequest("Item2", "Description2", 200m);
        var request = new CreateOrderRequest(Guid.NewGuid(), "Test Order", [item1, item2]);


        _fixture.MockRepository.SetSuccessfulAddAsync<Order>();

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Items);
        Assert.Equal(2, result.Data.Items.Count);

        var itemsList = result.Data.Items.ToList();

        // Verify Item 1 DTO fields
        var dtoItem1 = itemsList[0];
        Assert.Equal("Item1", dtoItem1.Name);
        Assert.Equal("Description1", dtoItem1.Description);
        Assert.Equal(100m, dtoItem1.Value);

        // Verify Item 2 DTO fields
        var dtoItem2 = itemsList[1];
        Assert.Equal("Item2", dtoItem2.Name);
        Assert.Equal("Description2", dtoItem2.Description);
        Assert.Equal(200m, dtoItem2.Value);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenSuccessThenResponseContainsOrderDtoWithCorrectTotal))]
    public async Task GivenAValidRequestWhenSuccessThenResponseContainsOrderDtoWithCorrectTotal()
    {
        // Arrange
        var item1 = new CreateOrderItemRequest("Item1", "Description1", 150m);
        var item2 = new CreateOrderItemRequest("Item2", "Description2", 250m);
        var request = new CreateOrderRequest(Guid.NewGuid(), "Test Order", [item1, item2]);


        _fixture.MockRepository.SetSuccessfulAddAsync<Order>();

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(400m, result.Data.Total);
        Assert.NotNull(result.Data.PeriodSinceWasCreated);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenSuccessThenShouldPublishSuccessNotification))]
    public async Task GivenAValidRequestWhenSuccessThenShouldPublishSuccessNotification()
    {
        // Arrange
        var request = _fixture.SetValidRequest();

        _fixture.MockRepository.SetSuccessfulAddAsync<Order>();

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        _fixture.VerifyProduce<CreateNotificationMessage>(1);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenFailureThenShouldPublishFailureNotification))]
    public async Task GivenAValidRequestWhenFailureThenShouldPublishFailureNotification()
    {
        // Arrange
        var request = CreateOrderUseCaseFixture.SetInvalidRequestWithNoItems();

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        _fixture.VerifyProduce<CreateNotificationMessage>(1);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenSuccessThenShouldPublishSuccessNotificationWithExpectedStatus))]
    public async Task GivenAValidRequestWhenSuccessThenShouldPublishSuccessNotificationWithExpectedStatus()
    {
        // Arrange
        var request = new CreateOrderRequest(
            Guid.NewGuid(),
            "Status Test Order",
            [new("Item1", "Description1", 100m)],
            "TestUser"
        );
        CreateNotificationMessage? publishedMessage = null;
        string? publishedQueue = null;

        _fixture.MockRepository.SetSuccessfulAddAsync<Order>();
        _fixture.MockProduceService
            .Setup(p => p.HandleAsync(
                It.IsAny<CreateNotificationMessage>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Callback<CreateNotificationMessage, CancellationToken, string, string>((message, _, queue, _) =>
            {
                publishedMessage = message;
                publishedQueue = queue;
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(publishedMessage);
        Assert.Equal(NotificationStatus.Success, publishedMessage!.NotificationStatus);
        Assert.Equal(NotificationType.OrderCreated, publishedMessage.NotificationType);
        Assert.Equal("TestUser", publishedMessage.CreatedBy);
        Assert.Equal(NotificationType.OrderCreated.ToString(), publishedQueue);
    }

    [Fact(DisplayName = nameof(GivenAInvalidRequestWithNoItemsThenShouldPublishFailureNotificationWithExpectedStatus))]
    public async Task GivenAInvalidRequestWithNoItemsThenShouldPublishFailureNotificationWithExpectedStatus()
    {
        // Arrange
        var request = new CreateOrderRequest(
            Guid.NewGuid(),
            "Failed Order",
            [],
            "TestUser"
        );
        CreateNotificationMessage? publishedMessage = null;

        _fixture.MockProduceService
            .Setup(p => p.HandleAsync(
                It.IsAny<CreateNotificationMessage>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Callback<CreateNotificationMessage, CancellationToken, string, string>((message, _, _, _) =>
            {
                publishedMessage = message;
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(publishedMessage);
        Assert.Equal(NotificationStatus.Failed, publishedMessage!.NotificationStatus);
        Assert.Equal(NotificationType.OrderCreated, publishedMessage.NotificationType);
        Assert.Equal("TestUser", publishedMessage.CreatedBy);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenRepositoryReturnsZeroThenShouldPublishFailureNotificationWithExpectedStatus))]
    public async Task GivenAValidRequestWhenRepositoryReturnsZeroThenShouldPublishFailureNotificationWithExpectedStatus()
    {
        // Arrange
        var request = new CreateOrderRequest(
            Guid.NewGuid(),
            "Repository Failure Order",
            [new("Item1", "Description1", 100m)],
            "TestUser"
        );
        CreateNotificationMessage? publishedMessage = null;

        _fixture.MockRepository.SetFailedAddAsync<Order>();
        _fixture.MockProduceService
            .Setup(p => p.HandleAsync(
                It.IsAny<CreateNotificationMessage>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Callback<CreateNotificationMessage, CancellationToken, string, string>((message, _, _, _) =>
            {
                publishedMessage = message;
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Failed to create order.", result.Message);
        Assert.NotNull(publishedMessage);
        Assert.Equal(NotificationStatus.Failed, publishedMessage!.NotificationStatus);
        Assert.Equal(NotificationType.OrderCreated, publishedMessage.NotificationType);
        Assert.Equal("TestUser", publishedMessage.CreatedBy);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWithEmptyDescriptionThenValidationShouldFail))]
    public async Task GivenAValidRequestWithEmptyDescriptionThenValidationShouldFail()
    {
        // Arrange
        var request = new CreateOrderRequest(Guid.NewGuid(), "", [new("Item1", "Desc1", 100m)]);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        _fixture.MockRepository.VerifyAddAsync<Order>(0);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWithSingleItemThenShouldCreateOrderSuccessfully))]
    public async Task GivenAValidRequestWithSingleItemThenShouldCreateOrderSuccessfully()
    {
        // Arrange
        var request = new CreateOrderRequest(Guid.NewGuid(), "Single Item Order", [new("Item1", "Description1", 99.99m)]);

        _fixture.MockRepository.SetSuccessfulAddAsync<Order>();

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Items);
        var itemsList = result.Data.Items.ToList();
        Assert.Single(itemsList);
        Assert.Equal(99.99m, result.Data.Total);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWithMultipleItemsThenShouldCreateOrderWithAllItems))]
    public async Task GivenAValidRequestWithMultipleItemsThenShouldCreateOrderWithAllItems()
    {
        // Arrange
        var items = new[]
        {
            new CreateOrderItemRequest("Item1", "Desc1", 100m),
            new CreateOrderItemRequest("Item2", "Desc2", 200m),
            new CreateOrderItemRequest("Item3", "Desc3", 300m),
            new CreateOrderItemRequest("Item4", "Desc4", 400m),
            new CreateOrderItemRequest("Item5", "Desc5", 500m)
        };
        var request = new CreateOrderRequest(Guid.NewGuid(), "Multi Item Order", items);

        _fixture.MockRepository.SetSuccessfulAddAsync<Order>();

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.Items);
        Assert.Equal(5, result.Data.Items.Count);
        Assert.Equal(1500m, result.Data.Total);
    }
}
