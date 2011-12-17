using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LostIsland.Shared.Tools;
using Ninject;
using Utopia.Editor;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;

namespace LostIslandHD.Client
{
    /// <summary>
    /// 
    /// This class 's responsability is to provide client side specific tool implementation
    /// for tools that implement IGameStateTool : switch some UI, disable some input, any client related things ...
    /// 
    /// </summary>
    public class GameStateToolManager : IGameStateToolManager
    {
        /// <summary>
        /// Cheating the IOC for runtime component switching having circular dependencies
        /// Don't do this at home kids :p 
        /// </summary>
        private readonly IKernel _iocContainer;
        
        public GameStateToolManager(IKernel iocContainer)
        {
            _iocContainer = iocContainer;            
        }

        public void Use(IGameStateTool tool)
        {

            //TODO refactor EntityEditor , split the carving(simple functionality, simple ui) from the editing (complex ui, more feature)
            if (tool is Carver )
            {
                //this can be slow if you abuse it, here its a user special mode activation so it's ok
                //CarvingEditor editor = _iocContainer.Get<CarvingEditor>();
                //editor.Enabled = true;
                //editor.StartCarving();

            }
            else if (tool is Editor)
            {
                //_iocContainer.Get<EntityEditor>().Enabled = true;
            }

        }

    }
}
