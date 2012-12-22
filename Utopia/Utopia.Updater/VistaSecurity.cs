using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;
using System.Diagnostics;

public class VistaSecurity
{
    [DllImport("user32")]
    public static extern UInt32 SendMessage
        (IntPtr hWnd, UInt32 msg, UInt32 wParam, UInt32 lParam);

    internal const int BCM_FIRST = 0x1600; //Normal button

    internal const int BCM_SETSHIELD = (BCM_FIRST + 0x000C); //Elevated button

    static internal bool IsAdmin()
    {
        WindowsIdentity id = WindowsIdentity.GetCurrent();
        WindowsPrincipal p = new WindowsPrincipal(id);
        return p.IsInRole(WindowsBuiltInRole.Administrator);
    }

    internal static void AddShieldToButton(Button b)
    {
        b.FlatStyle = FlatStyle.System;
        SendMessage(b.Handle, BCM_SETSHIELD, 0, 0xFFFFFFFF);
    }

    internal static void RestartElevated(string args = null)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.UseShellExecute = true;
        startInfo.WorkingDirectory = Environment.CurrentDirectory;
        startInfo.FileName = Application.ExecutablePath;
        startInfo.Verb = "runas";
        startInfo.Arguments = args;
        try
        {
            Process p = Process.Start(startInfo);
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return;
        }

        Application.Exit();
    }

}