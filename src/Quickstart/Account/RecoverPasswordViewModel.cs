using System.ComponentModel.DataAnnotations;

namespace IdentityService.Quickstart.Account
{
    public class RecoverPasswordViewModel
    {
        [Required(ErrorMessage = "Pole Email jest wymagane")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Niepoprawny adres e-mail")]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}