using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace PhysicsEngine
{
    public class CompRectangle : Component
    {
        private Rectangle _rect = new Rectangle();
        private RotateTransform RotationTransform { get; set; }

        private Coord pos;
        public Coord Position
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
        public Color FillColor
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
            }
        }


        public CompRectangle()
        {
            _rect.Tag = this;
            RotationTransform = new RotateTransform();
            _rect.RenderTransform = RotationTransform;
            _rect.Stroke = StrokeBrush;
            Position = new Coord(0,0);
            Size = new Size(0,0);
            FillColor = Colors.LightGray;
            StrokeColor = Colors.Black;
            _rect.StrokeThickness = 1.0;
        }
        public CompRectangle(Coord position, Size size)
        {
            _rect.Tag = this;
            RotationTransform = new RotateTransform();
            _rect.RenderTransform = RotationTransform;
            Position = position;
            Size = size;
            FillColor = Colors.LightGray;
            StrokeColor = Colors.Black;
            _rect.StrokeThickness = 1.0;

        }
        public CompRectangle(Coord position, Size size, Color fill, Color stroke, double strokeThickness)
        {
            _rect.Tag = this;
            RotationTransform = new RotateTransform();
            _rect.RenderTransform = RotationTransform;
            Position = position;
            Size = size;
            FillColor = fill;
            StrokeColor = stroke;
            _rect.StrokeThickness = strokeThickness;

        }

        public override Shape GetUIElement() => _rect;

        public override void Update()
        {
            base.Update();

        }

    }
}
