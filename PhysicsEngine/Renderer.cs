using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace PhysicsEngine
{
    public static class Renderer
    {
        private static TextBlock fpsText { get; set; }

        public static void Initialize(Color bgColor)
        {
            if (bgColor != null)
                SetBgColor(bgColor);

#if DEBUG
            fpsText = new TextBlock();
            Canvas.SetLeft(fpsText, 10);
            Canvas.SetTop(fpsText, 10);
            Canvas.SetZIndex(fpsText, 100);
            MainPage.MainScene.Children.Add(fpsText);
#endif
        }

        public static void Update()
        {
            //DebugRender Here
#if DEBUG
            fpsText.Text = "FPS: "+Timer.FPS;
#endif
        }

        public static void Shutdown()
        {

        }


        public static void SetBgColor(Color bg)
        {
            MainPage.MainScene.Background = new SolidColorBrush(bg);
        }
    }
}
