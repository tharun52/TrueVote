public class UpdateModeratorDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }

    public string? NewPassword { get; set; } 
    public string? PrevPassword { get; set; }
}