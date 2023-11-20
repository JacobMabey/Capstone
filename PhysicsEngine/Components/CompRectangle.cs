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
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace PhysicsEngine
{
    public class CompRectangle : Component
    {
        private Rectangle _rect = new Rectangle();
        private RotateTransform RotationTransform { get; set; }

        private Coord pos;
        public override Coord Position
        {
            get => pos;
            set
            {
                pos = value;
                Canvas.SetLeft(_rect, pos.X);
                Canvas.SetTop(_rect, pos.Y);
            }
        }

        private Size size;
        public Size Size
        {
            get => size;
            set
            {
                size = value;
                _rect.Width = size.Width;
                _rect.Height = size.Height;
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
                if (_rect.Fill != FillBrush)
                    _rect.Fill = FillBrush;
            }
        }

        private Color stroke;
        public Color StrokeColor
        {
            get => stroke;
            set
            {
                stroke = value;
                if (StrokeBrush == null)
                    StrokeBrush = new SolidColorBrush();
                StrokeBrush.Color = stroke;
                if (_rect.Stroke != StrokeBrush)
                    _rect.Stroke = StrokeBrush;
            }
        }

        private double strokeThickness;
        public double StrokeThickness
        {
            get => strokeThickness;
            set
            {
                strokeThickness = value;
                _rect.StrokeThickness = strokeThickness;
            }
        }


        private Coord rotationCenter;
        public Coord RotationCenter
        {
            get => rotationCenter;
            set
            {
                rotationCenter = value;
                RotationTransform.CenterX = rotationCenter.X;
                RotationTransform.CenterY = rotationCenter.Y;
            }
        }
        private double rotationAngle;
        public double RotationAngle
        {
            get => rotationAngle;
            set
            {
                rotationAngle = value;
                RotationTransform.Angle = rotationAngle;

                if (Scene.CompMenu.IsMenuExpanded && Scene.CompMenu.ParentComponent.ID == this.ID)
                {
                    double roundedAngle = rotationAngle;
                    while (roundedAngle < 0) roundedAngle += 360;
                    while (roundedAngle > 359) roundedAngle -= 359;
                    Scene.CompMenu.RotateInput.Text = ((int)(roundedAngle * 1000.0) / 1000.0) + "";
                }
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            _rect.Tag = this;
            _rect.PointerPressed += Rect_PointerPressed;
            _rect.PointerReleased += Rect_PointerReleased;
            _rect.PointerMoved += Rect_PointerMoved;
            _rect.Tapped += Rect_Tapped;
            RotationTransform = new RotateTransform();
            _rect.RenderTransform = RotationTransform;
        }

        public CompRectangle()
        {
            Initialize();
            Position = new Coord(0,0);
            Size = new Size(0,0);
            Fill = Colors.LightGray;
            StrokeColor = Colors.Black;
            _rect.StrokeThickness = 1.0;
        }
        public CompRectangle(Coord position, Size size)
        {
            Initialize();
            Position = position;
            Size = size;
            Fill = Colors.LightGray;
            StrokeColor = Colors.Black;
            _rect.StrokeThickness = 1.0;

        }
        public CompRectangle(Coord position, Size size, Color fill, Color stroke, double strokeThickness)
        {
            Initialize();
            Position = position;
            Size = size;
            Fill = fill;
            StrokeColor = stroke;
            _rect.StrokeThickness = strokeThickness;

        }


        private void Rect_PointerPressed( object sender, PointerRoutedEventArgs e)
        {
            IsBeingDragged = true;
            _rect.CapturePointer(e.Pointer);
            
            //Get position of pointer relative to shape movement center for smoother pickups
            Point pointerCoord = e.GetCurrentPoint(Scene.MainScene).Position;
            PointerDragPoint = new Coord(pointerCoord.X - Position.X, pointerCoord.Y - Position.Y);

            IsMouseDragMode = true;
            _rect.Opacity = 0.6;
            //Rotate mode on if user hold shift
            if (Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down))
            {
                IsMouseRotateMode = true;
            }
        }
        private void Rect_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (IsBeingAdded)
                OpenCompMenu();

            IsBeingDragged = false;
            IsBeingAdded = false;
            _rect.ReleasePointerCapture(e.Pointer);
            IsMouseDragMode = false;
            _rect.Opacity = 1.0;
        }
        private void Rect_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!IsBeingDragged) return;

            //Rotate mode on if user hold shift
            if (Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down))
            {
                IsMouseRotateMode = true;
            }
            else
            {
                IsMouseRotateMode = false;
            }

            Point pointerCoord = e.GetCurrentPoint(Scene.MainScene).Position;

            //If rotation mode is active, only rotate
            if (IsMouseRotateMode)
            {
                RotationAngle = -Physics.GetAngle(new Coord(Position.X, Position.Y), Coord.FromPoint(pointerCoord)) * 180.0 / Math.PI - 90.0;
                return;
            }

            double posx = pointerCoord.X - PointerDragPoint.X;
            double posy = pointerCoord.Y - PointerDragPoint.Y;
            if (Scene.IsSnappableGridEnabled && Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down))
            {
                posx = Math.Round(posx / Scene.SnapCellSize) * Scene.SnapCellSize;
                posy = Math.Round(posy / Scene.SnapCellSize) * Scene.SnapCellSize;
            }
            Position = new Coord(posx, posy);
        }
        private void Rect_Tapped(object sender, TappedRoutedEventArgs e)
        {
            IsMouseDragMode = false;
            IsBeingAdded = false;
            IsBeingDragged = false;
            _rect.Opacity = 1.0;
            _rect.ReleasePointerCaptures();

            OpenCompMenu();
        }


        public override Shape GetUIElement() => _rect;

        public override Component Clone()
        {
            CompRectangle clone = new CompRectangle();
            clone.IsCollisionEnabled = IsCollisionEnabled;
            clone.Position = Position;
            clone.Size = Size;
            clone.Fill = Fill;
            clone.RotationAngle = RotationAngle;
            clone.RotationCenter = RotationCenter;
            clone.StrokeColor = StrokeColor;
            clone.StrokeThickness = StrokeThickness;
            return clone;
        }

        public override void Update()
        {
            base.Update();

            if (IsBeingDragged) return;
        }

    }
}
