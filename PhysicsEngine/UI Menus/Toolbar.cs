using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace PhysicsEngine.UI_Menus
{
    public class Toolbar : Canvas
    {
        public readonly double ToolbarHeight = 30.0;
        private Rectangle PauseButton { get; set; }
        private TextBlock PauseText { get; set; }


        private Slider TimeScaleSlider { get; set; }


        public Toolbar() { Initialize(); }

        private void Initialize()
        {
            ResetPosition();
            Canvas.SetZIndex(this, 100);

            PauseButton = new Rectangle();
            PauseButton.Width = ToolbarHeight;
            PauseButton.Height = ToolbarHeight;
            PauseButton.Fill = new SolidColorBrush(Colors.MediumVioletRed);
            Canvas.SetLeft(PauseButton, 1.0);
            Canvas.SetZIndex(PauseButton, 101);
            PauseButton.PointerPressed += PauseButton_Pressed;
            this.Children.Add(PauseButton);

            PauseText = new TextBlock();
            PauseText.Width = ToolbarHeight;
            PauseText.Height = ToolbarHeight;
            Canvas.SetZIndex(PauseText, 102);
            PauseText.Text = "\u23F8";
            PauseText.PointerPressed += PauseButton_Pressed;
            this.Children.Add(PauseText);

            TimeScaleSlider = new Slider();
            TimeScaleSlider.ValueChanged += TimeScaleSlider_ValueChanged;
            TimeScaleSlider.Minimum = 0.0;
            TimeScaleSlider.Maximum = 1.0;
            TimeScaleSlider.StepFrequency = 0.01;
            TimeScaleSlider.Value = Timer.TimeScale;
            TimeScaleSlider.Width = 200.0;
            Canvas.SetLeft(TimeScaleSlider, 100.0);
            this.Children.Add(TimeScaleSlider);
        }

        private void TimeScaleSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Timer.TimeScale = e.NewValue;
        }

        private void PauseButton_Pressed(object sender, PointerRoutedEventArgs e)
        {
            Timer.IsPaused = !Timer.IsPaused;
            TimeScaleSlider.IsEnabled = !Timer.IsPaused;
        }

        public void ResetPosition()
        {
            Canvas.SetTop(this, Scene.MainScene.Height - ToolbarHeight);
        }
    }
}
