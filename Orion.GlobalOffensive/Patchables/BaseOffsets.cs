// Copyright (C) 2015 aevitas
// See the file LICENSE for copying permission.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.GlobalOffensive.Patchables
{
    struct Pattern
    {
        public byte[] pattern;
        public string mask;
    }
    [Flags]
    public enum SignatureType
    {
        NONE = 0,
        READ = 1 << 0,
        SUBTRACT = 1 << 1
    }
    // Offsets that change at least once per build, and are platform specific.
    public class BaseOffsets
    {
        public static IntPtr ViewMatrix;
        public static IntPtr EntityList;
        public static IntPtr ClientState;
        public static IntPtr LocalPlayer;
        public static IntPtr GlowObjBase;

        public static void Initialize()
        {
            LocalPlayer = GetOffset("client.dll", "A3 ? ? ? ? C7 05 ? ? ? ? ? ? ? ? E8 ? ? ? ? 59 C3 6A", 0x1, 0x2C, SignatureType.READ | SignatureType.SUBTRACT);
            ClientState = GetOffset("engine.dll", "A1 ? ? ? ? F3 0F 11 80 ? ? ? ? D9 46 04 D9 05", 0x1, 0x0, SignatureType.READ | SignatureType.SUBTRACT);
            GlowObjBase = GetOffset("client.dll", "A1 ? ? ? ? A8 01 75 4E 0F 57 C0", 0x58, 0x0, SignatureType.READ | SignatureType.SUBTRACT);
            EntityList = GetOffset("client.dll", "BB ? ? ? ? 83 FF 01 0F 8C ? ? ? ? 3B F8", 0x1, 0x0, SignatureType.READ | SignatureType.SUBTRACT);
            ViewMatrix = GetOffset("client.dll", "81 C6 ? ? ? ? 88 45 9A 0F B6 C0", 0x352, 0xB0, SignatureType.READ | SignatureType.SUBTRACT);
        }

        private static Pattern StringToPattern(string hex)
        {
            var pattern = new List<byte>();
            var sb = new StringBuilder();
            foreach (var c in hex.Replace("?", "00").Split(' '))
            {
                pattern.Add(Convert.ToByte(c, 16));
                sb.Append(pattern.Last() == 0x0 ? "?" : "x");
            }
            return new Pattern { pattern = pattern.ToArray(), mask = sb.ToString() };
        }

        private static IntPtr GetOffset(string module, string pattern, int patternOffset, int addressOffset, SignatureType type)
        {
            var patt = StringToPattern(pattern);
            var address = Orion.Memory.PatternScanner.FindPattern(Orion.Memory.GetModule(module).BaseAddress,
                Orion.Memory.GetModule(module).ModuleMemorySize - 100, patt.pattern, patt.mask) + patternOffset;
            if (type.HasFlag(SignatureType.READ))
                address = Orion.Memory.Read<IntPtr>(address);
            if (type.HasFlag(SignatureType.SUBTRACT))
                address -= (int)Orion.Memory.GetModule(module).BaseAddress;
            return address + addressOffset;
        }
    }
}