using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

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
                    Scene.MainScene.Background = new SolidColorBrush(value);
                bgColor = value;
                Scene.WorldMenu.BgColorPicker.SetColor(bgColor);

                //Set circle border color to black or white depending on intensity of bg color
                //Recieved the following math from https://stackoverflow.com/questions/3942878/how-to-decide-font-color-in-white-or-black-depending-on-background-color
                if (bgColor.R * 0.299 + bgColor.G * 0.587 + bgColor.B * 0.114 > 150)
                    Scene.SetCircleBorderColor(Colors.Black);
                else
                    Scene.SetCircleBorderColor(Colors.White);
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
