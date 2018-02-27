using System.Text;
using CSGO_Hack_WPF.Enums;
using CSGO_Hack_WPF.SDK;

namespace CSGO_Hack_WPF.Utils
{
    class MiscUtils
    {
        public static bool ShouldUpdate(bool checkKnife = true, bool checkGrenades = true, bool checkMisc = true)
        {
            //if (WindowTitle != Smurf.GameTitle)
            //    return false;

            if (Core.LocalPlayer == null)
                return false;

            if (Core.LocalPlayerWeapon == null)
                return false;

            if (Core.Client.State != SignonState.Full)
                return false;

            if (checkMisc)
                if (Core.LocalPlayerWeapon.WeaponType == WeaponType.Unkown)
                    return false;

            if (checkGrenades)
                if (Core.LocalPlayerWeapon.WeaponType == WeaponType.Grenade)
                    return false;

            if (checkKnife)
                if (Core.LocalPlayerWeapon.WeaponType == WeaponType.Knife)
                    return false;

            return true;
        }

        public static WeaponConfig GetCurrentWeaponConfig()
        {
            switch (Core.LocalPlayerWeapon.WeaponType)
            {
                case WeaponType.Pistol:
                    return Properties.Settings.Default.pistolConfig;
                case WeaponType.Smg:
                    return Properties.Settings.Default.smgConfig;
                case WeaponType.Rifle:
                    return Properties.Settings.Default.rifleConfig;
                case WeaponType.Sniper:
                    return Properties.Settings.Default.sniperConfig;
                case WeaponType.Shotgun:
                    return Properties.Settings.Default.shotgunConfig;
                case WeaponType.Heavy:
                    return Properties.Settings.Default.heavyConfig;
                default:
                    return new WeaponConfig() { triggerConfig = new TriggerConfig() { enabled = false }, rcsConfig = new RCSConfig() { enabled = false } };
            }
        }

        public static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            var builder = new StringBuilder(nChars);
            var handle = WinAPI.GetForegroundWindow();

            if (WinAPI.GetWindowText(handle, builder, nChars) > 0)
                return builder.ToString();

            return null;
        }
    }
}
