#region LGPL License
/*************************************************************************
    Crazy Eddie's GUI System (http://crayzedsgui.sourceforge.net)
    Copyright (C)2004 Paul D Turner (crayzed@users.sourceforge.net)

    C# Port developed by Chris McGuirk (leedgitar@latenitegames.com)
    Compatible with the Axiom 3D Engine (http://axiomengine.sf.net)

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*************************************************************************/
#endregion LGPL License

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CeGui.Demo.DirectX {

  /// <summary>CeGui demonstration application</summary>
  static class Program {

    /// <summary>The main entry point for the application</summary>
    [STAThread]
    static void Main() {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      // Create the CeGui demonstration form which will host the actual rendered GUI
      CeGuiDemoForm demoForm = new CeGuiDemoForm();
      demoForm.Show();

      // The typical WinForms stuff. Keep the application running and perform the
      // message pump until the main window is closed. We keep refreshing (= redrawing)
      // the window so it will be continuously updated.
      while(demoForm.Created & !demoForm.IsClosing) {
        demoForm.Refresh();
        Application.DoEvents();
      }

      demoForm.Close();
    }

  }

} // namespace CeGui.Demo.DirectX

