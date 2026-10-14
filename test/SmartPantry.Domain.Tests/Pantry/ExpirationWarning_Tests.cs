using System;
using Shouldly;
using Xunit;

namespace SmartPantry.Pantry;

public class ExpirationWarning_Tests
{
    [Fact]
    public void Should_Create_Valid_ExpirationWarning()
    {
        // Arrange
        var id = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var expDate = new DateTime(2026, 10, 16, 0, 0, 0, DateTimeKind.Utc);
        var message = "El ítem vence el 2026-10-16.";

        // Act
        var warning = new ExpirationWarning(id, itemId, userId, ExpirationWarningType.ExpirationNear, expDate, message);

        // Assert
        warning.Id.ShouldBe(id);
        warning.PantryItemId.ShouldBe(itemId);
        warning.UserId.ShouldBe(userId);
        warning.WarningType.ShouldBe(ExpirationWarningType.ExpirationNear);
        warning.ExpirationDate.ShouldBe(expDate.Date);
        warning.Message.ShouldBe(message);
        warning.IsActive.ShouldBeTrue();
        warning.ResolvedTime.ShouldBeNull();
    }

    [Fact]
    public void Should_Deactivate_And_Reactivate_Warning()
    {
        // Arrange
        var warning = new ExpirationWarning(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            ExpirationWarningType.ExpirationNear,
            DateTime.UtcNow.Date,
            "Aviso inicial");

        // Act - Deactivate
        warning.Deactivate("Desactivada por consumo");

        // Assert
        warning.IsActive.ShouldBeFalse();
        warning.ResolvedTime.ShouldNotBeNull();
        warning.Message.ShouldBe("Desactivada por consumo");

        // Act - Reactivate
        var newExpDate = DateTime.UtcNow.AddDays(2).Date;
        warning.Activate(newExpDate, "Reactivada con nueva fecha");

        // Assert
        warning.IsActive.ShouldBeTrue();
        warning.ResolvedTime.ShouldBeNull();
        warning.ExpirationDate.ShouldBe(newExpDate);
        warning.Message.ShouldBe("Reactivada con nueva fecha");
    }
}
