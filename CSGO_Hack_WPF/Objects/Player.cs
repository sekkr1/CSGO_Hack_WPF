using System;
using System.Numerics;
using CSGO_Hack_WPF.SDK;
using System.Text;

namespace CSGO_Hack_WPF.Objects
{
    public class Player : BaseEntity
    {
        public Player(IntPtr baseAddress) : base(baseAddress) { }

        public Vector3 VecVelocity => ReadField<Vector3>(Offsets.Player.VecVelocity);
        public float FlashMaxAlpha => ReadField<float>(Offsets.Player.FlashMaxAlpha);
        public bool InAir => Flags == 256 || Flags == 262;
        public Vector3 HeadPos => GetBone(8);
        public bool IsSpectating
        {
            get
            {
                /*if (!IsValid)
                    return false;
                if (Health >= 2 && Team != Enums.PlayerTeam.Neutral)
                    return false;
                if (DistanceSquared > 2500)
                    return false;*/
                return ReadField<int>(Offsets.Player.Spec) == 4;
            }
        }
        public Weapon CurrentWeapon
        {
            get
            {
                if (!IsValid)
                    return null;

                int wepptr = ReadField<int>(Offsets.Player.ActiveWeapon);
                int wepptr1 = wepptr & 0xfff;
                return new Weapon(Core.Memory.Read<IntPtr>(Core.ClientBase + Offsets.Misc.EntityList + (wepptr1 - 1) * 0x10));
            }
        }
        public string Name
        {
            get
            {
                var dwRadarBase = Core.Memory.Read<IntPtr>(Core.ClientBase + Offsets.Misc.RadarBase);
                var dwRadarPointer = Core.Memory.Read<IntPtr>(dwRadarBase + Offsets.Misc.RadarBasePointer);
                var name = Core.Memory.ReadString(dwRadarPointer + 0x1E0 * (Id + 1) + 0x24, Encoding.Unicode, 32);
                return name;
            }
        }

        public Vector3 GetBone(int bone)
        {
            int matrix = ReadField<int>(Offsets.BaseEntity.BoneMatrix);
            Vector3 bonePos = new Vector3
            {
                X = Core.Memory.Read<float>((IntPtr)(matrix + 0x30 * bone + 0xC)),
                Y = Core.Memory.Read<float>((IntPtr)(matrix + 0x30 * bone + 0x1C)),
                Z = Core.Memory.Read<float>((IntPtr)(matrix + 0x30 * bone + 0x2C))
            };
            return bonePos;
        }
    }
}