using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Inputs.Actions;

namespace Utopia.Action
{
    public class UtopiaActions : Actions
    {
        public const int Move_Forward                       =21;
        public const int Move_Backward                      =22;
        public const int Move_StrafeLeft                    =23;
        public const int Move_StrafeRight                   =24;
        public const int Move_Down                          =25;
        public const int Move_Up                            =26;
        public const int Move_Jump                          =27;
        public const int Move_Mode                          =28;
        public const int Move_Run                           =29;
        public const int Use_Left                           =30;
        public const int Use_LeftWhileCursorLocked          =31;
        public const int Use_LeftWhileCursorNotLocked       =32;
        public const int Use_Right                          =33;
        public const int Use_RightWhileCursorLocked         =34;
        public const int Use_RightWhileCursorNotLocked      =35;
        public const int ToolBar_SelectNext                 =36;
        public const int ToolBar_SelectPrevious             =37;
        public const int Toggle_Chat                        =38;
        public const int Exit_Chat                          =39;
        public const int EntityUse                          =40;
        public const int EntityThrow                        =41;
        public const int OpenInventory                      =42;
        public const int OpenMap                            =43;
        public const int EndMove_Forward                    =44;
        public const int EndMove_Backward                   =45;
        public const int EndMove_StrafeLeft                 =46;
        public const int EndMove_StrafeRight                =47;
    }
}
