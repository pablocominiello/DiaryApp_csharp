using System.ComponentModel.DataAnnotations;

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
    }
}
