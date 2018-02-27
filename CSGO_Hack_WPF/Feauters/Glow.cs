using System;
using System.Linq;
using CSGO_Hack_WPF.SDK;
using CSGO_Hack_WPF.Utils;

namespace CSGO_Hack_WPF.Feauters
{
    public class Glow
    {
        #region Fields

        private bool _glowActive, _glowFriendly;

        #endregion

        #region Properties

        public IntPtr GlowPointer { get; set; }

        #endregion

        #region Constructor

        public Glow()
        {
            GlowPointer = Core.Memory.Read<IntPtr>(Core.ClientBase + Offsets.Misc.GlowObject);
        }

        #endregion

        #region Methods

        public void Update()
        {
            if (!MiscUtils.ShouldUpdate(false, false, false))
                return;

            ReadSettings();

            if (!_glowActive)
                return;

            DoGlow();
        }

        private void ReadSettings()
        {
            _glowActive = Properties.Settings.Default.glow;
            _glowFriendly = Properties.Settings.Default.glow_friendly;
        }

        private void DoGlow()
        {
            #region Player Glow

            foreach (var player in Core.Objects.Players.Where(player => !player.IsDormant && player.IsAlive))
            {
                if (!player.IsFriendly || player.IsFriendly && _glowFriendly)
                {
                    Core.Memory.Write(GlowPointer + player.GlowIndex * 0x38 + 0x4, (100 - player.Health) * (1f / 100)); //red
                    Core.Memory.Write(GlowPointer + player.GlowIndex * 0x38 + 0x8, player.Health * (1f / 100)); //green
                    Core.Memory.Write(GlowPointer + player.GlowIndex * 0x38 + 0xC, !player.IsFriendly ? 0.0f : 1f); //blue
                    Core.Memory.Write(GlowPointer + player.GlowIndex * 0x38 + 0x10, 255f);//alpha
                    Core.Memory.Write(GlowPointer + player.GlowIndex * 0x38 + 0x24, true);
                    Core.Memory.Write(GlowPointer + player.GlowIndex * 0x38 + 0x25, false);
                }
            }

            #endregion
        }
        #endregion
    }
}