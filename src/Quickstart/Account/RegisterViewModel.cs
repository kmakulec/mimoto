// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.ComponentModel.DataAnnotations;

namespace IdentityService.Quickstart.UI
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Pole Email jest wymagane")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Niepoprawny adres e-mail")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Pole Hasło jest wymagane")]
        [DataType(DataType.Password)]
        [Display(Name = "Hasło")]
        public string Password { get; set; }
        
        [Required(ErrorMessage = "Pole Powtórz hasło jest wymagane")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Potwierdzenie hasła nie zgadza się z hasłem")]
        [Display(Name = "Powtórz hasło")]
        public string ConfirmPassword { get; set; }
        
        public string ReturnUrl { get; set; }
    }
}