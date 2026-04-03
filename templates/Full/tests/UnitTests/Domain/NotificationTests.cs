using Domain.Common.Enums;
using Domain.Notifications;

namespace UnitTests.Domain;

public sealed class NotificationTests
{
    [Fact(DisplayName = nameof(GivenANewNotificationWhenPropertiesAreProvidedThenShouldCreateNotificationWithSuccess))]
    public void GivenANewNotificationWhenPropertiesAreProvidedThenShouldCreateNotificationWithSuccess()
    {
        /// Arrange
        var notificationType = NotificationType.OrderCreated;
        var notificationStatus = "Success";
        var createdBy = "System";
        var timezoneId = "America/New_York";
        var message = new { Test = "Message" };

        /// Act
        Notification notification = new(notificationType, notificationStatus, message, createdBy, timezoneId);

        // Assert
        Assert.NotNull(notification);
        Assert.Equal(notificationType, notification.NotificationType);
        Assert.Equal(notificationStatus, notification.NotificationStatus);
        Assert.Equal(createdBy, notification.CreatedBy);
        Assert.Equal(timezoneId, notification.CreatedByTimezoneId);
        Assert.NotNull(notification.Message);
        Assert.Contains("\"Test\":\"Message\"", notification.Message);
    }

    [Fact(DisplayName = nameof(GivenANewNotificationWhenMessageIsNullThenShouldCreateNotificationWithEmptyMessage))]
    public void GivenANewNotificationWhenMessageIsNullThenShouldCreateNotificationWithEmptyMessage()
    {
        /// Arrange
        var notificationType = NotificationType.OrderCreated;
        var notificationStatus = "Success";
        var createdBy = "System";
        var timezoneId = "America/New_York";
        object? message = null;

        /// Act
        Notification notification = new(notificationType, notificationStatus, message, createdBy, timezoneId);

        // Assert
        Assert.NotNull(notification);
        Assert.Equal(notificationType, notification.NotificationType);
        Assert.Equal(notificationStatus, notification.NotificationStatus);
        Assert.Equal(createdBy, notification.CreatedBy);
        Assert.NotNull(notification.Message);
        Assert.Empty(notification.Message);
    }
}