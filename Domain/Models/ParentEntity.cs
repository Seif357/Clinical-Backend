namespace Domain.Models;

public class ParentEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ulong RowVersion { get; set; }
    public bool IsDeleted { get; set; }
}