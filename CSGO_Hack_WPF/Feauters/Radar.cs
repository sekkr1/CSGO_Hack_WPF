using CSGO_Hack_WPF.SDK;
using CSGO_Hack_WPF.Utils;

namespace CSGO_Hack_WPF.Feauters
{
    public class Radar
    {
        private bool _radar;

        public void Update()
        {
            if (!MiscUtils.ShouldUpdate())
                return;

            ReadSettings();

            if (!_radar)
                return;

            foreach (var player in Core.Objects.Players)
            {
                if (player.Team == Core.LocalPlayer.Team)
                    continue;
                if (!player.IsAlive)
                    continue;

                if (!player.IsDormant && !player.IsSpotted)
                    Core.Memory.Write(player.BaseAddress + Offsets.BaseEntity.Spotted, 1);
            }
        }

        private void ReadSettings()
        {
            _radar = Properties.Settings.Default.radar;
        }
    }
}