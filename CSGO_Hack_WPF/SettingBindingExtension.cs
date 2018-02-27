using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CSGO_Hack_WPF
{
    public class SettingBindingExtension : Binding
    {
        public SettingBindingExtension()
        {
            Initialize();
        }

        public SettingBindingExtension(string path)
            : base(path)
        {
            Initialize();
        }
        public SettingBindingExtension(string wepType,string config, string path)
            : base(path)
        {
            switch (config)
            {
                case "trigger":
                    this.Source = ((WeaponConfig)Properties.Settings.Default[wepType + "Config"]).triggerConfig;
                    break;
                case "rcs":
                    this.Source = ((WeaponConfig)Properties.Settings.Default[wepType + "Config"]).rcsConfig;
                    break;
            }
            this.Mode = BindingMode.TwoWay;
        }

        private void Initialize()
        {
            this.Source = Properties.Settings.Default;
            this.Mode = BindingMode.TwoWay;
        }
    }
}
