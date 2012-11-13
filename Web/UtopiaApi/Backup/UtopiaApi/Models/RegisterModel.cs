using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using UtopiaApi.Library;

namespace UtopiaApi.Models
{
    public class RegisterModel
    {
        [Required]
        [Email]
        public string Email { get; set; }

        [Required]
        public string DisplayName { get; set; }

        [Required]
        [Password]
        public string Password { get; set; }

        [Required]
        [Password]
        public string RepeatPassword { get; set; }

        public Language Language { get; set; }
    }

    public enum Language
    {
        English,
        French,
        Russian
    }
}