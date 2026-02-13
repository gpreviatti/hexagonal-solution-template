using Domain.Common;

namespace UnitTests.Domain;

public sealed class DomainEntityTests
{
    private sealed class TestDomainEntity : DomainEntity
    {
        public TestDomainEntity() : base() { }
        public TestDomainEntity(DateTime currentDate, string? user = null, string timezoneId = "")
            : base(currentDate, user, timezoneId) { }
    }

    [Fact(DisplayName = nameof(ConstructorWithValidParametersShouldCreateEntityWithProvidedValues))]
    public void ConstructorWithValidParametersShouldCreateEntityWithProvidedValues()
    {
        var currentDate = new DateTime(2026, 2, 13, 10, 30, 0, DateTimeKind.Utc);
        var user = "TestUser";
        var timezoneId = "America/New_York";

        var entity = new TestDomainEntity(currentDate, user, timezoneId);

        Assert.Equal(currentDate, entity.CreatedAt);
        Assert.Equal(user, entity.CreatedBy);
        Assert.Equal(currentDate, entity.UpdatedAt);
        Assert.Equal(user, entity.UpdatedBy);
        Assert.Equal(timezoneId, entity.TimezoneId);
    }

    [Fact(DisplayName = nameof(ConstructorWithDefaultDateTimeShouldUseUtcNow))]
    public void ConstructorWithDefaultDateTimeShouldUseUtcNow()
    {
        var beforeCreation = DateTime.UtcNow;

        var entity = new TestDomainEntity(default);

        var afterCreation = DateTime.UtcNow;
        Assert.True(entity.CreatedAt >= beforeCreation && entity.CreatedAt <= afterCreation);
        Assert.Equal(entity.CreatedAt, entity.UpdatedAt);
    }

    [Fact(DisplayName = nameof(ConstructorWithNullUserShouldDefaultToSystem))]
    public void ConstructorWithNullUserShouldDefaultToSystem()
    {
        var currentDate = new DateTime(2026, 2, 13, 10, 30, 0, DateTimeKind.Utc);

        var entity = new TestDomainEntity(currentDate, null);

        Assert.Equal("System", entity.CreatedBy);
        Assert.Equal("System", entity.UpdatedBy);
    }

    [Fact(DisplayName = nameof(ConstructorWithValidTimezoneIdShouldSetTimezoneId))]
    public void ConstructorWithValidTimezoneIdShouldSetTimezoneId()
    {
        var currentDate = new DateTime(2026, 2, 13, 10, 30, 0, DateTimeKind.Utc);
        var timezoneId = "America/Sao_Paulo";

        var entity = new TestDomainEntity(currentDate, "TestUser", timezoneId);

        Assert.Equal(timezoneId, entity.TimezoneId);
    }

    [Fact(DisplayName = nameof(ConstructorWithInvalidTimezoneIdShouldDefaultToUtc))]
    public void ConstructorWithInvalidTimezoneIdShouldDefaultToUtc()
    {
        var currentDate = new DateTime(2026, 2, 13, 10, 30, 0, DateTimeKind.Utc);
        var invalidTimezoneId = "Invalid/Timezone";

        var entity = new TestDomainEntity(currentDate, "TestUser", invalidTimezoneId);

        Assert.Equal(TimeZoneInfo.Utc.Id, entity.TimezoneId);
    }

    [Fact(DisplayName = nameof(ConstructorWithNullTimezoneIdShouldDefaultToUtc))]
    public void ConstructorWithNullTimezoneIdShouldDefaultToUtc()
    {
        var currentDate = new DateTime(2026, 2, 13, 10, 30, 0, DateTimeKind.Utc);
        string? nullTimezone = null;

        var entity = new TestDomainEntity(currentDate, "TestUser", nullTimezone!);

        Assert.Equal(TimeZoneInfo.Utc.Id, entity.TimezoneId);
    }

    [Fact(DisplayName = nameof(ConstructorWithEmptyTimezoneIdShouldDefaultToUtc))]
    public void ConstructorWithEmptyTimezoneIdShouldDefaultToUtc()
    {
        var currentDate = new DateTime(2026, 2, 13, 10, 30, 0, DateTimeKind.Utc);

        var entity = new TestDomainEntity(currentDate, "TestUser", string.Empty);

        Assert.Equal(TimeZoneInfo.Utc.Id, entity.TimezoneId);
    }

    [Fact(DisplayName = nameof(ConstructorWithWhitespaceTimezoneIdShouldDefaultToUtc))]
    public void ConstructorWithWhitespaceTimezoneIdShouldDefaultToUtc()
    {
        var currentDate = new DateTime(2026, 2, 13, 10, 30, 0, DateTimeKind.Utc);

        var entity = new TestDomainEntity(currentDate, "TestUser", "   ");

        Assert.Equal(TimeZoneInfo.Utc.Id, entity.TimezoneId);
    }

