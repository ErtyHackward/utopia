using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;

namespace LostIsland.Client.States
{
    /// <summary>
    /// Blank for inventory state
    /// </summary>
    public class GameInventoryState : GamePlayState
    {
        public override string Name
        {
            get
            {
                return "Inventory";
            }
        }

        public GameInventoryState(IKernel iocContainer)
            : base(iocContainer)
        {
            
        }
    }
}
