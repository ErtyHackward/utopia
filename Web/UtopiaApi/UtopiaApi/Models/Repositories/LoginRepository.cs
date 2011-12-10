using System;
using System.Linq;
using BLToolkit.Data.Linq;

namespace UtopiaApi.Models.Repositories
{
    public class LoginRepository : Repository
    {
        public User Auth(string login, string passwordHash)
        {
            return Context.Users.Where(u => u.Login == login && u.PasswordHash == passwordHash).First();
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

    }
}