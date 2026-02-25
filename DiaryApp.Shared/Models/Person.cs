namespace DiaryApp.Shared.Models;

public class Person
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Born { get; set; }
    public string? ImagenUrl { get; set; }
    public ICollection<Payment>? Payments { get; set; }
}