using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSGO_Hack_WPF.SDK
{
    internal static class Offsets
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

        private static int GetOffset(string module, string pattern, int patternOffset, int addressOffset, SignatureType type = SignatureType.READ | SignatureType.SUBTRACT)
        {
            var patt = StringToPattern(pattern);
            var address = Core.Memory.PatternScanner.FindPattern(Core.Memory.GetModule(module).BaseAddress,
                Core.Memory.GetModule(module).ModuleMemorySize - 100, patt.pattern, patt.mask) + patternOffset;
            if (type.HasFlag(SignatureType.READ))
                address = Core.Memory.Read<IntPtr>(address);
            if (type.HasFlag(SignatureType.SUBTRACT))
                address -= (int)Core.Memory.GetModule(module).BaseAddress;
            return (int)address + addressOffset;
        }

        public static void Initialize()
        {
            Misc.Initialize();
            ClientState.Initialize();
            LocalPlayer.Initialize();
        }



        public static class Misc
        {
            public static int EntityList;
            public static int ViewMatrix;
            public static int Jump;
            public static int GlowIndex = 0x0000A320;
            public static int GlowObject;
            public static int ForceAttack;
            public static int Sensitivity;
            public static int RadarBase;
            public static int RadarBasePointer = 0x50;
            public static void Initialize()
            {
                GlowObject = GetOffset("client.dll", "A1 ? ? ? ? A8 01 75 4B", 0x1, 0x4);
                EntityList = GetOffset("client.dll", "BB ? ? ? ? 83 FF 01 0F 8C ? ? ? ? 3B F8", 0x1, 0x0);
                ViewMatrix = GetOffset("client.dll", "0F 10 05 ? ? ? ? 8D 85 ? ? ? ? B9", 0x3, 0xB0);
                Jump = GetOffset("client.dll", "89 0D ? ? ? ? 8B 0D ? ? ? ? 8B F2 8B C1 83 CE 08", 0x2, 0x0);
                ForceAttack = GetOffset("client.dll", "89 0D ? ? ? ? 8B 0D ? ? ? ? 8B F2 8B C1 83 CE 04", 0x2, 0x0);
                Sensitivity = GetOffset("client.dll", "81 F9 ? ? ? ? 75 1D F3 0F 10 05 ? ? ? ? F3 0F 11 44 24 ? 8B 44 24 18 35 ? ? ? ? 89 44 24 0C EB 0B", 0x2, 0x2C);
                RadarBase = GetOffset("client.dll", "A1 ? ? ? ? 8B 0C B0 8B 01 FF 50 ? 46 3B 35 ? ? ? ? 7C EA 8B 0D", 0x1, 0x0);
            }
        }

        public static class ClientState
        {
            public static int Base; //ClientState
            public static int LocalPlayerIndex = 0x00000180;
            public static int GameState = 0x00000108; //Ingame
            public static int ViewAngles = 0x00004D10;

            public static void Initialize()
            {
                Base = GetOffset("engine.dll", "A1 ? ? ? ? 33 D2 6A 00 6A 00 33 C9 89 B0", 0x1, 0x0);
            }
        }

        public static class BaseEntity
        {
            public static int Position = 0x00000134; //m_vecOrigin
            public static int Team = 0xF0;
            public static int Armor = 0xA8F4;
            public static int VecMin = 0x0320;
            public static int VecMax = 0x032C;
            public static int ClrRender = 0x0070;
            public static int Health = 0xFC;
            public static int Dormant = 0xE9;
            public static int Index = 0x64;
            public static int EntitySize = 0x10;
            public static int Spotted = 0x00000939;
            public static int BoneMatrix = 0x00002698;
            public static int SpottedByMask = 0x0000097C;
        }

        public static class Player
        {
            public static int LifeState = 0x25B;
            public static int Flags = 0x100;
            public static int ActiveWeapon = 0x00002EE8;
            public static int VecVelocity = 0x110;
            public static int GunGameImmune = 0x000038B0;
            public static int FlashMaxAlpha = 0x0000A304;
            public static int Spec = 0x171C;
        }

        public static class LocalPlayer
        {
            public static int Base; //LocalPlayer
            public static int CrosshairId = 0xAA70;
            public static int VecViewOffset = 0x104;
            public static int VecPunch = 0x0000301C;
            public static int ShotsFired = 0x0000A2C0;
            public static void Initialize()
            {
                Base = GetOffset("client.dll", "A3 ? ? ? ? C7 05 ? ? ? ? ? ? ? ? E8 ? ? ? ? 59 C3 6A ?", 0x1, 0x10);
            }
        }

        public static class Weapon
        {
            public static int State = 0x000031F8;
            public static int Clip1 = 0x00003204;
            public static int WeaponId = 0x000032EC;
            public static int ZoomLevel = 0x00003350;
        }
    }
}