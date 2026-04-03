using Domain.Common;

namespace UnitTests.Domain;

public sealed class DomainEntityTests
{
    private sealed class TestDomainEntity : DomainEntity
    {
        public TestDomainEntity() : base() { }
        public TestDomainEntity(string user, string timezoneId = "")
            : base(user, timezoneId) { }
    }

    [Fact(DisplayName = nameof(ConstructorWithValidParametersShouldCreateEntityWithProvidedValues))]
    public void ConstructorWithValidParametersShouldCreateEntityWithProvidedValues()
    {
        var user = "TestUser";
        var timezoneId = "America/New_York";

        var entity = new TestDomainEntity(user, timezoneId);

        Assert.Equal(user, entity.CreatedBy);
        Assert.Equal(timezoneId, entity.CreatedByTimezoneId);
        Assert.Equal(user, entity.UpdatedBy);
        Assert.Equal(timezoneId, entity.UpdatedByTimezoneId);
    }

    [Fact(DisplayName = nameof(ConstructorWithValidTimezoneIdShouldSetTimezoneId))]
    public void ConstructorWithValidTimezoneIdShouldSetTimezoneId()
    {
        var timezoneId = "America/Sao_Paulo";

        var entity = new TestDomainEntity("TestUser", timezoneId);

        Assert.Equal(timezoneId, entity.CreatedByTimezoneId);
        Assert.Equal(timezoneId, entity.UpdatedByTimezoneId);
    }

    [Fact(DisplayName = nameof(ConstructorWithInvalidTimezoneIdShouldDefaultToUtc))]
    public void ConstructorWithInvalidTimezoneIdShouldDefaultToUtc()
    {
        var invalidTimezoneId = "Invalid/Timezone";

        var result = Assert.Throws<TimeZoneNotFoundException>(() => new TestDomainEntity("TestUser", invalidTimezoneId));

        Assert.Contains("not found on the local computer.", result.Message);
    }

    [Fact(DisplayName = nameof(ConstructorWithNullTimezoneIdShouldDefaultToUtc))]
    public void ConstructorWithNullTimezoneIdShouldDefaultToUtc()
    {
        string? nullTimezone = null;

        var entity = new TestDomainEntity("TestUser", nullTimezone!);

        Assert.Equal(TimeZoneInfo.Utc.Id, entity.CreatedByTimezoneId);
        Assert.Equal(TimeZoneInfo.Utc.Id, entity.UpdatedByTimezoneId);
    }

    [Fact(DisplayName = nameof(ConstructorWithEmptyTimezoneIdShouldDefaultToUtc))]
    public void ConstructorWithEmptyTimezoneIdShouldDefaultToUtc()
    {
        var entity = new TestDomainEntity("TestUser", string.Empty);

        Assert.Equal(TimeZoneInfo.Utc.Id, entity.CreatedByTimezoneId);
        Assert.Equal(TimeZoneInfo.Utc.Id, entity.UpdatedByTimezoneId);
    }

    [Fact(DisplayName = nameof(ConstructorWithWhitespaceTimezoneIdShouldDefaultToUtc))]
    public void ConstructorWithWhitespaceTimezoneIdShouldDefaultToUtc()
    {
        var entity = new TestDomainEntity("TestUser", "   ");

        Assert.Equal(TimeZoneInfo.Utc.Id, entity.CreatedByTimezoneId);
        Assert.Equal(TimeZoneInfo.Utc.Id, entity.UpdatedByTimezoneId);
    }

    [Fact(DisplayName = nameof(UpdateWithValidParametersShouldUpdateEntityProperties))]
    public void UpdateWithValidParametersShouldUpdateEntityProperties()
    {
        var entity = new TestDomainEntity("InitialUser", "America/New_York");
        var beforeUpdate = DateTime.UtcNow;

        entity.Update("UpdatedUser", "Europe/London");

        var afterUpdate = DateTime.UtcNow;
        Assert.True(entity.UpdatedAt >= beforeUpdate && entity.UpdatedAt <= afterUpdate);
        Assert.Equal("UpdatedUser", entity.UpdatedBy);
        Assert.Equal("Europe/London", entity.UpdatedByTimezoneId);
        Assert.Equal("InitialUser", entity.CreatedBy);
    }

    [Fact(DisplayName = nameof(UpdateWithNullUserShouldDefaultToSystem))]
    public void UpdateWithNullUserShouldDefaultToSystem()
    {
        var entity = new TestDomainEntity("InitialUser");

        entity.Update(null);

        Assert.Equal("System", entity.UpdatedBy);
    }

    [Fact(DisplayName = nameof(UpdateWithInvalidTimezoneIdShouldDefaultToUtc))]
    public void UpdateWithInvalidTimezoneIdShouldDefaultToUtc()
    {
        var entity = new TestDomainEntity("TestUser", "America/New_York");

        var result = Assert.Throws<TimeZoneNotFoundException>(() => entity.Update("UpdatedUser", "Invalid/Timezone"));

        Assert.Contains("not found on the local computer.", result.Message);
    }

    [Fact(DisplayName = nameof(UpdateWithEmptyTimezoneIdShouldDefaultToUtc))]
    public void UpdateWithEmptyTimezoneIdShouldDefaultToUtc()
    {
        var entity = new TestDomainEntity("TestUser", "America/New_York");

        entity.Update("UpdatedUser", string.Empty);

        Assert.Equal("America/New_York", entity.CreatedByTimezoneId);
        Assert.Equal(TimeZoneInfo.Utc.Id, entity.UpdatedByTimezoneId);
    }

    [Fact(DisplayName = nameof(UpdateWithoutParametersShouldUseDefaultValues))]
    public void UpdateWithoutParametersShouldUseDefaultValues()
    {
        var entity = new TestDomainEntity("InitialUser", "America/New_York");
        var beforeUpdate = DateTime.UtcNow;

        entity.Update();

        var afterUpdate = DateTime.UtcNow;
        Assert.True(entity.UpdatedAt >= beforeUpdate && entity.UpdatedAt <= afterUpdate);
        Assert.Equal("System", entity.UpdatedBy);
        Assert.Equal(TimeZoneInfo.Utc.Id, entity.UpdatedByTimezoneId);
    }

    [Fact(DisplayName = nameof(ConstructorWithUtcTimezoneIdShouldSetUtcTimezone))]
    public void ConstructorWithUtcTimezoneIdShouldSetUtcTimezone()
    {
        var entity = new TestDomainEntity("TestUser", TimeZoneInfo.Utc.Id);

        Assert.Equal(TimeZoneInfo.Utc.Id, entity.CreatedByTimezoneId);
        Assert.Equal(TimeZoneInfo.Utc.Id, entity.UpdatedByTimezoneId);
    }

    [Fact(DisplayName = nameof(UpdatePreservesCreatedAtAndCreatedByWhenUpdating))]
    public void UpdatePreservesCreatedAtAndCreatedByWhenUpdating()
    {
        var initialUser = "InitialUser";
        var entity = new TestDomainEntity(initialUser, "America/New_York");

        entity.Update("UpdatedUser", "Europe/London");
        entity.Update("AnotherUser", "Asia/Tokyo");

        Assert.Equal(initialUser, entity.CreatedBy);
        Assert.Equal("AnotherUser", entity.UpdatedBy);
        Assert.Equal("America/New_York", entity.CreatedByTimezoneId);
        Assert.Equal("Asia/Tokyo", entity.UpdatedByTimezoneId);
    }
}
