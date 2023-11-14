﻿using Microsoft.Graphics.Canvas;
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


            particleCounter = new TextBlock();
            Canvas.SetLeft(particleCounter, 500);
            Canvas.SetTop(particleCounter, 10);
            Canvas.SetZIndex(particleCounter, 100);
            Scene.MainScene.Children.Add(particleCounter);


            rect = new CompRectangle(new Coord(220, 100), new Size(50, 80));
            rect.RotationAngle = 45;
            Scene.Add(rect);

            //p1 = new Particle(new Coord(300, 15), 25);
            //p1.Phys.Elasticity = 0.9;
            //p1.Phys.Friction = 0;
            //p1.Phys.ApplyForce(new Coord(-1, 1));
            //MainScene.Children.Add(p1.GetUIElement());

            //p2 = new Particle(new Coord(100, 200), 25);
            //p2.Phys.Elasticity = 0.9;
            //p2.Phys.Friction = 0;
            //p2.Phys.Mass = 5.0;
            //p2.Phys.ApplyForce(new Coord(50, -50));
            //MainScene.Children.Add(p2.GetUIElement());

            l = new CompLine(new Coord(10, 300), new Coord(310, 300));
            //Scene.Add(l);

            l2 = new CompLine(new Coord(10, 10), new Coord(10, 300));
            //Scene.Add(l2);

            l3 = new CompLine(new Coord(310, 10), new Coord(310, 300));
            //Scene.Add(l3);

            l4 = new CompLine(new Coord(10, 10), new Coord(310, 10));
            //Scene.Add(l4);

            ejector = new ParticleEjector(new Coord(500, 200), 200.0, 1000, 6, 80);
            ejector.ParticleElasticity = 0.6;
            ejector.ParticleRadius = 15;
            ejector.ParticleRadiusRange = 0;
            ejector.ParticleFriction = 0.0;
            ejector.ParticleScatterAngle = 0.0;
            ejector.ParticleColor = Colors.Blue;
            ejector.ColorChangeRate = 1.5;
            ejector.ParticleColorChangeRate = 0;
            Scene.Add(ejector);

            ejector2 = new ParticleEjector(new Coord(400, 50), -45.0, 1000, 5, 0.6);
            ejector2.ParticleColor = Colors.AliceBlue;
            ejector2.ParticleRadius = 10;
            //Scene.Add(ejector2);

            Timer.TimeScale = 1.0;

            //Scene.ToggleBorderCollision();
            //Scene.SetCircleBorderRadius(200);
            //Scene.ToggleCircleBorderCollision();
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
