using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using MahApps.Metro;
using System.Threading;
using CSGO_Hack_WPF.SDK;
using System.Configuration;
using CSGO_Hack_WPF.Feauters;
using Process.NET;
using Process.NET.Memory;
using Overlay.NET.Directx;
using System.Windows.Input;
using System.Xml.Serialization;
using System.Windows.Data;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace CSGO_Hack_WPF
{
    [Serializable]
    public class TriggerConfig : INotifyPropertyChanged
    {
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private bool _enabled = false;
        [XmlAttribute]
        public bool enabled
        {
            get { return _enabled; }
            set { SetField(ref _enabled, value); }
        }

        private bool _camp = false;
        [XmlElement]
        public bool camp
        {
            get { return _camp; }
            set { SetField(ref _camp, value); }
        }
        private bool _zoom = false;
        [XmlElement]
        public bool zoom
        {
            get { return _zoom; }
            set { SetField(ref _zoom, value); }
        }
        private int _type = 0;
        [XmlElement]
        public int type
        {
            get { return _type; }
            set { SetField(ref _type, value); }
        }
        private int _delayFirstShot = 80;
        [XmlElement]
        public int delayFirstShot
        {
            get { return _delayFirstShot; }
            set { SetField(ref _delayFirstShot, value); }
        }
        private int _delayShots = 35;
        [XmlElement]
        public int delayShots
        {
            get { return _delayShots; }
            set { SetField(ref _delayShots, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    [Serializable]
    public class RCSConfig : INotifyPropertyChanged
    {
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        private bool _enabled = false;
        [XmlAttribute]
        public bool enabled
        {
            get { return _enabled; }
            set { SetField(ref _enabled, value); }
        }
        private int _strengthMin = 0;
        [XmlElement]
        public int strengthMin
        {
            get { return _strengthMin; }
            set { SetField(ref _strengthMin, value); }
        }
        private int _strengthMax = 0;
        [XmlElement]
        public int strengthMax
        {
            get { return _strengthMax; }
            set { SetField(ref _strengthMax, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    [Serializable]
    public class WeaponConfig
    {
        public WeaponConfig()
        {
            triggerConfig.PropertyChanged += (ass, asd) =>
             { saveConfig(); };
            rcsConfig.PropertyChanged += delegate { saveConfig(); };
        }
        [XmlElement]
        public TriggerConfig triggerConfig { get; set; } = new TriggerConfig();
        [XmlElement]
        public RCSConfig rcsConfig { get; set; } = new RCSConfig();
        void saveConfig()
        {
            Properties.Settings.Default.Save();
        }
    }
    
    public class RangeSliderMultiValueConvertor : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0].Equals(values[1]))
                return String.Format("{0}{1}", values[0], parameter);
            return String.Format("{0}{2}-{1}{2}", values[0], values[1], parameter);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            //if (System.Diagnostics.Debugger.IsAttached) Properties.Settings.Default.Reset();
            Properties.Settings.Default.PropertyChanged += delegate { Properties.Settings.Default.Save(); };
            if (Properties.Settings.Default.pistolConfig == null)
                Properties.Settings.Default.pistolConfig = new WeaponConfig();
            if (Properties.Settings.Default.rifleConfig == null)
                Properties.Settings.Default.rifleConfig = new WeaponConfig();
            if (Properties.Settings.Default.smgConfig == null)
                Properties.Settings.Default.smgConfig = new WeaponConfig();
            if (Properties.Settings.Default.heavyConfig == null)
                Properties.Settings.Default.heavyConfig = new WeaponConfig();
            if (Properties.Settings.Default.sniperConfig == null)
                Properties.Settings.Default.sniperConfig = new WeaponConfig();
            if (Properties.Settings.Default.shotgunConfig == null)
                Properties.Settings.Default.shotgunConfig = new WeaponConfig();
            NHotkey.Wpf.HotkeyManager.Current.AddOrReplace("asd", Key.NumPad9, ModifierKeys.None, delegate { Properties.Settings.Default.pistolConfig.triggerConfig.enabled = !Properties.Settings.Default.pistolConfig.triggerConfig.enabled; });
            Thread thread1 = new Thread(UpdateBase);
            Thread thread2 = new Thread(UpdateBhop);
            Thread thread3 = new Thread(UpdateRcs);
            Thread thread4 = new Thread(UpdateKeyUtils);
            Thread thread5 = new Thread(UpdateAutoPistol);
            Thread thread6 = new Thread(UpdateAimAssist);
            Thread thread7 = new Thread(UpdateSkinChanger);
            while ((Core.HWnd = WinAPI.FindWindowByCaption(Core.HWnd, Core.GameTitle)) == IntPtr.Zero)
                Thread.Sleep(250);
            Core.Attach(System.Diagnostics.Process.GetProcessesByName("csgo")[0]);
            InitializeComponent();
            if (Properties.Settings.Default.darkMode)
                ThemeManager.ChangeAppStyle(Application.Current,
                                    ThemeManager.GetAccent("Blue"),
                                    ThemeManager.GetAppTheme("BaseDark"));
            SourceInitialized += Window_SourceInitialized;

            while (true)
            {
                using (Core.Renderer.OpenScene())
                {
                    Core.Renderer.Graphics.DrawCircle(200, 200, 100, 2, Core.Renderer.Graphics.CreateBrush(0x7FFF0000));
                }
            }

            StartThreads(thread1, thread2, thread3, thread4, thread5, thread6, thread7);
        }
        private static void UpdateBase()
        {
            while (true)
            {
                Core.Objects.Update();
                Core.TriggerBot.Update();
                Core.SoundEsp.Update();
                Core.Radar.Update();
                Core.Glow.Update();
                using (Core.Renderer.OpenScene())
                {
                    Core.Box.Update();
                    Core.HeadHelper.Update();
                }
                Thread.Sleep(1);
            }
        }
        private static void UpdateSkinChanger()
        {
            while (true)
            {
                Core.SkinChanger.Update();
                Thread.Sleep(1);
            }
        }
        private static void UpdateAimAssist()
        {
            while (true)
            {
                Core.AimAssist.Update();
                Thread.Sleep(1);
            }
        }
        private static void StartThreads(params Thread[] threads)
        {
            foreach (var thread in threads)
            {
                thread.Priority = ThreadPriority.Highest;
                thread.IsBackground = true;
                thread.Start();
            }
        }
        private static void UpdateBhop()
        {
            while (true)
            {
                Core.BunnyJump.Update();
                Thread.Sleep(5);
            }
        }
        private static void UpdateRcs()
        {
            while (true)
            {
                Core.ControlRecoil.Update();
                Thread.Sleep(1);
            }
        }
        private static void UpdateKeyUtils()
        {
            while (true)
            {
                Core.KeyUtils.Update();
                Core.NoFlash.Update();
                Thread.Sleep(10);
            }
        }
        private static void UpdateAutoPistol()
        {
            while (true)
            {
                Core.AutoPistol.Update();
                Thread.Sleep(1);
            }
        }
        private void ToggleSwitch_OnIsCheckedChanged(object sender, EventArgs e)
        {
            ThemeManager.ChangeAppStyle(Application.Current,
                ThemeManager.GetAccent("Blue"),
                Properties.Settings.Default.darkMode
                    ? ThemeManager.GetAppTheme("BaseDark")
                    : ThemeManager.GetAppTheme("BaseLight"));
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        #region resizable
        private double _aspectRatio;
        private bool? _adjustingHeight; internal enum SWP
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
        internal static extern bool GetCursorPos(ref Win32Point pt);[StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        }; public static Point GetMousePosition() // mouse position relative to screen
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }
        private void Window_SourceInitialized(object sender, EventArgs ea)
        {
            HwndSource hwndSource = (HwndSource)HwndSource.FromVisual((Window)sender);
            hwndSource.AddHook(DragHook); _aspectRatio = Width / Height;
        }
        private IntPtr DragHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((WM)msg)
            {
                case WM.WINDOWPOSCHANGING:
                    {
                        WINDOWPOS pos = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS)); if ((pos.flags & (int)SWP.NOMOVE) != 0)
                            return IntPtr.Zero; Window wnd = (Window)HwndSource.FromHwnd(hwnd).RootVisual;
                        if (wnd == null)
                            return IntPtr.Zero;                        // determine what dimension is changed by detecting the mouse position relative to the 
                                                                       // window bounds. if gripped in the corner, either will work.
                        if (!_adjustingHeight.HasValue)
                        {
                            Point p = GetMousePosition(); double diffWidth = Math.Min(Math.Abs(p.X - pos.x), Math.Abs(p.X - pos.x - pos.cx));
                            double diffHeight = Math.Min(Math.Abs(p.Y - pos.y), Math.Abs(p.Y - pos.y - pos.cy)); _adjustingHeight = diffHeight > diffWidth;
                        }
                        if (_adjustingHeight.Value)
                            pos.cy = (int)(pos.cx / _aspectRatio); // adjusting height to width change
                        else
                            pos.cx = (int)(pos.cy * _aspectRatio); // adjusting width to heigth change                        Marshal.StructureToPtr(pos, lParam, true);
                        handled = true;
                    }
                    break;
                case WM.EXITSIZEMOVE:
                    _adjustingHeight = null; // reset adjustment dimension and detect again next time window is resized
                    break;
            }
            return IntPtr.Zero;
        }
        #endregion
    }
}
