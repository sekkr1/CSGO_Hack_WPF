using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using MahApps.Metro;
using Orion.GlobalOffensive.Objects;
using System.Speech.Synthesis;
namespace CSGO_Hack_WPF
{
    using NHotkey;
    using Overlay.NET.Directx;
    using Overlay.NET.Wpf;
    using Process.NET;
    using Process.NET.Memory;
    using System.Linq;
    using System.Numerics;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using GameManager = Orion.GlobalOffensive.Orion;
    using Patchables = Orion.GlobalOffensive.Patchables;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static ManualResetEventSlim infoLock = new ManualResetEventSlim();
        Thread triggerThread, jumpThread;
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;
        private const int KEYDOWN = 0x1;
        DirectXOverlayWindow wpfOverlay;
        SpeechSynthesizer synth = new SpeechSynthesizer();
        private GlobalKeyboardHook _globalKeyboardHook;
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetAsyncKeyState(int keyCode);

        bool triggerOn = false;
        public void SetupKeyboardHooks()
        {
            _globalKeyboardHook = new GlobalKeyboardHook();
            _globalKeyboardHook.KeyboardPressed += OnKeyPressed;
        }
        bool attackState = false;
        public MainWindow()
        {
            //if (Debugger.IsAttached) Properties.Settings.Default.Reset();
            InitializeComponent();
            SetupKeyboardHooks();
            var proc = System.Diagnostics.Process.GetProcessesByName("csgo")[0];
            var _processSharp = new ProcessSharp(proc, MemoryType.Remote);
            wpfOverlay = new DirectXOverlayWindow(_processSharp.WindowFactory.MainWindow.Handle, false);
            GameManager.Attach(proc);
            synth.SetOutputToDefaultAudioDevice();
            NHotkey.Wpf.HotkeyManager.Current.AddOrReplace("trigger", Key.numpad, ModifierKeys.None, (sender, e) => hotkey(e.Name, ref triggerOn));
            if (Properties.Settings.Default.darkMode)
                ThemeManager.ChangeAppStyle(Application.Current,
                                    ThemeManager.GetAccent("Blue"),
                                    ThemeManager.GetAppTheme("BaseDark"));
            SourceInitialized += Window_SourceInitialized;

            new Thread(ESP) { IsBackground = true }.Start();
        }
        void hotkey(string name, ref bool change)
        {
            change = !change;
            synth.SpeakAsync(name + " " + (change ? "On" : "Off"));
        }
        private void OnKeyPressed(object sender, GlobalKeyboardHookEventArgs e)
        {
        }
        void jump()
        {
            GameManager.Memory.Write(GameManager.ClientBase + (int)Patchables.StaticOffsets.ForceJump, 5);
            Thread.Sleep(10);
            GameManager.Memory.Write(GameManager.ClientBase + (int)Patchables.StaticOffsets.ForceJump, 4);
        }
        void ESP()
        {
            while (true)
            {
                GameManager.Update();
                if (!(GameManager.Client.InGame && (GameManager.Me?.IsValid ?? false))) continue;
                var me = GameManager.Me;
                var t = me.Target;
                var asd = me.Weapon;
                Application.Current.Dispatcher.Invoke(delegate
                {
                    pos.Content = asd.ItemDefinitionIndex + "\n" + asd.Id;
                });
                //BHop
                if (Properties.Settings.Default.bhop && me.IsAlive && GetAsyncKeyState(0x20) != 0 && (jumpThread == null || !jumpThread.IsAlive))
                    if (me.Flags == 257)
                    {
                        jumpThread = new Thread(jump) { IsBackground = true };
                        jumpThread.Start();
                    }

                //No flash
                var perc = Properties.Settings.Default.noFlashPercentage * (255 / 100f);
                if (Properties.Settings.Default.noFlash && me.FlashMaxAlpha != perc)
                    me.FlashMaxAlpha = perc;
                else if (!Properties.Settings.Default.noFlash && me.FlashMaxAlpha != 255f)
                    me.FlashMaxAlpha = 255f;

                //Trigger
                if (Properties.Settings.Default.trigger && me.IsAlive && triggerOn)
                {
                    if (t != null && t.clsId == (int)ClassID.CCSPlayerResource && !t.IsFriendly)
                    {
                        if (triggerThread == null || !triggerThread.IsAlive)
                        {
                            triggerThread = new Thread(Shoot) { IsBackground = true };
                            triggerThread.Start();
                        }
                    }
                    else if (attackState) Attack();
                }
                else if (attackState) Attack();

                //ESP
                Application.Current.Dispatcher.Invoke(delegate
                {
                    wpfOverlay.Graphics.BeginScene();
                    wpfOverlay.Graphics.ClearScene();
                });
                foreach (GlowObject glowObj in GameManager.GlowObjects.glowObjects)
                {
                    var entity = glowObj.entity;
                    if (!(entity != null && !entity.IsDormant && (entity.clsId == (int)ClassID.CCSPlayerResource || entity.clsId == 107) && entity.IsAlive)) continue;
                    if (entity.IsFriendly)
                        glowObj.Glow(new Orion.GlobalOffensive.Objects.Color(0, 0, 1, 0.5f), Properties.Settings.Default.chams);
                    else
                    {
                        glowObj.Glow(new Orion.GlobalOffensive.Objects.Color(1 - (entity.Health / 100f), entity.Health / 100f, 0, (t?.Id == entity.Id) ? 1f : 0.75f), Properties.Settings.Default.chams);

                        var distance = entity.Distance;
                        Vector3? originW2S = WorldToScreen(entity.Position),
                            headW2S = WorldToScreen(entity.HeadPos),
                            topW2S = WorldToScreen(new Vector3(entity.HeadPos.X, entity.HeadPos.Y, entity.HeadPos.Z + 7));

                        //draw Bones
                        /*for (int i = 0; i < 81; i++)
                        {
                            var boneW2S = WorldToScreen(me.ViewMatrix, entity.getBone(i));
                            if(boneW2S!=null)
                            wpfOverlay.Graphics.DrawText(i.ToString(), wpfOverlay.Graphics.CreateFont("Arial", 10), wpfOverlay.Graphics.CreateBrush(0x7FFFFFFF), (int)boneW2S?.X, (int)boneW2S?.Y );
                        }*/

                        if (Properties.Settings.Default.headHelper && headW2S != null)
                            wpfOverlay.Graphics.DrawCircle((int)headW2S?.X, (int)headW2S?.Y, (int)(15 * (300 / distance)), 2, wpfOverlay.Graphics.CreateBrush(0x7FFF0000));

                        if (Properties.Settings.Default.d3box || Properties.Settings.Default.d2box)
                        {
                            var min = entity.vecMin + entity.Position;
                            var max = entity.vecMax + entity.Position;
                            Vector3? blb = WorldToScreen(new Vector3(min.X, min.Y, min.Z)),
                                brb = WorldToScreen(new Vector3(min.X, max.Y, min.Z)),
                                frb = WorldToScreen(new Vector3(max.X, max.Y, min.Z)),
                                flb = WorldToScreen(new Vector3(max.X, min.Y, min.Z)),
                                frt = WorldToScreen(new Vector3(max.X, max.Y, max.Z)),
                                brt = WorldToScreen(new Vector3(min.X, max.Y, max.Z)),
                                blt = WorldToScreen(new Vector3(min.X, min.Y, max.Z)),
                                flt = WorldToScreen(new Vector3(max.X, min.Y, max.Z));
                            if (flt != null && frt != null && flb != null && frb != null && blb != null && brb != null && brt != null && blt != null)
                            {
                                if (Properties.Settings.Default.d2box)
                                {
                                    wpfOverlay.Graphics.DrawLine((int)blt?.X, (int)blt?.Y, (int)frt?.X, (int)frt?.Y, 3, wpfOverlay.Graphics.CreateBrush(0x7FFF0000));
                                    wpfOverlay.Graphics.DrawLine((int)blt?.X, (int)blt?.Y, (int)blb?.X, (int)blb?.Y, 3, wpfOverlay.Graphics.CreateBrush(0x7FFF0000));
                                    wpfOverlay.Graphics.DrawLine((int)frb?.X, (int)frb?.Y, (int)frt?.X, (int)frt?.Y, 3, wpfOverlay.Graphics.CreateBrush(0x7FFF0000));
                                    wpfOverlay.Graphics.DrawLine((int)frb?.X, (int)frb?.Y, (int)blb?.X, (int)blb?.Y, 3, wpfOverlay.Graphics.CreateBrush(0x7FFF0000));
                                }
                                if (Properties.Settings.Default.d3box)
                                {
                                    wpfOverlay.Graphics.DrawLine((int)flt?.X, (int)flt?.Y, (int)frt?.X, (int)frt?.Y, 3, wpfOverlay.Graphics.CreateBrush(0x7FFF0000));
                                    wpfOverlay.Graphics.DrawLine((int)flt?.X, (int)flt?.Y, (int)flb?.X, (int)flb?.Y, 3, wpfOverlay.Graphics.CreateBrush(0x7FFF0000));
                                    wpfOverlay.Graphics.DrawLine((int)frb?.X, (int)frb?.Y, (int)frt?.X, (int)frt?.Y, 3, wpfOverlay.Graphics.CreateBrush(0x7FFF0000));
                                    wpfOverlay.Graphics.DrawLine((int)frb?.X, (int)frb?.Y, (int)flb?.X, (int)flb?.Y, 3, wpfOverlay.Graphics.CreateBrush(0x7FFF0000));

                                    wpfOverlay.Graphics.DrawLine((int)blt?.X, (int)blt?.Y, (int)brt?.X, (int)brt?.Y, 3, wpfOverlay.Graphics.CreateBrush(0x7FFF0000));
                                    wpfOverlay.Graphics.DrawLine((int)blt?.X, (int)blt?.Y, (int)blb?.X, (int)blb?.Y, 3, wpfOverlay.Graphics.CreateBrush(0x7FFF0000));
                                    wpfOverlay.Graphics.DrawLine((int)brb?.X, (int)brb?.Y, (int)brt?.X, (int)brt?.Y, 3, wpfOverlay.Graphics.CreateBrush(0x7FFF0000));
                                    wpfOverlay.Graphics.DrawLine((int)brb?.X, (int)brb?.Y, (int)blb?.X, (int)blb?.Y, 3, wpfOverlay.Graphics.CreateBrush(0x7FFF0000));

                                    wpfOverlay.Graphics.DrawLine((int)flt?.X, (int)flt?.Y, (int)blt?.X, (int)blt?.Y, 3, wpfOverlay.Graphics.CreateBrush(0x7FFF0000));
                                    wpfOverlay.Graphics.DrawLine((int)frt?.X, (int)frt?.Y, (int)brt?.X, (int)brt?.Y, 3, wpfOverlay.Graphics.CreateBrush(0x7FFF0000));
                                    wpfOverlay.Graphics.DrawLine((int)flb?.X, (int)flb?.Y, (int)blb?.X, (int)blb?.Y, 3, wpfOverlay.Graphics.CreateBrush(0x7FFF0000));
                                    wpfOverlay.Graphics.DrawLine((int)frb?.X, (int)frb?.Y, (int)brb?.X, (int)brb?.Y, 3, wpfOverlay.Graphics.CreateBrush(0x7FFF0000));
                                }
                            }

                            if (Properties.Settings.Default.distance)
                                wpfOverlay.Graphics.DrawText(((int)distance).ToString(), wpfOverlay.Graphics.CreateFont("Arial", 8), wpfOverlay.Graphics.CreateBrush(0x7FFFFFFF), (int)headW2S?.X - ((int)distance).ToString().Length / 2 * 5, (int)(headW2S?.Y - 40 * (300 / distance)));
                        }
                    }
                }
                Application.Current.Dispatcher.Invoke(delegate { wpfOverlay.Graphics.EndScene(); });
                Thread.Sleep(1);
            }
        }
        void Shoot()
        {
            if (Properties.Settings.Default.triggerDelay > 0)
                Thread.Sleep(Properties.Settings.Default.triggerDelay);
            if (!attackState)
                Attack();
            Thread.Sleep(1);
        }
        void Attack()
        {
            if (!attackState) // +attack
            {
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                attackState = true;
            }
            else // -attack
            {
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                attackState = false;
            }
            Thread.Sleep(1);
        }
        void fetchInfo()
        {
        }
        Vector3? WorldToScreen(Vector3 Position)
        {
            var _Result = new Vector3();
            var ViewMatrix = GameManager.Me.ViewMatrix;
            _Result.X = (ViewMatrix.M11 * Position.X) + (ViewMatrix.M12 * Position.Y) + (ViewMatrix.M13 * Position.Z) + ViewMatrix.M14;
            _Result.Y = (ViewMatrix.M21 * Position.X) + (ViewMatrix.M22 * Position.Y) + (ViewMatrix.M23 * Position.Z) + ViewMatrix.M24;
            _Result.Z = (ViewMatrix.M41 * Position.X) + (ViewMatrix.M42 * Position.Y) + (ViewMatrix.M43 * Position.Z) + ViewMatrix.M44;

            if (_Result.Z < 0.01f)
                return null;

            float invw = 1.0f / _Result.Z;
            _Result.X *= invw;
            _Result.Y *= invw;

            float x = 1920 / 2;
            float y = 1080 / 2;

            x += 0.5f * _Result.X * 1920 + 0.5f;
            y -= 0.5f * _Result.Y * 1080 + 0.5f;

            _Result.X = x + 0;
            _Result.Y = y + 0;

            return _Result;
        }
        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void TabControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.popupShown) return;
            new FirstTimePopup().ShowDialog();
            Properties.Settings.Default.popupShown = true;
        }

        private void ToggleSwitch_OnIsCheckedChanged(object sender, EventArgs e)
        {
            ThemeManager.ChangeAppStyle(Application.Current,
                ThemeManager.GetAccent("Blue"),
                Properties.Settings.Default.darkMode
                    ? ThemeManager.GetAppTheme("BaseDark")
                    : ThemeManager.GetAppTheme("BaseLight"));
        }


        private double _aspectRatio;
        private bool? _adjustingHeight;

        internal enum SWP
        {
            NOMOVE = 0x0002
        }
        internal enum WM
        {
            WINDOWPOSCHANGING = 0x0046,
            EXITSIZEMOVE = 0x0232,
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        public static Point GetMousePosition() // mouse position relative to screen
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }


        private void Window_SourceInitialized(object sender, EventArgs ea)
        {
            HwndSource hwndSource = (HwndSource)HwndSource.FromVisual((Window)sender);
            hwndSource.AddHook(DragHook);

            _aspectRatio = Width / Height;
        }



        private IntPtr DragHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((WM)msg)
            {
                case WM.WINDOWPOSCHANGING:
                    {
                        WINDOWPOS pos = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));

                        if ((pos.flags & (int)SWP.NOMOVE) != 0)
                            return IntPtr.Zero;

                        Window wnd = (Window)HwndSource.FromHwnd(hwnd).RootVisual;
                        if (wnd == null)
                            return IntPtr.Zero;

                        // determine what dimension is changed by detecting the mouse position relative to the 
                        // window bounds. if gripped in the corner, either will work.
                        if (!_adjustingHeight.HasValue)
                        {
                            Point p = GetMousePosition();

                            double diffWidth = Math.Min(Math.Abs(p.X - pos.x), Math.Abs(p.X - pos.x - pos.cx));
                            double diffHeight = Math.Min(Math.Abs(p.Y - pos.y), Math.Abs(p.Y - pos.y - pos.cy));

                            _adjustingHeight = diffHeight > diffWidth;
                        }

                        if (_adjustingHeight.Value)
                            pos.cy = (int)(pos.cx / _aspectRatio); // adjusting height to width change
                        else
                            pos.cx = (int)(pos.cy * _aspectRatio); // adjusting width to heigth change

                        Marshal.StructureToPtr(pos, lParam, true);
                        handled = true;
                    }
                    break;
                case WM.EXITSIZEMOVE:
                    _adjustingHeight = null; // reset adjustment dimension and detect again next time window is resized
                    break;
            }

            return IntPtr.Zero;
        }
    }
}
