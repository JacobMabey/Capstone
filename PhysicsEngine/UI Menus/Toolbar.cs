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

        //Toobar border
        private Rectangle rectBorder;

        //Pause Button
        private Rectangle PauseButton { get; set; }
        private TextBlock PauseText { get; set; }


        //TimeScale Slider
        private TextBlock TimeScaleText { get; set; }
        private Slider TimeScaleSlider { get; set; }


        //Clear Scene Button
        private Rectangle ClearButton { get; set; }


        public Toolbar() { Initialize(); }

        private void Initialize()
        {
            //Initialize
            Height = ToolbarHeight;
            Canvas.SetZIndex(this, 100);

            //Background
            Background = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0));

            //Border
            rectBorder = new Rectangle();
            rectBorder.Width = Width + 2;
            rectBorder.Height = ToolbarHeight + 2;
            rectBorder.Stroke = new SolidColorBrush(Colors.Black);
            rectBorder.StrokeThickness = 1;
            Canvas.SetLeft(rectBorder, -1);
            Canvas.SetTop(rectBorder, -1);
            this.Children.Add(rectBorder);

            //Pause Button
            PauseButton = new Rectangle();
            PauseButton.Width = ToolbarHeight;
            PauseButton.Height = ToolbarHeight;
            PauseButton.Fill = new SolidColorBrush(Colors.Green);
            Canvas.SetLeft(PauseButton, 1.0);
            Canvas.SetZIndex(PauseButton, 101);
            PauseButton.PointerPressed += PauseButton_Pressed;
            this.Children.Add(PauseButton);


            //TimeScale Text
            TimeScaleText = new TextBlock();
            TimeScaleText.Text = (int)(Timer.TimeScale * 100.0) + "%";
            TimeScaleText.Margin = new Thickness(4);
            Canvas.SetLeft(TimeScaleText, 42.0);
            this.Children.Add(TimeScaleText);

            //TimeScale Slider
            TimeScaleSlider = new Slider();
            TimeScaleSlider.ValueChanged += TimeScaleSlider_ValueChanged;
            TimeScaleSlider.Minimum = 0.0;
            TimeScaleSlider.Maximum = 1.0;
            TimeScaleSlider.StepFrequency = 0.01;
            TimeScaleSlider.Value = Timer.TimeScale;
            TimeScaleSlider.Width = 200.0;
            TimeScaleSlider.Height = ToolbarHeight;
            TimeScaleSlider.Margin = new Thickness(-4);
            TimeScaleSlider.RequestedTheme = ElementTheme.Dark;
            Canvas.SetLeft(TimeScaleSlider, 100.0);
            this.Children.Add(TimeScaleSlider);


            //Clear Button
            ClearButton = new Rectangle();
            ClearButton.Width = ToolbarHeight;
            ClearButton.Height = ToolbarHeight;
            ClearButton.Fill = new SolidColorBrush(Colors.Red);
            Canvas.SetLeft(ClearButton, Width - ToolbarHeight - 1.0);
            Canvas.SetZIndex(ClearButton, 101);
            ClearButton.PointerPressed += ClearButton_Pressed;
            this.Children.Add(ClearButton);


            ResetPosition();
        }

        private void TimeScaleSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Timer.TimeScale = e.NewValue;
            TimeScaleText.Text = (int)(Timer.TimeScale * 100.0) + "%";
        }

        private void PauseButton_Pressed(object sender, PointerRoutedEventArgs e)
        {
            Timer.IsPaused = !Timer.IsPaused;
            TimeScaleSlider.IsEnabled = !Timer.IsPaused;
            TimeScaleText.Text = (int)(Timer.TimeScale * 100.0) + "%";
            PauseButton.Fill = Timer.IsPaused ? new SolidColorBrush(Colors.Blue) : new SolidColorBrush(Colors.Green);
        }

        private void ClearButton_Pressed(object sender, PointerRoutedEventArgs e)
        {
            Scene.ClearScene();
        }


        public void ResetPosition()
        {
            Canvas.SetTop(this, Scene.MainScene.Height - ToolbarHeight);
            Width = MainPage.WindowSize.Width;

            Canvas.SetLeft(ClearButton, Width - ToolbarHeight - 1.0);
            rectBorder.Width = Width + 2;
        }
    }
}
