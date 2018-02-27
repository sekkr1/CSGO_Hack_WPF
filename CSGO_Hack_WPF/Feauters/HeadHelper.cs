using CSGO_Hack_WPF.SDK;
using CSGO_Hack_WPF.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace CSGO_Hack_WPF.Feauters
{
    public class HeadHelper
    {
        public bool _headHelper;

        public HeadHelper()
        {

        }

        public void Update()
        {
            if (!MiscUtils.ShouldUpdate(false, false, false))
                return;

            ReadSettings();

            foreach (var player in Core.Objects.Players.Where(player => !player.IsDormant && player.IsAlive && !player.IsFriendly))
            {
                var headPos = player.HeadPos;
                var headW2S = MathUtils.WorldToScreen(headPos);
                var aboveHeadW2S = MathUtils.WorldToScreen(Vector3.Add(headPos, new Vector3(0, 0, 6.5f)));
                if (headW2S != null && aboveHeadW2S != null)
                    Core.Renderer.Graphics.DrawCircle((int)headW2S?.X, (int)headW2S?.Y, (int)(headW2S?.Y - aboveHeadW2S?.Y), 4, Core.Renderer.Graphics.CreateBrush(0x7FFF0000));
            }
        }

        private void ReadSettings()
        {
            _headHelper = Properties.Settings.Default.headHelper;
        }
    }
}
