using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SmartPantry.Pantry;

public class ExpirationWarning : AuditedAggregateRoot<Guid>
{
    public Guid PantryItemId { get; private set; }

    public Guid UserId { get; private set; }

    public ExpirationWarningType WarningType { get; private set; }

    public DateTime? ExpirationDate { get; private set; }

    public bool IsActive { get; private set; }

    public string Message { get; private set; } = null!;

    public DateTime? ResolvedTime { get; private set; }

    protected ExpirationWarning()
    {
        /* Constructor protegido para ORM (EF Core) */
    }

    public ExpirationWarning(
        Guid id,
        Guid pantryItemId,
        Guid userId,
        ExpirationWarningType warningType,
        DateTime? expirationDate,
        string message)
        : base(id)
    {
        Check.NotNull(pantryItemId, nameof(pantryItemId));
        Check.NotNull(userId, nameof(userId));
        Check.NotNullOrWhiteSpace(message, nameof(message), maxLength: PantryConsts.MaxWarningMessageLength);

        PantryItemId = pantryItemId;
        UserId = userId;
        WarningType = warningType;
        ExpirationDate = expirationDate?.Date;
        Message = message.Trim();
        IsActive = true;
        ResolvedTime = null;
    }

    public void Activate(DateTime? expirationDate, string message)
    {
        Check.NotNullOrWhiteSpace(message, nameof(message), maxLength: PantryConsts.MaxWarningMessageLength);

        ExpirationDate = expirationDate?.Date;
        Message = message.Trim();
        IsActive = true;
        ResolvedTime = null;
    }

    public void Deactivate(string? message = null)
    {
        IsActive = false;
        ResolvedTime = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(message))
        {
            Message = message.Trim();
        }
    }

    public void UpdateExpirationDate(DateTime? expirationDate, string message)
    {
        Check.NotNullOrWhiteSpace(message, nameof(message), maxLength: PantryConsts.MaxWarningMessageLength);

        ExpirationDate = expirationDate?.Date;
        Message = message.Trim();
        IsActive = true;
    }
}
