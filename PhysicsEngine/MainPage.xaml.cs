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

        public static readonly Thickness OUT_OF_BOUNDS_MARGIN = new Thickness(100);

        //Fills list of indexes for elements to be destroyed
        private static List<int> elementsToBeDestroyed = new List<int>();

        //Main Snappable Grid Global Values
        public static double SnapCellSize { get; set; } = 25.0;
        public static bool IsSnappableGridEnabled { get; set; } = true;


        //Global Random Object
        public static Random Rand = new Random();
        CompRectangle rect;
        Particle p;
        CompLine l;
        CompLine l2;
        CompLine l3;
        ParticleEjector ejector;
        ParticleEjector ejector2;


        CompLine borderTop;
        CompLine borderRight;
        CompLine borderBottom;
        CompLine borderLeft;

        TextBlock particleCounter;

        public MainPage()
        {
            Initialize(Colors.White); //Main Initial Canvas Initialization

            //Add collision border lines
            borderTop = new CompLine(new Coord(0, 0), new Coord(MainCanvas.Width, 0), Colors.Black, 1.0);
            borderRight = new CompLine(new Coord(MainCanvas.Width, 0), new Coord(MainCanvas.Width, MainCanvas.Height), Colors.Black, 1.0);
            borderBottom = new CompLine(new Coord(0, MainCanvas.Height), new Coord(MainCanvas.Width, MainCanvas.Height), Colors.Black, 1.0);
            borderLeft = new CompLine(new Coord(0, 0), new Coord(0, MainCanvas.Height), Colors.Black, 1.0);
            MainScene.Children.Add(borderTop.GetUIElement());
            MainScene.Children.Add(borderRight.GetUIElement());
            MainScene.Children.Add(borderBottom.GetUIElement());
            MainScene.Children.Add(borderLeft.GetUIElement());

            //ToggleBorderCollision();

            particleCounter = new TextBlock();
            Canvas.SetLeft(particleCounter, 500);
            Canvas.SetTop(particleCounter, 10);
            Canvas.SetZIndex(particleCounter, 100);
            MainPage.MainScene.Children.Add(particleCounter);


            rect = new CompRectangle(new Coord(220, 100), new Size(50, 80));
            rect.RotationAngle = 45;
            MainScene.Children.Add(rect.GetUIElement());

            //p = new Particle(new Coord(200, 5), 25);
            //p.Phys.Elasticity = 0.9;
            //MainScene.Children.Add(p.GetUIElement());

            //l = new CompLine(new Coord(10, 300), new Coord(410, 300));
            //MainScene.Children.Add(l.GetUIElement());

            l2 = new CompLine(new Coord(10, 10), new Coord(10, 300));
            MainScene.Children.Add(l2.GetUIElement());

            l3 = new CompLine(new Coord(410, 10), new Coord(410, 300));
            MainScene.Children.Add(l3.GetUIElement());

            ejector = new ParticleEjector(new Coord(350, 50), 225.0, 1000, 6, 5);
            ejector.ParticleElasticity = 0.9;
            ejector.ParticleFriction = 1.0;
            ejector.ParticleScatterAngle = 20.0;
            MainScene.Children.Add(ejector.GetUIElement());

            ejector2 = new ParticleEjector(new Coord(50, 50), 135.0, 1000, 10, 5);
            //MainScene.Children.Add(ejector2.GetUIElement());

            Timer.TimeScale = 1;
        }


        private void Loop(object sender, object e) 
        {
            Update(); //Main Canvas Update To be within Loop

            particleCounter.Text = ""+ejector.ParticlesEjected;
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

            //For testing
            //Window.Current.Content.KeyDown += HandleKeyDown;

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
            //

            if (elementsToBeDestroyed == null)
                elementsToBeDestroyed = new List<int>();
            else elementsToBeDestroyed.Clear();

            //Update all Children
            foreach (UIElement element in MainScene.Children)
            {

                //Update all components
                if (element is Shape && ((Shape)element).Tag is Component)
                {
                    //Check for out of bounds elements
                    double x = Canvas.GetLeft(element);
                    double y = Canvas.GetTop(element);

                    if (x < -OUT_OF_BOUNDS_MARGIN.Left || x > MainScene.Width + OUT_OF_BOUNDS_MARGIN.Right
                        || y < -OUT_OF_BOUNDS_MARGIN.Top || y > MainScene.Height + OUT_OF_BOUNDS_MARGIN.Bottom)
                    {
                        //Mark element to be destroyed
                    //    elementsToBeDestroyed.Add(MainScene.Children.IndexOf(element));
                    //    continue;
                    }

                    //Update Element
                    ((Component)((Shape)element).Tag).Update();
                }
            }

            //Destroy and remove all marked elements
            for (int i = elementsToBeDestroyed.Count - 1; i >= 0; i--)
                MainScene.Children.RemoveAt(elementsToBeDestroyed[i]);
        }



        public void ToggleBorderCollision()
        {
            borderTop.IsCollisionEnabled = !borderTop.IsCollisionEnabled;
            borderRight.IsCollisionEnabled = !borderRight.IsCollisionEnabled;
            borderBottom.IsCollisionEnabled = !borderBottom.IsCollisionEnabled;
            borderLeft.IsCollisionEnabled = !borderLeft.IsCollisionEnabled;
        }
    }
}
