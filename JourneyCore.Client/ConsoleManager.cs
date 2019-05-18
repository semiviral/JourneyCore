using System;
using System.Runtime.InteropServices;

namespace JourneyCore.Client
{
    public class ConsoleManager
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        public void Hide(bool hide)
        {
            ShowWindow(GetConsoleWindow(), hide ? SW_HIDE : SW_SHOW);
        }
    }
}
