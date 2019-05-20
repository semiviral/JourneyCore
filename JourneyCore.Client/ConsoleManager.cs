using System;
using System.Runtime.InteropServices;

namespace JourneyCore.Client
{
    public class ConsoleManager
    {
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public void Hide(bool hide)
        {
            ShowWindow(GetConsoleWindow(), hide ? SW_HIDE : SW_SHOW);
        }
    }
}