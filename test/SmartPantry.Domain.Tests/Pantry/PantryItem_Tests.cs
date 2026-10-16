using System;
using Shouldly;
using Xunit;

namespace SmartPantry.Pantry;

public class PantryItem_Tests
{
    [Fact]
    public void Should_Create_Valid_PantryItem()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var expDate = new DateTime(2026, 10, 20, 15, 30, 0, DateTimeKind.Utc);

        // Act
        var item = new PantryItem(id, userId, productId, expDate);

        // Assert
        item.Id.ShouldBe(id);
        item.UserId.ShouldBe(userId);
        item.ProductId.ShouldBe(productId);
        item.ExpirationDate.ShouldBe(expDate.Date);
        item.Status.ShouldBe(PantryItemStatus.Active);
    }

    [Fact]
    public void Should_Change_Expiration_Date()
    {
        // Arrange
        var item = new PantryItem(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        var newDate = new DateTime(2026, 10, 25, 0, 0, 0, DateTimeKind.Utc);

        // Act
        item.SetExpirationDate(newDate);

        // Assert
        item.ExpirationDate.ShouldBe(newDate.Date);

        // Act - remove date
        item.SetExpirationDate(null);
        item.ExpirationDate.ShouldBeNull();
    }

    [Fact]
    public void Should_Change_Status_To_Consumed_And_Back_To_Active()
    {
        // Arrange
        var item = new PantryItem(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        item.Status.ShouldBe(PantryItemStatus.Active);

        // Act
        item.MarkAsConsumed();
        item.Status.ShouldBe(PantryItemStatus.Consumed);

        item.MarkAsActive();
        item.Status.ShouldBe(PantryItemStatus.Active);
    }
}
