using BluecoreApi.Enums;
namespace Bluecore.Models;

public class CreditRequest
{
    public int Id { get; set; }
    public string ApplicantId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int TermMonths { get; set; }
    public CreditStatus Status { get; set; } = CreditStatus.Pending;
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}