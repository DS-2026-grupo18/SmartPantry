using System;

namespace SmartPantry.Pantry;

public class ExpirationWarningProcessResult
{
    public DateTime ReferenceDateUtc { get; set; }

    public int TotalProcessedItems { get; set; }

    public int CreatedWarnings { get; set; }

    public int UpdatedWarnings { get; set; }

    public int DeactivatedWarnings { get; set; }

    public int UnchangedWarnings { get; set; }
}
