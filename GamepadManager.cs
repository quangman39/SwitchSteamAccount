using System.Runtime.InteropServices;
namespace SteamSwitcherApp;

public class GamepadManager
{
    [DllImport("xinput1_4.dll")]
    private static extern uint XInputGetState(
        uint dwUserIndex,
        out XINPUT_STATE state
    );
    [StructLayout(LayoutKind.Sequential)]
    public struct XINPUT_STATE
    {
        public uint PacketNumber;
        public XINPUT_GAMEPAD Gamepad;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct XINPUT_GAMEPAD
    {
        public ushort Buttons;
        public byte LeftTrigger;
        public byte RightTrigger;
        public short ThumbLX;
        public short ThumbLY;
        public short ThumbRX;
        public short ThumbRY;
    }
    
    public const ushort DPadUp = 0x0001;
    public const ushort DPadDown = 0x0002;
    public const ushort DPadLeft = 0x0004;
    public const ushort DPadRight = 0x0008;
    public const ushort A = 0x1000;
    public const ushort B = 0x2000;
    public const ushort X = 0x4000;
    
    public static ushort GetButtons()
    {
        uint result = XInputGetState(0, out XINPUT_STATE state);

        if (result != 0)
            return 0;

        return state.Gamepad.Buttons;
    }
}
