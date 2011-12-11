using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BLToolkit.Data.Linq;

namespace UtopiaApi.Models.Repositories
{
    public class LoginRepository : Repository
    {
        public User Auth(string login, string passwordHash)
        {
            return Context.Users.Where(u => u.Login == login && u.PasswordHash == passwordHash && u.Confirmed == 1).First();
        }

        public void UpdateLoginDate(uint userId)
        {
            Context.Users.Where(u => u.id == userId).Set(u => u.LastLogin, DateTime.UtcNow).Update();
        }

        public void DeleteToken(string token)
        {
            //var t = new Token{ TokenValue = token };

            Context.Tokens.Delete(t => t.TokenValue == token);
        }

        public void WriteToken(uint userId, string token)
        {
            var t = new Token { UserId = userId, TokenValue = token, LastUpdate = DateTime.UtcNow };
            Context.InsertOrReplace(t);
        }

        public bool IsRegistered(string email)
        {
            return Context.Users.Any(u => u.Login.ToLower() == email.ToLower());
        }

        public void Register(string email, string password, string confirmToken)
        {
            var user = new User();

            var encoded = Encoding.UTF8.GetBytes(password);
            string passwordHash;

            using (var sha1 = SHA1.Create())
            {

                byte[] hash = sha1.ComputeHash(encoded, 0, encoded.Length);
                var formatted = new StringBuilder(hash.Length);
                foreach (byte b in hash)
                {
                    formatted.AppendFormat("{0:X2}", b);
                }
                passwordHash = formatted.ToString();
            }
            
            user.Login = email;
            user.RegisterDate = DateTime.UtcNow;
            user.PasswordHash = passwordHash;
            user.ConfirmToken = confirmToken;
            user.Culture = 1;

            Context.InsertWithIdentity(user);
        }

        public bool Confirm(string token)
        {
            return Context.Users.Where(u => u.ConfirmToken == token).Update(u => new User { Confirmed = 1 }) > 0; 
        }

        public bool IsTokenExists(string token)
        {
            return Context.Tokens.Any(t => t.TokenValue == token);
        }
    }
}