using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.SQLite;

namespace Utopia.Settings
{
    /// <summary>
    /// Class that wil lbe used to manage game settings DB load, update, insert
    /// </summary>
    public class SQLiteSettingsManager : SQLiteManager
    {
        public SQLiteSettingsManager():
            base("","",false)
        {

        }

        protected override void CreateDataBase(System.Data.SQLite.SQLiteConnection conn)
        {
            throw new NotImplementedException();
        }
    }
}