    [Fact(DisplayName = nameof(UpdateWithValidParametersShouldUpdateEntityProperties))]
    public void UpdateWithValidParametersShouldUpdateEntityProperties()
    {
        var initialDate = new DateTime(2026, 2, 13, 10, 30, 0, DateTimeKind.Utc);
        var entity = new TestDomainEntity(initialDate, "InitialUser", "America/New_York");
        var beforeUpdate = DateTime.UtcNow;

        entity.Update("UpdatedUser", "Europe/London");

        var afterUpdate = DateTime.UtcNow;
        Assert.True(entity.UpdatedAt >= beforeUpdate && entity.UpdatedAt <= afterUpdate);
        Assert.Equal("UpdatedUser", entity.UpdatedBy);
        Assert.Equal("Europe/London", entity.TimezoneId);
        Assert.Equal(initialDate, entity.CreatedAt);
        Assert.Equal("InitialUser", entity.CreatedBy);
    }

    [Fact(DisplayName = nameof(UpdateWithNullUserShouldDefaultToSystem))]
    public void UpdateWithNullUserShouldDefaultToSystem()
    {
        var initialDate = new DateTime(2026, 2, 13, 10, 30, 0, DateTimeKind.Utc);
        var entity = new TestDomainEntity(initialDate, "InitialUser");

        entity.Update(null);

        Assert.Equal("System", entity.UpdatedBy);
    }

    [Fact(DisplayName = nameof(UpdateWithInvalidTimezoneIdShouldDefaultToUtc))]
    public void UpdateWithInvalidTimezoneIdShouldDefaultToUtc()
    {
        var initialDate = new DateTime(2026, 2, 13, 10, 30, 0, DateTimeKind.Utc);
        var entity = new TestDomainEntity(initialDate, "TestUser", "America/New_York");

        entity.Update("UpdatedUser", "Invalid/Timezone");

        Assert.Equal(TimeZoneInfo.Utc.Id, entity.TimezoneId);
        Assert.Equal("UpdatedUser", entity.UpdatedBy);
    }

    [Fact(DisplayName = nameof(UpdateWithEmptyTimezoneIdShouldDefaultToUtc))]
    public void UpdateWithEmptyTimezoneIdShouldDefaultToUtc()
    {
        var initialDate = new DateTime(2026, 2, 13, 10, 30, 0, DateTimeKind.Utc);
        var entity = new TestDomainEntity(initialDate, "TestUser", "America/New_York");

        entity.Update("UpdatedUser", string.Empty);

        Assert.Equal(TimeZoneInfo.Utc.Id, entity.TimezoneId);
    }

    [Fact(DisplayName = nameof(UpdateWithoutParametersShouldUseDefaultValues))]
    public void UpdateWithoutParametersShouldUseDefaultValues()
    {
        var initialDate = new DateTime(2026, 2, 13, 10, 30, 0, DateTimeKind.Utc);
        var entity = new TestDomainEntity(initialDate, "InitialUser", "America/New_York");
        var beforeUpdate = DateTime.UtcNow;

        entity.Update();

        var afterUpdate = DateTime.UtcNow;
        Assert.True(entity.UpdatedAt >= beforeUpdate && entity.UpdatedAt <= afterUpdate);
        Assert.Equal("System", entity.UpdatedBy);
        Assert.Equal(TimeZoneInfo.Utc.Id, entity.TimezoneId);
    }

    [Fact(DisplayName = nameof(ConstructorWithUtcTimezoneIdShouldSetUtcTimezone))]
    public void ConstructorWithUtcTimezoneIdShouldSetUtcTimezone()
    {
        var currentDate = new DateTime(2026, 2, 13, 10, 30, 0, DateTimeKind.Utc);

        var entity = new TestDomainEntity(currentDate, "TestUser", TimeZoneInfo.Utc.Id);

        Assert.Equal(TimeZoneInfo.Utc.Id, entity.TimezoneId);
    }

    [Fact(DisplayName = nameof(UpdatePreservesCreatedAtAndCreatedByWhenUpdating))]
    public void UpdatePreservesCreatedAtAndCreatedByWhenUpdating()
    {
        var initialDate = new DateTime(2026, 2, 13, 10, 30, 0, DateTimeKind.Utc);
        var initialUser = "InitialUser";
        var entity = new TestDomainEntity(initialDate, initialUser, "America/New_York");

        entity.Update("UpdatedUser", "Europe/London");
        entity.Update("AnotherUser", "Asia/Tokyo");

        Assert.Equal(initialDate, entity.CreatedAt);
        Assert.Equal(initialUser, entity.CreatedBy);
        Assert.Equal("AnotherUser", entity.UpdatedBy);
        Assert.Equal("Asia/Tokyo", entity.TimezoneId);
    }
}
