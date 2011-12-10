using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UtopiaApi.Models
{
    public class RegisterModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
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