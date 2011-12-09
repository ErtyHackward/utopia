using System.Linq;

namespace UtopiaApi.Models.Repositories
{
    public class LoginRepository : Repository
    {
        public bool Auth(string login, string passwordHash)
        {
            return Context.Users.Where(u => u.Login == login && u.PasswordHash == passwordHash).Any();
        }

    }
}