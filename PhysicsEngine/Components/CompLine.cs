using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using Windows.UI.Xaml.Input;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas;
using Windows.Devices.Radios;

namespace PhysicsEngine
{
    public class CompLine : Component
    {
        private Line _line = new Line();

        private bool PosABeingDragged { get; set; } = false;
        public bool FullLineBeingDragged { get; set; } = false;

        private Coord posA;
        public Coord PosA
        {
            get => posA;
            set
            {
                posA = value;
                _line.X1 = posA.X;
                _line.Y1 = posA.Y;
            }
        }

        private Coord posB;
        public Coord PosB
        {
            get => posB;
            set
            {
                posB = value;
                _line.X2 = posB.X;
                _line.Y2 = posB.Y;
            }
        }

        private double thickness;
        public double Thickness
        {
            get => thickness;
            set
            {
                thickness = value;
                _line.StrokeThickness = thickness;
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
                if (_line.Stroke != FillBrush)
                    _line.Stroke = FillBrush;
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            _line.Tag = this;
            _line.PointerPressed += Line_PointerPressed;
            _line.PointerReleased += Line_PointerReleased;
            _line.PointerMoved += Line_PointerMoved;
            _line.Tapped += Line_Tapped;
            _line.StrokeStartLineCap = PenLineCap.Round;
            _line.StrokeEndLineCap = PenLineCap.Round;
        }

        public CompLine()
        {
            Initialize();
            PosA = new Coord(0, 0);
            PosB = new Coord(50, 50);
            Fill = Colors.Red;
            Thickness = 3.0;
        }

        public CompLine(Coord posA, Coord posB)
        {
            Initialize();
            PosA = posA;
            PosB = posB;
            Fill = Colors.Red;
            Thickness = 5.0;
        }

        public CompLine(Coord posA, Coord posB, Color fill, double thickness = 5.0)
        {
            Initialize();
            PosA = posA;
            PosB = posB;
            Fill = fill;
            Thickness = thickness;
            
        }


        private void Line_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            FullLineBeingDragged = false;
            IsBeingDragged = true;
            _line.CapturePointer(e.Pointer);

            //Find whether pointA or pointB is closer to the mouse
            Point pointerCoord = e.GetCurrentPoint(Scene.MainScene).Position;
            double pointADistance = Math.Sqrt(Math.Pow(Math.Abs(pointerCoord.X - PosA.X), 2) + Math.Pow(Math.Abs(pointerCoord.Y - PosA.Y), 2));
            double pointBDistance = Math.Sqrt(Math.Pow(Math.Abs(pointerCoord.X - PosB.X), 2) + Math.Pow(Math.Abs(pointerCoord.Y - PosB.Y), 2));
            PosABeingDragged = pointADistance < pointBDistance;

            double toMidPointDistance = Physics.GetDistance(Coord.FromPoint(pointerCoord), GetMidPoint());
            FullLineBeingDragged = (PosABeingDragged && toMidPointDistance < pointADistance) || (!PosABeingDragged && toMidPointDistance < pointBDistance);

            //Drag mode on if user hold control
            if (Window.Current.CoreWindow.GetKeyState(VirtualKey.Control) == CoreVirtualKeyStates.Down)
            {
                IsMouseDragMode = true;
                _line.Opacity = 0.6;
            }
        }
        private void Line_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IsBeingDragged = false;
            IsBeingAdded = false;
            _line.ReleasePointerCapture(e.Pointer);
            IsMouseDragMode = false;
            _line.Opacity = 1.0;
        }
        private void Line_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!IsBeingDragged) return;

            //Drag mode on if user hold control
            if (!IsMouseRotateMode && Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down))
            {
                IsMouseDragMode = true;
                _line.Opacity = 0.6;
            }
            else
            {
                IsMouseDragMode = false;
                _line.Opacity = 1.0;
            }

            Point pointerCoord = e.GetCurrentPoint(Scene.MainScene).Position;

            double posx = pointerCoord.X;
            double posy = pointerCoord.Y;
            if (Scene.IsSnappableGridEnabled && IsMouseDragMode)
            {
                posx = Math.Round(posx / Scene.SnapCellSize) * Scene.SnapCellSize;
                posy = Math.Round(posy / Scene.SnapCellSize) * Scene.SnapCellSize;
            }

            if (FullLineBeingDragged)
            {
                Coord midPoint = GetMidPoint();
                double pointerToCenter = Physics.GetDistance(new Coord(posx, posy), midPoint);
                double pointerToCenterAngle = Physics.GetAngle(midPoint, new Coord(posx, posy));

                PosA = Physics.MovePoint(PosA, pointerToCenter, pointerToCenterAngle);
                PosB = Physics.MovePoint(PosB, pointerToCenter, pointerToCenterAngle);
            } else
            {
                if (PosABeingDragged)
                {
                    PosA = new Coord(posx, posy);
                }
                else
                {
                    PosB = new Coord(posx, posy);
                }
            }
        }
        private void Line_Tapped(object sender, TappedRoutedEventArgs e)
        {
            IsMouseDragMode = false;
            IsBeingAdded = false;
            IsBeingDragged = false;
            _line.Opacity = 1.0;
            _line.ReleasePointerCaptures();

            OpenCompMenu();
        }


        public override Shape GetUIElement() => _line;

        public override Component Clone()
        {
            CompLine clone = new CompLine();
            clone.IsCollisionEnabled = IsCollisionEnabled;
            clone.PosA = PosA;
            clone.PosB = PosB;
            clone.Fill = Fill;
            clone.Thickness = Thickness;

            return clone;
        }

        public override string GetSaveText()
        {
            string output = "comp_line\n";

            output += "IsCollisionEnabled:" + IsCollisionEnabled + "\n";
            output += "PosA:" + PosA.X + "," + PosA.Y + "\n";
            output += "PosB:" + PosB.X + "," + PosB.Y + "\n";
            output += "Fill:" + Fill.A + "," + Fill.R + "," + Fill.G + "," + Fill.B + "\n";
            output += "Thickness:" + Thickness + "\n";

            output += "-\n";
            return output;
        }

        public override void Update()
        {
            base.Update();

            if (IsBeingDragged) return;

        }


        public Coord GetMidPoint()
        {
            return new Coord((posA.X + posB.X) / 2.0, (posA.Y + posB.Y) / 2.0);
        }
    }
}
