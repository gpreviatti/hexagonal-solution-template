using Domain.Notifications;

namespace UnitTests.Domain;

public sealed class NotificationTests
{
    [Fact(DisplayName = nameof(GivenANewNotificationWhenPropertiesAreProvidedThenShouldCreateNotificationWithSuccess))]
    public void GivenANewNotificationWhenPropertiesAreProvidedThenShouldCreateNotificationWithSuccess()
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

    [Fact(DisplayName = nameof(GivenANewNotificationWhenMessageIsNullThenShouldCreateNotificationWithEmptyMessage))]
    public void GivenANewNotificationWhenMessageIsNullThenShouldCreateNotificationWithEmptyMessage()
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