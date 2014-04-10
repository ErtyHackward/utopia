using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Server.Managers
{
    public class EntityGrowingManager
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private variable
        private ServerCore _server;
        #endregion

        #region Public Properties
        #endregion

        public EntityGrowingManager(ServerCore server)
        {
            _server = server;
        }

        #region Public Methods
        #endregion

        #region Private Methods
        #endregion

    }
}
