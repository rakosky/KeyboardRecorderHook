using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace KeyboardHookForm
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    class InterceptKeys
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        [STAThread]

        public static void Main()
        {        
            // Delete the log file on startup
            if (File.Exists("keystrokes.txt"))
            {
                File.Delete("keystrokes.txt");
            }
            _hookID = SetHook(_proc);
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            //if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                string keyboardEvent = "";
                switch(wParam)
                {
                    case 256: case 260: keyboardEvent = "DOWN"; break;
                    case 257: case 261: keyboardEvent = "UP"; break;
                }

                int vkCode = Marshal.ReadInt32(lParam);

                long elapsed = TimeGetTime() - _timeHolder;
                _timeHolder = TimeGetTime();
                if (elapsed > 0)
                {
                    string delaytext = $"Sleep({elapsed});\n";
                    File.AppendAllText("keystrokes.txt", delaytext);
                }

                //                SendKey(ScanCodeShort.KEY_0, KeyPressType.DOWN);
                //Sleep(80);
                var scanCode = Structs.VirtualKeyToScanCode((Structs.VirtualKeyShort)vkCode);
                string keytext = $"SendKey(ScanCodeShort.{scanCode}, KeyPressType.{keyboardEvent}); \n";
                File.AppendAllText("keystrokes.txt", keytext);
                Console.WriteLine((Keys)vkCode);
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("winmm.dll", EntryPoint = "timeGetTime")]
        private static extern uint TimeGetTime();
        private static uint _timeHolder = TimeGetTime();


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}