using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiaryApp.Models
{
    public class DiaryEntry
    {
        //[Key]
        public int Id { get; set; }

        [Required(ErrorMessage ="Ingrese el Titulo!"    )]
        //[StringLength(100, MinimumLength = 3,ErrorMessage ="Ingrese entre 3 y 10 caracteres")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Ingrese Contenido entre 3 y 10 caracteres")]
        public string Content { get; set; }
        
        public DateTime DateCreated { get; set; }

    }
    public class Person
    { 
        //[Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ingrese el Nombre!")] 
        //[StringLength(100, MinimumLength = 3,ErrorMessage ="Ingrese entre 3 y 10 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(250, MinimumLength = 3, ErrorMessage = "Ingrese Contenido entre 3 y 250 caracteres")]
        public string Content { get; set; }

        public DateTime Born { get; set; }

        // Nueva propiedad para almacenar la ruta de la imagen
        public string? ImagenUrl { get; set; }

        // Relación uno a muchos con Payments
        public ICollection<Payment>? Payments { get; set; }
    }

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

        // Propiedad de navegación
        [ForeignKey("PeoplesId")]
        public Person? Person { get; set; }
    }
}
