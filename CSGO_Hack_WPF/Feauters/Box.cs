using CSGO_Hack_WPF.SDK;
using CSGO_Hack_WPF.Utils;
using System.Linq;
using System.Numerics;

namespace CSGO_Hack_WPF.Feauters
{
    public class Box
    {
        private bool _d2Box, _d2BoxFriendly, _d3Box, _d3BoxFriendly;
        private Vector2[] _box;

        public Box()
        {
            _box = new Vector2[8];
        }

        public void Update()
        {
            if (!MiscUtils.ShouldUpdate(false, false, false))
                return;

            ReadSettings();

            if (!_d2Box && !_d3Box)
                return;

            foreach (var player in Core.Objects.Players.Where(player => !player.IsDormant && player.IsAlive))
            {
                if (!_d2BoxFriendly && !_d3BoxFriendly && player.IsFriendly)
                    continue;

                var min = player.VecMin + player.Position;
                var max = player.VecMax + player.Position;

                var tempBox = new Vector2?[8];
                tempBox[0] = MathUtils.WorldToScreen(new Vector3(min.X, min.Y, min.Z));
                tempBox[1] = MathUtils.WorldToScreen(new Vector3(min.X, max.Y, min.Z));
                tempBox[2] = MathUtils.WorldToScreen(new Vector3(max.X, max.Y, min.Z));
                tempBox[3] = MathUtils.WorldToScreen(new Vector3(max.X, min.Y, min.Z));
                tempBox[4] = MathUtils.WorldToScreen(new Vector3(max.X, max.Y, max.Z));
                tempBox[5] = MathUtils.WorldToScreen(new Vector3(min.X, max.Y, max.Z));
                tempBox[6] = MathUtils.WorldToScreen(new Vector3(min.X, min.Y, max.Z));
                tempBox[7] = MathUtils.WorldToScreen(new Vector3(max.X, min.Y, max.Z));

                if (tempBox.Contains(null)) continue;

                _box = tempBox.Cast<Vector2>().ToArray();
                if (_d2Box)
                    if (!player.IsFriendly) Draw2D();
                    else if (_d2BoxFriendly) Draw2D();
                if (_d3Box)
                    if (!player.IsFriendly) Draw3D();
                    else if (_d3BoxFriendly) Draw3D();
            }
        }

        private void Draw3D()
        {
            Core.Renderer.Graphics.DrawLine((int)_box[7].X, (int)_box[7].Y, (int)_box[4].X, (int)_box[4].Y, 3, Core.Renderer.Graphics.CreateBrush(0x7FFF0000));
            Core.Renderer.Graphics.DrawLine((int)_box[7].X, (int)_box[7].Y, (int)_box[3].X, (int)_box[3].Y, 3, Core.Renderer.Graphics.CreateBrush(0x7FFF0000));
            Core.Renderer.Graphics.DrawLine((int)_box[2].X, (int)_box[2].Y, (int)_box[4].X, (int)_box[4].Y, 3, Core.Renderer.Graphics.CreateBrush(0x7FFF0000));
            Core.Renderer.Graphics.DrawLine((int)_box[2].X, (int)_box[2].Y, (int)_box[3].X, (int)_box[3].Y, 3, Core.Renderer.Graphics.CreateBrush(0x7FFF0000));

            Core.Renderer.Graphics.DrawLine((int)_box[6].X, (int)_box[6].Y, (int)_box[5].X, (int)_box[5].Y, 3, Core.Renderer.Graphics.CreateBrush(0x7FFF0000));
            Core.Renderer.Graphics.DrawLine((int)_box[6].X, (int)_box[6].Y, (int)_box[0].X, (int)_box[0].Y, 3, Core.Renderer.Graphics.CreateBrush(0x7FFF0000));
            Core.Renderer.Graphics.DrawLine((int)_box[1].X, (int)_box[1].Y, (int)_box[5].X, (int)_box[5].Y, 3, Core.Renderer.Graphics.CreateBrush(0x7FFF0000));
            Core.Renderer.Graphics.DrawLine((int)_box[1].X, (int)_box[1].Y, (int)_box[0].X, (int)_box[0].Y, 3, Core.Renderer.Graphics.CreateBrush(0x7FFF0000));

            Core.Renderer.Graphics.DrawLine((int)_box[7].X, (int)_box[7].Y, (int)_box[6].X, (int)_box[6].Y, 3, Core.Renderer.Graphics.CreateBrush(0x7FFF0000));
            Core.Renderer.Graphics.DrawLine((int)_box[4].X, (int)_box[4].Y, (int)_box[5].X, (int)_box[5].Y, 3, Core.Renderer.Graphics.CreateBrush(0x7FFF0000));
            Core.Renderer.Graphics.DrawLine((int)_box[3].X, (int)_box[3].Y, (int)_box[0].X, (int)_box[0].Y, 3, Core.Renderer.Graphics.CreateBrush(0x7FFF0000));
            Core.Renderer.Graphics.DrawLine((int)_box[2].X, (int)_box[2].Y, (int)_box[1].X, (int)_box[1].Y, 3, Core.Renderer.Graphics.CreateBrush(0x7FFF0000));
        }

        private void Draw2D()
        {
            float left = _box[0].X,
                top = _box[0].Y,
                bottom = _box[0].Y,
                right = _box[0].X;
            for (var i = 1; i < 8; i++)
            {
                if (_box[i].X < left) left = _box[i].X;
                if (_box[i].X > right) right = _box[i].X;
                if (_box[i].Y < top) top = _box[i].Y;
                if (_box[i].Y > bottom) bottom = _box[i].Y;
            }
            Core.Renderer.Graphics.DrawRectangle((int)left, (int)top, (int)(right - left), (int)(bottom - top), 3, Core.Renderer.Graphics.CreateBrush(0x7FFF0000));
        }

        private void ReadSettings()
        {
            _d2Box = Properties.Settings.Default.d2box;
            _d2BoxFriendly = Properties.Settings.Default.d2box_friendly;
            _d3Box = Properties.Settings.Default.d3box;
            _d3BoxFriendly = Properties.Settings.Default.d3box_friendly;
        }
    }
}
