using System;
using CSGO_Hack_WPF.Enums;
using CSGO_Hack_WPF.SDK;
using CSGO_Hack_WPF.Utils;

namespace CSGO_Hack_WPF.Feauters
{
    public class AutoPistol
    {
        private bool _autoPistol;
        private WinAPI.VirtualKeyShort _autoPistolKey;
        private int _delay;
        private long _lastShot;

        public void Update()
        {
            if (!MiscUtils.ShouldUpdate())
                return;

            if (Core.LocalPlayerWeapon.WeaponType != WeaponType.Pistol)
                return;

            ReadSettings();

            if (!_autoPistol)
                return;

            if (Core.KeyUtils.KeyIsDown(_autoPistolKey))
            {
                if (!(new TimeSpan(DateTime.Now.Ticks - _lastShot).TotalMilliseconds >= _delay))
                    return;

                _lastShot = DateTime.Now.Ticks;

                Engine.ForceAttack(0, 12, 10);
            }
        }

        private void ReadSettings()
        {
            /*_autoPistol = Properties.Settings.autoPistol;
            _delay = Properties.Settings.autoPistolDelay;
            _autoPistolKey = Properties.Settings.autoPistolKey;*/
        }
    }
}