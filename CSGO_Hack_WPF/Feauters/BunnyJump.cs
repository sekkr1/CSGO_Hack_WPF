using System;
using CSGO_Hack_WPF.SDK;
using CSGO_Hack_WPF.Utils;

namespace CSGO_Hack_WPF.Feauters
{
    public class BunnyJump
    {
        #region Methods

        public void Update()
        {
            if (!MiscUtils.ShouldUpdate(false, false, false))
                return;

            ReadSettings();

            if (!_bunnyJumpEnabled)
                return;

            if (Core.LocalPlayer.Velocity <= 100)
                return;

            BHop();
        }

        private void ReadSettings()
        {
            _bunnyJumpEnabled = Properties.Settings.Default.bhop;
            _bunnyJumpKey = WinAPI.VirtualKeyShort.SPACE;
        }

        private void BHop()
        {
            if (Core.KeyUtils.KeyIsDown(_bunnyJumpKey))
                Core.Memory.Write(Core.ClientBase + Offsets.Misc.Jump, Core.LocalPlayer.InAir ? 4 : 5);
        }

        #endregion

        #region Fields

        private bool _bunnyJumpEnabled;
        private WinAPI.VirtualKeyShort _bunnyJumpKey;

        #endregion
    }
}