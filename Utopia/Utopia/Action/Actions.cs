using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Action
{
    public enum Actions
    {
        Move_Forward,
        Move_Backward,
        Move_StrafeLeft,
        Move_StrafeRight,
        Move_Down,
        Move_Up,
        Move_Jump,
        Move_Mode,
        Move_Run,
        Engine_FullScreen,
        Engine_LockMouseCursor,
        Engine_VSync,
        Engine_ShowDebugUI,
        Engine_Exit,
        Use_Left,
        Use_Right,
        Block_SelectNext,
        Block_SelectPrevious,
        World_FreezeTime,
        DebugUI_Insert
    }
}
