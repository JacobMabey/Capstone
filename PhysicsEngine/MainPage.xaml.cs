using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using PhysicsEngine.UI_Menus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
