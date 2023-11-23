using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace PhysicsEngine
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static FontFamily GlobalFont = new FontFamily("Cascadia Code");

        private static Size windowSize;
        public static Size WindowSize
        {
            get => windowSize;
            set
            {
                windowSize = value;
                Scene.MainScene.Width = windowSize.Width;
                Scene.MainScene.Height = windowSize.Height;
                if (Scene.Toolbar != null)
                    Scene.Toolbar.ResetPosition();
                if (Scene.AddMenu != null)
                    Scene.AddMenu.ResetPosition();

                if (Scene.WorldMenu != null)
                    Scene.WorldMenu.ResetPosition();
                if (Scene.AddMenu != null)
                    Scene.AddMenu.ResetPosition();
                Scene.ResetBorderPositions();
            }
        }

        CompRectangle rect;
        Particle p1;
        Particle p2;
        CompLine l;
        CompLine l2;
        CompLine l3;
        CompLine l4;
        ParticleEjector ejector;
        ParticleEjector ejector2;

        TextBlock particleCounter;

        public MainPage()
        {
            Initialize(Color.FromArgb(255, 230, 230, 230)); //Main Initial Canvas Initialization
        }


        private void Loop(object sender, object e)
        {
            Update(); //Main Canvas Update To be within Loop
        }


        private void Initialize(Color bgColor)
        {
            //Add Initial Windows/Canvases
            this.InitializeComponent();

            Scene.Initialize();
            MainCanvas.Children.Add(Scene.MainScene);

            //Initialize Systems
            this.Loaded += PageLoaded;

            Timer.Initialize();
            Renderer.Initialize(bgColor);
            //
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering += Loop;
        }

        private void Update()
        {
            //Update Systems
            Timer.Update();
            Renderer.Update();
            Scene.Update();
            //
        }



    }
}
