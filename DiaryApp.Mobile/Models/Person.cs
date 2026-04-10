using System.ComponentModel.DataAnnotations;

namespace DiaryApp.Mobile.Models;

public class Person
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ingrese el Nombre!")]
    public string Nombre { get; set; } = string.Empty;

    // ✅ Campo OPCIONAL - Removido [Required]
    [StringLength(250, ErrorMessage = "La descripción no puede exceder 250 caracteres")]
    public string? Content { get; set; }

    public DateTime Born { get; set; }

    public string? ImagenUrl { get; set; }

    // ✅ NUEVO: Campo para identificar administradores
    public bool Admin { get; set; } = false;

    public ICollection<Payment>? Payments { get; set; }
}