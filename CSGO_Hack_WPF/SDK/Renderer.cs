using Overlay.NET.Directx;
using Process.NET;
using Process.NET.Memory;
using System;
using System.Windows;

namespace CSGO_Hack_WPF.SDK
{
    public class Renderer
    {
        public DirectXOverlayWindow Overlay;
        public Direct2DRenderer Graphics => Overlay.Graphics;

        public Renderer(System.Diagnostics.Process process)
        {
            var _processSharp = new ProcessSharp(process, MemoryType.Remote);
            Overlay = new DirectXOverlayWindow(_processSharp.WindowFactory.MainWindow.Handle, false);
        }

        public Scene OpenScene() => new Scene();

        public class Scene : IDisposable
        {
            public Scene()
            {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    Core.Renderer.Graphics.BeginScene();
                    Core.Renderer.Graphics.ClearScene();
                });
            }
            public void Dispose()
            {
                Application.Current.Dispatcher.Invoke(delegate { Core.Renderer.Graphics.EndScene(); });
            }
        }
    }
}
