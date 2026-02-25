using System.ComponentModel.DataAnnotations;

namespace DiaryApp.Core.Models
{
    public class DiaryEntry
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ingrese el Titulo!")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Ingrese Contenido entre 3 y 10 caracteres")]
        public string Content { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; }
    }
}