using System.ComponentModel.DataAnnotations;

namespace WebApiApp.Models;

public class OperationLog
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    public int EntityId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Operation { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? UserName { get; set; }

    [MaxLength(500)]
    public string? Payload { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
