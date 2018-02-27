  using CSGO_Hack_WPF.SDK;
using CSGO_Hack_WPF.Utils;

namespace CSGO_Hack_WPF.Feauters
{
    public class NoFlash
    {
        private bool _noFlashActive;
        private float _noFlashPerc;

        public void Update()
        {
            if (!MiscUtils.ShouldUpdate(false, false, false))
                return;

            ReadSettings();

            if (!_noFlashActive)
            {
                if (Core.LocalPlayer.FlashMaxAlpha != 255f)
                    Core.Memory.Write(Core.LocalPlayer.BaseAddress + Offsets.Player.FlashMaxAlpha, 255f);
                return;
            }

            if (Core.LocalPlayer.FlashMaxAlpha != _noFlashPerc)
                Core.Memory.Write(Core.LocalPlayer.BaseAddress + Offsets.Player.FlashMaxAlpha, _noFlashPerc);
        }

        private void ReadSettings()
        {
            _noFlashActive = Properties.Settings.Default.noFlash;
            _noFlashPerc = Properties.Settings.Default.noFlashPercentage * (255f / 100);
        }
    }
}