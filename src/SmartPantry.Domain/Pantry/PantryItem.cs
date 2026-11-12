using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartPantry.Pantry;

public class PantryItem : AuditedAggregateRoot<Guid>
{
    public Guid UserId { get; private set; }

    public Guid ProductId { get; private set; }

    public PantryItemStatus Status { get; private set; }

    public DateTime? ExpirationDate { get; private set; }

    public decimal? Quantity { get; private set; }
   
    public string? Unit { get; private set; }

    protected PantryItem()
    {
        /* Constructor protegido para ORM (EF Core) */
    }

    public PantryItem(
        Guid id,
        Guid userId,
        Guid productId,
        DateTime? expirationDate = null,
        PantryItemStatus status = PantryItemStatus.Active)
        : base(id)
    {
        Check.NotNull(userId, nameof(userId));
        Check.NotNull(productId, nameof(productId));

        UserId = userId;
        ProductId = productId;
        ExpirationDate = expirationDate?.Date;
        Status = status;
    }

    public PantryItem SetExpirationDate(DateTime? expirationDate)
    {
        ExpirationDate = expirationDate?.Date;
        return this;
    }

    public PantryItem MarkAsConsumed()
    {
        Status = PantryItemStatus.Consumed;
        return this;
    }

    public PantryItem MarkAsActive()
    {
        Status = PantryItemStatus.Active;
        return this;
    }

    public PantryItem SetProduct(Guid productId)
    {
        Check.NotNull(productId, nameof(productId));
        ProductId = productId;
        return this;
    }

    public PantryItem SetQuantity(decimal? quantity)
    {
        if (quantity < 0)
        {
            throw new BusinessException("Pantry:QuantityCannotBeNegative");
        }
        Quantity = quantity;
        return this;
    }
    public PantryItem SetUnit(string? unit)
    {
        Unit = unit?.Trim();
        return this;
    }
}
