using System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace PhysicsEngine.UI_Menus
{
    class ThumbSlider : Canvas
    {
        public event EventHandler ValueChanged;
        private bool IsThumbBeingDragged { get; set; } = false;
        public Line SliderLine { get; set; }
        public Ellipse Thumb {  get; set; }

        private double thumbRadius;
        public double ThumbRadius
        {
            get => thumbRadius;
            set
            {
                thumbRadius = value;
                Thumb.Width = thumbRadius * 2.0;
                Thumb.Height = thumbRadius * 2.0;
            }
        }

        private bool isEnabled = true;
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                Thumb.Opacity = isEnabled ? 1 : 0.5;
                SliderLine.Opacity = isEnabled ? 1 : 0.65;
            }
        }

        public double Minimum { get; set; } = 0;
        public double Maximum { get; set; } = 10;

        private double val = 0;
        public virtual double Value
        {
            get => val;
            set
            {
                val = value;
                Canvas.SetLeft(Thumb, (double)(val - Minimum) / (double)(Maximum - Minimum) * Width - Thumb.Width / 2.0);
                if (ValueChanged != null)
                    ValueChanged(this, EventArgs.Empty);
            }
        }

        public ThumbSlider(double width, double thumbRadius) : base()
        {
            ValueChanged = new EventHandler((s, e) =>
            {

            });
            Width = width;
            Height = 20.0;
            SliderLine = new Line();
            SliderLine.X1 = 0;
            SliderLine.Y1 = - 3;
            SliderLine.X2 = Width;
            SliderLine.Y2 = - 3;
            SliderLine.StrokeThickness = 2;
            SliderLine.Stroke = new SolidColorBrush(Colors.White);

            Thumb = new Ellipse();
            Thumb.PointerEntered += (s, o) =>
            {
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);
            };
            Thumb.PointerExited += (s, o) =>
            {
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
            };
            ThumbRadius = thumbRadius;
            Canvas.SetTop(Thumb, -thumbRadius - 3);
            Thumb.PointerPressed += Thumb_PointerPressed;
            Thumb.PointerMoved += Thumb_PointerMoved;
            Thumb.PointerReleased += Thumb_PointerReleased;
            Thumb.Fill = new SolidColorBrush(Colors.White);

            Children.Add(SliderLine);
            Children.Add(Thumb);
        }


        private void Thumb_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!IsEnabled) return;

            IsThumbBeingDragged = true;
            Thumb.CapturePointer(e.Pointer);
        }
        private void Thumb_PointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (IsThumbBeingDragged)
            {
                double newPos = e.GetCurrentPoint(SliderLine).Position.X;
                if (newPos < 0.0) newPos = 0.0;
                if (newPos > Width)
                    newPos = Width;
                Canvas.SetLeft(Thumb, newPos - Thumb.Width / 2.0);

                Value = newPos / Width * (double)(Maximum - Minimum) + Minimum;
            }
        }
        private void Thumb_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            IsThumbBeingDragged = false;
            Thumb.ReleasePointerCapture(e.Pointer);
        }

    }
}