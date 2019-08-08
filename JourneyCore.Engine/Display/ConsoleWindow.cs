using System;
using System.Runtime.InteropServices;

namespace JourneyCore.Lib.Display
{
    public class ConsoleManager
    {
        private const int _SW_HIDE = 0;
        private const int _SW_SHOW = 5;

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public void Hide(bool hide)
        {
            ShowWindow(GetConsoleWindow(), hide ? _SW_HIDE : _SW_SHOW);
        }
    }
}