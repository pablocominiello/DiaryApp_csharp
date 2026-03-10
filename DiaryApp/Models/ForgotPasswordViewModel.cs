using System.ComponentModel.DataAnnotations;

namespace DiaryApp.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = string.Empty;
    }
}