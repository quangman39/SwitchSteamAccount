using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SteamSwitcherApp;

public static class SteamManager
{
    private const string SteamPath =
        @"C:\Program Files (x86)\Steam\steam.exe";


    // =====================================================
    // WINDOWS API
    // =====================================================

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);


    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(
        uint nInputs,
        INPUT[] pInputs,
        int cbSize
    );


    // =====================================================
    // CONSTANTS
    // =====================================================

    private const uint INPUT_MOUSE = 0;
    private const uint INPUT_KEYBOARD = 1;
    private const uint INPUT_HARDWARE = 2;

    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const uint KEYEVENTF_UNICODE = 0x0004;

    private const ushort VK_TAB = 0x09;
    private const ushort VK_RETURN = 0x0D;


    // =====================================================
    // WINDOWS INPUT STRUCTS
    // =====================================================

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public InputUnion U;
    }


    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public MOUSEINPUT mi;

        [FieldOffset(0)]
        public KEYBDINPUT ki;

        [FieldOffset(0)]
        public HARDWAREINPUT hi;
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct HARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }


    // =====================================================
    // LOGIN
    // =====================================================

    public static async Task OpenSteam(Action<string>? reportStatus = null)
    {
        // Đóng Steam hiện tại
        reportStatus?.Invoke("Closing Steam...");
        await CloseSteam();


        // Mở Steam
        reportStatus?.Invoke("Opening Steam...");
        Process.Start(new ProcessStartInfo
        {
            FileName = SteamPath,
            UseShellExecute = true
        });
    }


    public static async Task FillLogin(
        Account account,
        Action<string>? reportStatus = null)
    {
        if (string.IsNullOrWhiteSpace(account.Name))
        {
            throw new ArgumentException("Username không được để trống.");
        }

        if (account.Password == null)
        {
            throw new ArgumentException("Password không được để trống.");
        }


        // Chờ cửa sổ Login xuất hiện
        reportStatus?.Invoke("Waiting for Steam Login window...");
        Process? loginWindow = await FindLoginWindowAsync(reportStatus);

        if (loginWindow == null)
        {
            throw new Exception(
                "Không tìm thấy cửa sổ Steam Login."
            );
        }


        // Đưa Steam Login lên foreground
        reportStatus?.Invoke("Focusing Steam Login window...");
        bool focused =
            SetForegroundWindow(loginWindow.MainWindowHandle);

        if (!focused)
        {
            throw new Exception(
                "Không thể focus cửa sổ Steam Login."
            );
        }


        await Task.Delay(500);
        reportStatus?.Invoke("Waiting for login fields...");

        await Task.Delay(1500);


        // Username
        reportStatus?.Invoke("Typing username...");
        SendUnicodeText(account.Name);

        await Task.Delay(150);


        // Chuyển sang ô password
        reportStatus?.Invoke("Moving to password field...");
        SendKey(VK_TAB);

        await Task.Delay(150);


        // Password
        reportStatus?.Invoke("Typing password...");
        SendUnicodeText(account.Password);

        await Task.Delay(150);


        // Enter để Login
        reportStatus?.Invoke("Submitting login...");
        SendKey(VK_RETURN);
    }


    // =====================================================
    // SEND TEXT
    // =====================================================

    private static void SendUnicodeText(string text)
    {
        foreach (char character in text)
        {
            INPUT[] inputs =
            {
                // Key Down
                new INPUT
                {
                    type = INPUT_KEYBOARD,

                    U = new InputUnion
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = 0,
                            wScan = character,
                            dwFlags = KEYEVENTF_UNICODE,
                            time = 0,
                            dwExtraInfo = UIntPtr.Zero
                        }
                    }
                },


                // Key Up
                new INPUT
                {
                    type = INPUT_KEYBOARD,

                    U = new InputUnion
                    {
                        ki = new KEYBDINPUT
                        {
                            wVk = 0,
                            wScan = character,
                            dwFlags =
                                KEYEVENTF_UNICODE |
                                KEYEVENTF_KEYUP,

                            time = 0,
                            dwExtraInfo = UIntPtr.Zero
                        }
                    }
                }
            };


            SendInputs(inputs);
        }
    }


    // =====================================================
    // SEND SPECIAL KEY
    // =====================================================

    private static void SendKey(ushort key)
    {
        INPUT[] inputs =
        {
            // Key Down
            new INPUT
            {
                type = INPUT_KEYBOARD,

                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = key,
                        wScan = 0,
                        dwFlags = 0,
                        time = 0,
                        dwExtraInfo = UIntPtr.Zero
                    }
                }
            },


            // Key Up
            new INPUT
            {
                type = INPUT_KEYBOARD,

                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = key,
                        wScan = 0,
                        dwFlags = KEYEVENTF_KEYUP,
                        time = 0,
                        dwExtraInfo = UIntPtr.Zero
                    }
                }
            }
        };


        SendInputs(inputs);
    }


    // =====================================================
    // CALL SENDINPUT
    // =====================================================

    private static void SendInputs(INPUT[] inputs)
    {
        uint sent = SendInput(
            (uint)inputs.Length,
            inputs,
            Marshal.SizeOf<INPUT>()
        );


        if (sent != inputs.Length)
        {
            throw new Win32Exception(
                Marshal.GetLastWin32Error(),
                "SendInput failed."
            );
        }
    }


    // =====================================================
    // FIND STEAM LOGIN WINDOW
    // =====================================================

    private static async Task<Process?> FindLoginWindowAsync(
        Action<string>? reportStatus = null)
    {
        string? lastWindowTitle = null;

        // Thử trong tối đa khoảng 30 giây
        for (int i = 0; i < 60; i++)
        {
            Process[] processes =
                Process.GetProcessesByName(
                    "steamwebhelper"
                );


            foreach (Process process in processes)
            {
                process.Refresh();

                string title = process.MainWindowTitle;

                if (
                    !string.IsNullOrWhiteSpace(title) &&
                    title != lastWindowTitle
                )
                {
                    reportStatus?.Invoke(
                        "Found Steam window: " + title
                    );
                    lastWindowTitle = title;
                }

                if (
                    process.MainWindowHandle != IntPtr.Zero &&
                    IsSteamLoginWindowTitle(title)
                )
                {
                    return process;
                }
            }


            await Task.Delay(500);
        }


        return null;
    }


    private static bool IsSteamLoginWindowTitle(string title)
    {
        if (
            !title.Contains(
                "Steam",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            return false;
        }

        return
            title.Contains(
                "Sign in",
                StringComparison.OrdinalIgnoreCase
            ) ||
            title.Contains(
                "Login",
                StringComparison.OrdinalIgnoreCase
            ) ||
            title.Contains(
                "Log in",
                StringComparison.OrdinalIgnoreCase
            ) ||
            title.Contains(
                "Đăng nhập",
                StringComparison.OrdinalIgnoreCase
            );
    }


    // =====================================================
    // CLOSE STEAM
    // =====================================================

    private static async Task CloseSteam()
    {
        Process[] processes =
            Process.GetProcessesByName("steam");


        foreach (Process process in processes)
        {
            if (process.HasExited)
            {
                continue;
            }

            bool closeRequested = process.CloseMainWindow();

            if (
                closeRequested &&
                await WaitForExitAsync(process, 5000)
            )
            {
                continue;
            }

            if (!process.HasExited)
            {
                process.Kill();
            }

            await process.WaitForExitAsync();
        }
    }


    private static async Task<bool> WaitForExitAsync(
        Process process,
        int millisecondsDelay)
    {
        Task waitForExit = process.WaitForExitAsync();
        Task timeout = Task.Delay(millisecondsDelay);

        return await Task.WhenAny(waitForExit, timeout) == waitForExit;
    }
}
