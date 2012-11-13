using System.ComponentModel.DataAnnotations;

namespace UtopiaApi.Library
{
    public class EmailAttribute : RegularExpressionAttribute
    {
        public EmailAttribute()
            : base("[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-z0-9])?\\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?")
        {
            ErrorMessage = "Enter a valid email address";
        }
    }
}