using System.ComponentModel.DataAnnotations;

namespace IdentityService.Quickstart.Account
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Pole Hasło jest wymagane")]
        [DataType(DataType.Password)]
        [Display(Name = "Nowe hasło")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Pole Potwierdź hasło jest wymagane")]
        [DataType(DataType.Password, ErrorMessage = "Niepoprawny adres e-mail")]
        [Display(Name = "Potwierdź hasło")]
        [Compare("Password", ErrorMessage = "Potwierdzenie hasła nie zgadza się z hasłem")]

        public string ConfirmPassword { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
    }
}