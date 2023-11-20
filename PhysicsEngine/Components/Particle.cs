using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace PhysicsEngine
{
    public class Particle : Component
    {
        private Ellipse _ellipse = new Ellipse();

        private Coord pos;
        public override Coord Position
        {
            get => pos;
            set
            {
                pos = value;
                Canvas.SetLeft(_ellipse, pos.X - radius);
                Canvas.SetTop(_ellipse, pos.Y - radius);
            }
        }

        private double radius;
        public double Radius
        {
            get => radius;
            set
            {
                radius = value;
                _ellipse.Width = radius * 2.0;
                _ellipse.Height = radius * 2.0;
            }
        }

        private Color fill;
        public override Color Fill
        {
            get => fill;
            set
            {
                fill = value;
                if (FillBrush == null)
                    FillBrush = new SolidColorBrush();
                FillBrush.Color = fill;
                if (_ellipse.Fill != FillBrush)
                    _ellipse.Fill = FillBrush;

                if (Scene.CompMenu.IsMenuExpanded && Scene.CompMenu.ParentComponent.ID == this.ID)
                {
                    Scene.CompMenu.ColorPicker.SetColor(fill);
                }
            }
        }

        public double ColorChangeRate { get; set; } = 0.0;

        public bool IsParticleCollisionEnabled { get; set; } = true;


        public override void Initialize()
        {
            base.Initialize();

            _ellipse.Tag = this;
            _ellipse.Tapped += Ellipse_Tapped;
            _ellipse.PointerPressed += Ellipse_PointerPressed;
            _ellipse.PointerReleased += Ellipse_PointerReleased;
            _ellipse.PointerMoved += Ellipse_PointerMoved;
        }

        public Particle()
        {
            Initialize();
            Position = new Coord(0, 0);
            Radius = 5.0;
            Fill = Colors.Red;
        }
        public Particle(Coord position, double radius = 5.0)
        {
            Initialize();
            Position = position;
            Radius = radius;
            Fill = Colors.Red;
        }


        private void Ellipse_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            IsBeingDragged = true;
            _ellipse.CapturePointer(e.Pointer);

            //Drag mode on if user hold control
            if (Window.Current.CoreWindow.GetKeyState(VirtualKey.Control) == CoreVirtualKeyStates.Down)
            {
                IsMouseDragMode = true;
                _ellipse.Opacity = 0.6;
            }
        }
        private void Ellipse_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (IsBeingAdded)
                OpenCompMenu();

            IsBeingDragged = false;
            IsBeingAdded = false;
            _ellipse.ReleasePointerCapture(e.Pointer);
            IsMouseDragMode = false;
            _ellipse.Opacity = 1.0;
        }
        private void Ellipse_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!IsBeingDragged) return;

            Point pointerCoord = e.GetCurrentPoint(Scene.MainScene).Position;

            //Drag mode on if user hold control
            if (Window.Current.CoreWindow.GetKeyState(VirtualKey.Control) == CoreVirtualKeyStates.Down)
            {
                IsMouseDragMode = true;
                _ellipse.Opacity = 0.6;
            }
            else
            {
                IsMouseDragMode = false;
                _ellipse.Opacity = 1.0;
            }

            double posx = pointerCoord.X - PointerDragPoint.X;
            double posy = pointerCoord.Y - PointerDragPoint.Y;
            if (Scene.IsSnappableGridEnabled && IsMouseDragMode)
            {
                posx = Math.Round(posx / Scene.SnapCellSize) * Scene.SnapCellSize;
                posy = Math.Round(posy / Scene.SnapCellSize) * Scene.SnapCellSize;
            }
            Position = new Coord(posx, posy);
        }
        private void Ellipse_Tapped(object sender, TappedRoutedEventArgs e)
        {
            IsMouseDragMode = false;
            IsBeingAdded = false;
            IsBeingDragged = false;
            _ellipse.Opacity = 1.0;
            _ellipse.ReleasePointerCaptures();

            OpenCompMenu();
        }


        public override Shape GetUIElement() => _ellipse;

        public override Component Clone()
        {
            Particle clone = new Particle();
            clone.IsCollisionEnabled = IsCollisionEnabled;
            clone.IsParticleCollisionEnabled = IsParticleCollisionEnabled;
            clone.Fill = Fill;
            clone.ColorChangeRate = ColorChangeRate;
            clone.HasPhysics = HasPhysics;
            clone.Radius = Radius;
            clone.Position = Position;
            clone.Phys = Phys.Clone(clone);

            return clone;
        }

        public override void Update()
        {
            base.Update();

            if (ColorChangeRate != 0.0)
            {
                double[] HsvParticleColor = ColorFunctions.RgbToHsv(Fill.R, Fill.G, Fill.B);
                HsvParticleColor[0] = (HsvParticleColor[0] + ColorChangeRate * Timer.TimeScale) % 360.0;
                double[] newColor = ColorFunctions.HsvToRgb(HsvParticleColor[0], HsvParticleColor[1], HsvParticleColor[2]);
                Fill = Color.FromArgb(255, (byte)newColor[0], (byte)newColor[1], (byte)newColor[2]);
            }
        }

    }
}
