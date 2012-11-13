using System.ComponentModel.DataAnnotations;

namespace UtopiaApi.Library
{
    public class PasswordAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
                return false;
            
            var passString = value.ToString();

            if (string.IsNullOrWhiteSpace(passString))
                return false;

            if (passString.Length < 5)
            {
                ErrorMessage = "Use at least 5 symbols in a password";
                return false;
            }

            if (passString.Length > 30)
            {
                ErrorMessage = "Maximum length of a password is 30 symbols";
                return false;
            }

            return true;
        }
    }
}