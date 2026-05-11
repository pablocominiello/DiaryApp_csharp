using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiaryApp.Mobile.Models;

public class Payment
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Seleccione una persona")]
    [Display(Name = "Persona")]
    public int PeoplesId { get; set; }

    [Required(ErrorMessage = "Ingrese el año")]
    [Range(2000, 2100, ErrorMessage = "Ingrese un año válido entre 2000 y 2100")]
    [Display(Name = "Año")]
    public int Ano { get; set; }

    [Required(ErrorMessage = "Ingrese el mes")]
    [Range(1, 12, ErrorMessage = "Ingrese un mes válido entre 1 y 12")]
    [Display(Name = "Mes")]
    public int Mes { get; set; }

    [Required(ErrorMessage = "Ingrese la fecha")]
    [Display(Name = "Fecha")]
    public DateTime Fecha { get; set; }

    [Display(Name = "Comprobante")]
    public string? ComprobanteUrl { get; set; }

    [StringLength(255, ErrorMessage = "El comentario no puede exceder 255 caracteres")]
    [Display(Name = "Comentario")]
    public string? Comentary { get; set; }

    [Required(ErrorMessage = "Ingrese el monto")]
    [Range(0.01, 999999.99, ErrorMessage = "El monto debe ser mayor a 0")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Monto")]
    [DataType(DataType.Currency)]
    public decimal Amount { get; set; }

    [ForeignKey("PeoplesId")]
    public Person? Person { get; set; }
}