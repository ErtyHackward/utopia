using S33M3CoreComponents.Inputs.Actions;

namespace Utopia.Action
{
    public class UtopiaActions : Actions
    {
        public const int Use_Left                       = 31;
        public const int Use_LeftWhileCursorLocked      = 32;
        public const int Use_LeftWhileCursorNotLocked   = 33;
        public const int Use_Right                      = 34;
        public const int Use_RightWhileCursorLocked     = 35;
        public const int Use_RightWhileCursorNotLocked  = 36;
        public const int ToolBar_SelectNext             = 37;
        public const int ToolBar_SelectPrevious         = 38;
        public const int Toggle_Chat                    = 39;
        public const int Exit_Chat                      = 40;
        public const int EntityUse                      = 41;
        public const int EntityThrow                    = 42;
        public const int OpenInventory                  = 43;
        public const int OpenMap                        = 44;
        public const int EndMove_Forward                = 45;
        public const int EndMove_Backward               = 46;
        public const int EndMove_StrafeLeft             = 47;
        public const int EndMove_StrafeRight            = 48;
        public const int RightDown                      = 49;
        public const int Toggle_Interface               = 50;
        public const int Drop_Mode                      = 51;
        public const int Open_Crafting                  = 52;
    }
}
