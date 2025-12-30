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
}
