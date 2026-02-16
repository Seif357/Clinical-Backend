namespace Domain.Models.AI;

public class ModelOutput : ParentEntity
{
    public int ModelInputId { get; set; }
    public string Classification { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public DateTime ProcessedAt { get; set; }
    
    public ModelInput Input { get; set; } = null!;
}