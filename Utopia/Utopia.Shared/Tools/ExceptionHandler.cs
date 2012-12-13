﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Utopia.Shared.Tools
{
    public class ExceptionHandler
    {
        public static void SaveCrashReportsToFile()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var bugReportPath = Path.GetTempFileName();
            var exception = (Exception)e.ExceptionObject;
            File.WriteAllText(bugReportPath, string.Format("{0}\r\n{1}", exception.Message, exception.StackTrace));

            Process.Start("notepad", bugReportPath);

        }
    }
}