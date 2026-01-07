using Domain.Notifications;

namespace UnitTests.Domain;

public sealed class NotificationTests
{
    [Fact(DisplayName = nameof(Given_A_New_Notification_When_Properties_Are_Provided_Then_Should_Create_Notification_With_Success))]
    public void Given_A_New_Notification_When_Properties_Are_Provided_Then_Should_Create_Notification_With_Success()
    {
        /// Arrange
        var notificationType = "TestNotification";
        var notificationStatus = "Success";
        var createdBy = "System";
        var message = new { Test = "Message" };

        /// Act
        Notification notification = new(notificationType, notificationStatus, createdBy, message);

        // Assert
        Assert.NotNull(notification);
        Assert.Equal(notificationType, notification.NotificationType);
        Assert.Equal(notificationStatus, notification.NotificationStatus);
        Assert.Equal(createdBy, notification.CreatedBy);
        Assert.NotNull(notification.Message);
        Assert.Contains("\"Test\":\"Message\"", notification.Message);
    }

    [Fact(DisplayName = nameof(Given_A_New_Notification_When_Message_Is_Null_Then_Should_Create_Notification_With_Empty_Message))]
    public void Given_A_New_Notification_When_Message_Is_Null_Then_Should_Create_Notification_With_Empty_Message()
    {
        /// Arrange
        var notificationType = "TestNotification";
        var notificationStatus = "Success";
        var createdBy = "System";
        object? message = null;

        /// Act
        Notification notification = new(notificationType, notificationStatus, createdBy, message);

        // Assert
        Assert.NotNull(notification);
        Assert.Equal(notificationType, notification.NotificationType);
        Assert.Equal(notificationStatus, notification.NotificationStatus);
        Assert.Equal(createdBy, notification.CreatedBy);
        Assert.NotNull(notification.Message);
        Assert.Empty(notification.Message);
    }
}