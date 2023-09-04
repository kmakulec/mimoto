// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.ComponentModel.DataAnnotations;

namespace IdentityService.Quickstart.UI
{
    public class LoginInputModel
    {
        [Required(ErrorMessage = "Pole Email jest wymagane")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Niepoprawny adres e-mail")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Pole Hasło jest wymagane")]
        [Display(Name = "Hasło")]
        public string Password { get; set; }

        public bool RememberLogin { get; set; }
        public string ReturnUrl { get; set; }
    }
}