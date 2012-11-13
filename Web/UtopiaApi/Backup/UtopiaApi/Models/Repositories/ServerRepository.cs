using System;
using System.Collections.Generic;
using BLToolkit.Data.Linq;
using System.Linq;

namespace UtopiaApi.Models.Repositories
{
    public class ServerRepository : Repository
    {
        public void ServerAlive(string name, string address, uint users)
        {
            var result = from s in Context.Servers where s.Address == address select s;

            var server = result.FirstOrDefault();
            
            if (server == null)
            {
                server = new Server { Name = name, Address = address, Culture = 1, UsersCount = users, LastUpdate = DateTime.Now };
                Context.InsertWithIdentity(server);
            }
            else
            {
                server.Name = name;
                server.Address = address;
                server.UsersCount = users;
                server.LastUpdate = DateTime.Now;
                Context.Update(server);
            }
        }

        public List<Server> GetServers(uint cultureId)
        {
            var list = Context.Servers.Where(s => s.Culture == cultureId && s.LastUpdate > (DateTime.Now.AddMinutes(-11))).ToList();
            return list;
        }
    }
}