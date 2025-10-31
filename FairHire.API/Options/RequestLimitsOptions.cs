namespace FairHire.API.Options;

/// <summary>
/// Параметри лімітів для тіла запиту. 
/// Інжектимо один екземпляр через DI (Options не обов'язково).
/// </summary>

public sealed class RequestLimitsOptions
{
    /// <summary>
    /// Ліміт для звичайних API-запитів (байти). Напр., 2 МБ.
    /// </summary>
    public long ApiMaxBodyBytes { get; set; } = 2 * 1024 * 1024;
}
