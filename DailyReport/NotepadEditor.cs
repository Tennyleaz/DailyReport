﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace DailyReport
{
    class NotepadEditor
    {
        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Unicode)]
        static extern IntPtr SendMessage(IntPtr windowHandle, int message, IntPtr wParam, string text);

        const int WM_SETTEXT = 0x000C;

        public static void PipeTextToNotepad(string textIn)
        {
            // Fire up a brand new Notepad.
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = @"notepad.exe";
            process.Start();
            process.WaitForInputIdle();

            // Find the Notepad edit control.
            var edit = AutomationElement.FromHandle(process.MainWindowHandle)
                .FindFirst(TreeScope.Subtree,
                           new PropertyCondition(
                               AutomationElement.ControlTypeProperty,
                               ControlType.Document));

            // Shove the text into that window.
            var nativeHandle = new IntPtr((int)edit.GetCurrentPropertyValue(
                              AutomationElement.NativeWindowHandleProperty));
            SendMessage(nativeHandle, WM_SETTEXT, IntPtr.Zero, textIn);
        }
    }
}
