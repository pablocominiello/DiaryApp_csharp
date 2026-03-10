using System.ComponentModel.DataAnnotations;

namespace DiaryApp.Models
{
    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva Contraseña")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;
    }
}