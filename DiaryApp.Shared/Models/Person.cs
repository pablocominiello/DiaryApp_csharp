using System.ComponentModel.DataAnnotations;

namespace DiaryApp.Shared.Models;

public class Person
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
    [Display(Name = "Nombre Completo")]
    public string Nombre { get; set; } = string.Empty;
    
    [StringLength(250, ErrorMessage = "La descripción no puede exceder 250 caracteres")]
    [Display(Name = "Descripción / Observaciones")]
    public string Content { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La fecha de nacimiento es requerida")]
    [Display(Name = "Fecha de Nacimiento")]
    [DataType(DataType.Date)]
    public DateTime Born { get; set; } = DateTime.Now.AddYears(-18);
    
    [Display(Name = "Foto de Perfil")]
    public string? ImagenUrl { get; set; }
    
    // ✅ Relación 1:1 con IdentityUser (obligatoria)
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public ICollection<Payment>? Payments { get; set; }
}