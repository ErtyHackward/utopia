using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UtopiaApi.Models
{
    [Serializable]
    public class LoginResponce
    {
        public bool Logged { get; set; }
        public string Token { get; set; }
        public string Error { get; set; }
        public string Notice { get; set; }
    }
}