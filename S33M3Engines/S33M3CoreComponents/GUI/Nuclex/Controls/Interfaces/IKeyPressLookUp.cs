using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.GUI.Nuclex.Controls.Interfaces
{
    public interface IKeyPressLookUp
    {
        void ProcessPressKeyLookUp(System.Windows.Forms.Keys keyCode);
        string Text { get; set; }
    }
}
