using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace PhysicsEngine
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //Main Global Canvas
        public static Canvas MainScene { get; private set; }



        int x = 50;
        CompRectangle rect;
        Particle p;
        ParticleEjector ejector;

        public MainPage()
        {
            Initialize(Colors.White); //Main Initial Canvas Initialization


            rect = new CompRectangle(new Coord(300, 50), new Size(50, 80));
            MainScene.Children.Add(rect.GetUIElement());

            p = new Particle(new Coord(500, 500), 25);
            MainScene.Children.Add(p.GetUIElement());

            ejector = new ParticleEjector(new Coord(50, 200), 0.0, 10, 3);
            MainScene.Children.Add(ejector.GetUIElement());
        }


        private void Loop(object sender, object e) 
        {
            Update(); //Main Canvas Update To be within Loop

        }



        private void Initialize(Color bgColor)
        {
            //Add Initial Windows/Canvases
            this.InitializeComponent();

            MainScene = new Canvas();
            MainScene.Width = MainCanvas.Width;
            MainScene.Height = MainCanvas.Height;
            MainCanvas.Children.Add(MainScene);
            //

            //Initialize Systems
            CompositionTarget.Rendering += Loop;

            Timer.Initialize();
            Renderer.Initialize(bgColor);
            //
        }


        private void Update()
        {
            //Update Systems
            Timer.Update();
            Renderer.Update();
            //

            //Update all Children
            foreach (UIElement element in MainScene.Children)
            {
                if (element is Shape && ((Shape)element).Tag is Component)
                {
                    ((Component)((Shape)element).Tag).Update();
                }
            }
        }
    }
}
