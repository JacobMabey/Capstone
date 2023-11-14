using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace PhysicsEngine.UI_Menus
{
    public class Toolbar : Canvas
    {
        public static readonly double ToolbarHeight = 30.0;

        //Toobar border
        private Rectangle rectBorder;

        //Pause Button
        private Image PauseButton { get; set; }


        //TimeScale Slider
        private TextBlock TimeScaleText { get; set; }
        private Slider TimeScaleSlider { get; set; }

        //Add Component Button
        private Image AddButton { get; set; }


        //Clear Scene Button
        private Image ClearButton { get; set; }
        
        //Settings Button
        private Image SettingsButton { get; set; }



        public Toolbar() { Initialize(); }

        private void Initialize()
        {
            //Initialize
            Height = ToolbarHeight;
            Canvas.SetZIndex(this, 100);

            //Background
            Background = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));

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
            PauseButton = new Image();
            PauseButton.Width = ToolbarHeight;
            PauseButton.Height = ToolbarHeight;
            PauseButton.Source = new BitmapImage(new Uri("ms-appx:///Assets/StoreLogo.png"));
            Canvas.SetLeft(PauseButton, 5.0);
            Canvas.SetZIndex(PauseButton, 101);
            PauseButton.PointerPressed += PauseButton_Pressed;
            this.Children.Add(PauseButton);


            //TimeScale Text
            TimeScaleText = new TextBlock();
            TimeScaleText.Foreground = new SolidColorBrush(Colors.White);
            TimeScaleText.Text = (int)(Timer.TimeScale * 100.0) + "%";
            TimeScaleText.Margin = new Thickness(4);
            Canvas.SetLeft(TimeScaleText, ToolbarHeight + 10.0);
            this.Children.Add(TimeScaleText);

            //TimeScale Slider
            TimeScaleSlider = new Slider();
            TimeScaleSlider.ValueChanged += TimeScaleSlider_ValueChanged;
            TimeScaleSlider.IsThumbToolTipEnabled = false;
            double sliderStretchScale = 1.6;
            ScaleTransform scale = new ScaleTransform();
            scale.ScaleX = sliderStretchScale;
            TimeScaleSlider.RenderTransform = scale;
            TimeScaleSlider.Minimum = 0.0;
            TimeScaleSlider.Maximum = Timer.TIMESCALE_MAX;
            TimeScaleSlider.StepFrequency = 0.01;
            TimeScaleSlider.Value = Timer.TimeScale;
            TimeScaleSlider.Width = 200.0 / sliderStretchScale;
            TimeScaleSlider.Height = ToolbarHeight;
            TimeScaleSlider.Margin = new Thickness(-4);
            TimeScaleSlider.RequestedTheme = ElementTheme.Dark;
            Canvas.SetLeft(TimeScaleSlider, 100.0);
            this.Children.Add(TimeScaleSlider);


            //Add Component Button
            AddButton = new Image();
            AddButton.Width = ToolbarHeight;
            AddButton.Height = ToolbarHeight;
            AddButton.Source = new BitmapImage(new Uri("ms-appx:///Assets/StoreLogo.png"));
            Canvas.SetLeft(AddButton, Canvas.GetLeft(TimeScaleSlider) + TimeScaleSlider.Width * sliderStretchScale + 5.0);
            Canvas.SetZIndex(AddButton, 101);
            AddButton.PointerPressed += AddButton_Pressed;
            this.Children.Add(AddButton);

            //Clear Button
            ClearButton = new Image();
            ClearButton.Width = ToolbarHeight;
            ClearButton.Height = ToolbarHeight;
            ClearButton.Source = new BitmapImage(new Uri("ms-appx:///Assets/StoreLogo.png"));
            Canvas.SetZIndex(ClearButton, 101);
            ClearButton.PointerPressed += ClearButton_Pressed;
            this.Children.Add(ClearButton);

            //Settings Button
            SettingsButton = new Image();
            SettingsButton.Width = ToolbarHeight;
            SettingsButton.Height = ToolbarHeight;
            SettingsButton.Source = new BitmapImage(new Uri("ms-appx:///Assets/StoreLogo.png"));
            Canvas.SetZIndex(SettingsButton, 101);
            SettingsButton.PointerPressed += SettingsButton_Pressed;
            this.Children.Add(SettingsButton);


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
            //PauseButton.Fill = Timer.IsPaused ? new SolidColorBrush(Colors.Blue) : new SolidColorBrush(Colors.Green);
            PauseButton.Source = new BitmapImage(new Uri("ms-appx:///Assets/StoreLogo.png"));
        }

        private void AddButton_Pressed(object sender, PointerRoutedEventArgs e)
        {
            //Toggle add component menu
            Scene.AddMenu.ToggleMenuExpanded();
        }

        private void ClearButton_Pressed(object sender, PointerRoutedEventArgs e)
        {
            Scene.ClearScene();
        }
        private void SettingsButton_Pressed(object sender, PointerRoutedEventArgs e)
        {
            //Toggle Settings Menu
        }



        public void ResetPosition()
        {
            Canvas.SetTop(this, Scene.MainScene.Height - ToolbarHeight);
            Width = MainPage.WindowSize.Width;

            Canvas.SetLeft(ClearButton, Width - (ToolbarHeight + 5.0) * 2.0);
            Canvas.SetLeft(SettingsButton, Width - ToolbarHeight - 5.0);
            rectBorder.Width = Width + 2;
        }
    }
}
