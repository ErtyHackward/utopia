﻿using System;
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
        Engine_TogglePerfMonitor,
        Engine_ToggleDebugInfo,
        Use_Left,
        Use_LeftWhileCursorLocked,
        Use_LeftWhileCursorNotLocked,
        Use_Right,
        Use_RightWhileCursorLocked,
        Use_RightWhileCursorNotLocked,
        ToolBar_SelectNext,
        ToolBar_SelectPrevious,
        World_FreezeTime,
        DebugUI_Insert,
        Toggle_Chat,//XXX this syntax is not standard, its' camelCase with an underscore, resharper doesn't even support it !
        EntityUse,
        EntityThrow,
        OpenInventory,
        OpenMap
    }
}
