﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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

        private void Initialize()
        {
            _rect.Tag = this;
            _rect.PointerPressed += Rect_PointerPressed;
            _rect.PointerReleased += Rect_PointerReleased;
            _rect.PointerMoved += Rect_PointerMoved;
            RotationTransform = new RotateTransform();
            _rect.RenderTransform = RotationTransform;
        }

        public CompRectangle()
        {
            Initialize();
            Position = new Coord(0,0);
            Size = new Size(0,0);
            FillColor = Colors.LightGray;
            StrokeColor = Colors.Black;
            _rect.StrokeThickness = 1.0;
        }
        public CompRectangle(Coord position, Size size)
        {
            Initialize();
            Position = position;
            Size = size;
            FillColor = Colors.LightGray;
            StrokeColor = Colors.Black;
            _rect.StrokeThickness = 1.0;

        }
        public CompRectangle(Coord position, Size size, Color fill, Color stroke, double strokeThickness)
        {
            Initialize();
            Position = position;
            Size = size;
            FillColor = fill;
            StrokeColor = stroke;
            _rect.StrokeThickness = strokeThickness;

        }


        private void Rect_PointerPressed( object sender, PointerRoutedEventArgs e)
        {
            IsBeingDragged = true;
            _rect.Opacity = 0.6;
            _rect.CapturePointer(e.Pointer);

            //Get position of pointer relative to shape movement center for smoother pickups
            Point pointerCoord = e.GetCurrentPoint(MainPage.MainScene).Position;
            PointerDragPoint = new Coord(pointerCoord.X - Position.X, pointerCoord.Y - Position.Y);
        }
        private void Rect_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            IsBeingDragged = false;
            _rect.Opacity = 1.0;
            _rect.ReleasePointerCapture(e.Pointer);
        }
        private void Rect_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!IsBeingDragged) return;

            Point pointerCoord = e.GetCurrentPoint(MainPage.MainScene).Position;
            Position = new Coord(pointerCoord.X - PointerDragPoint.X, pointerCoord.Y - PointerDragPoint.Y);
        }



        public override Shape GetUIElement() => _rect;

        public override void Update()
        {
            base.Update();

            if (IsBeingDragged) return;
        }

    }
}
