using System;
using System.Collections.Generic;
using BLToolkit.Data.Linq;
using System.Linq;

namespace UtopiaApi.Models.Repositories
{
    public class ServerRepository : Repository
    {
        public void ServerAlive(string name, string address)
        {
            var server = new Server { Name = name, Address = address, Culture = 1 };

            Context.InsertOrReplace(server);
        }

        public List<Server> GetServers(uint cultureId)
        {
            var list = Context.Servers.Where(s => s.Culture == cultureId && s.LastUpdate > (DateTime.Now.AddMinutes(-11))).ToList();
            return list;
        }
    }
}