using EcoApi.Domain.Common;

namespace EcoApi.Domain.Entities;

public class AuthAuditLog : BaseEntity
{
    public int? UserId { get; set; }
    public string? Email { get; set; }
    public required string EventType { get; set; }
    public required string IpAddress { get; set; }
    public bool IsSuccess { get; set; }
    public string? FailureReason { get; set; }

    public User? User { get; set; }
}
