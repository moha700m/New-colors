using System.Runtime.InteropServices;

namespace MohammedLab.ColorVision.Core;

public static class XInput
{
    [Flags]
    public enum Buttons : ushort
    {
        DPadUp = 0x0001, DPadDown = 0x0002, DPadLeft = 0x0004, DPadRight = 0x0008,
        Start = 0x0010, Back = 0x0020, LeftThumb = 0x0040, RightThumb = 0x0080,
        LeftShoulder = 0x0100, RightShoulder = 0x0200, A = 0x1000, B = 0x2000, X = 0x4000, Y = 0x8000
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Gamepad
    {
        public Buttons wButtons;
        public byte bLeftTrigger;
        public byte bRightTrigger;
        public short sThumbLX;
        public short sThumbLY;
        public short sThumbRX;
        public short sThumbRY;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct State
    {
        public uint dwPacketNumber;
        public Gamepad Gamepad;
    }

    [DllImport("xinput1_4.dll", EntryPoint = "XInputGetState")]
    private static extern uint GetStateNative(uint dwUserIndex, out State pState);

    public static bool TryGetState(int index, out State state)
    {
        try { return GetStateNative((uint)Math.Clamp(index, 0, 3), out state) == 0; }
        catch (DllNotFoundException) { state = default; return false; }
    }
}
