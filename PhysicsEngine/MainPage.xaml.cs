using System;
using System.Collections.Generic;
using System.IO;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

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
        Ellipse circ = new Ellipse();
        CompRectangle rect;
        Particle p;

        public MainPage()
        {
            Initialize(Colors.White); //Main Initial Canvas Initialization


            circ.Height = 100;
            circ.Width = 100;
            SolidColorBrush red = new SolidColorBrush();
            red.Color = Colors.Red;
            circ.Fill = red;
            Canvas.SetLeft(circ, x);
            Canvas.SetTop(circ, 200);
            MainScene.Children.Add(circ);

            rect = new CompRectangle(new Point(300, 50), new Point(50, 80));
            rect.RotationCenter = new Point(100, 100);
            MainScene.Children.Add(rect.UIRect);

            //p = new Particle(new Point(500, 500), 10);
            //MainScene.Children.Add(p.UIEllipse);
        }

        private void Loop(object sender, object e) 
        {
            Update(); //Main Canvas Update To be within Loop


            double left = Canvas.GetLeft(circ);
            Canvas.SetLeft(circ, left + 1 * Timer.DeltaTime);


            rect.RotationAngle = (rect.RotationAngle + 1 * Timer.DeltaTime) % 360.0;
            if (rect.RotationAngle > 180)
                rect.Fill = Colors.Red;
            else
                rect.Fill = Colors.Blue;

            if (left > 1200)
                Renderer.SetBgColor(Colors.Gray);
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
        }
    }
}
