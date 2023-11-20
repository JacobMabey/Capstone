using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace PhysicsEngine
{
    public static class Renderer
    {
        private static Color bgColor;
        public static Color BackgroundColor
        {
            get => bgColor;
            set
            {
                if (bgColor != value)
                    Scene.MainScene.Background = new SolidColorBrush(bgColor);
                bgColor = value;
                Scene.WorldMenu.BgColorPicker.SetColor(bgColor);
            }
        }
        private static TextBlock fpsText { get; set; }
        private static TextBlock particleCountText { get; set; }

        public static void Initialize(Color bgColor)
        {
            if (bgColor != null)
                BackgroundColor = bgColor;

#if DEBUG
            fpsText = new TextBlock();
            Canvas.SetLeft(fpsText, 10);
            Canvas.SetTop(fpsText, 10);
            Canvas.SetZIndex(fpsText, 100);
            Scene.MainScene.Children.Add(fpsText);

            particleCountText = new TextBlock();
            Canvas.SetLeft(particleCountText, 10);
            Canvas.SetTop(particleCountText, 30);
            Canvas.SetZIndex(particleCountText, 100);
            Scene.MainScene.Children.Add(particleCountText);
#endif
        }

        public static void Update()
        {
            //DebugRender Here
#if DEBUG
            fpsText.Text = "FPS: "+Timer.FPS;
            particleCountText.Text = "P#: " + Scene.ParticleCount;
#endif
        }

        public static void Shutdown()
        {

        }
    }
}
