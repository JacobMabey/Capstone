using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace PhysicsEngine.UI_Menus
{
    public class HsvColorPicker : StackPanel
    {
        public EventHandler ColorChanged;

        //Preview Color
        private Rectangle PreviewColorRect {  get; set; }
        public SolidColorBrush PreviewColorBrush { get; private set; }

        //Hue
        public int Hue { get; set; }
        private ThumbSlider HueSlider { get; set; }
        private TextBlock HueValue {  get; set; }

        //Saturation
        public int Saturation { get; set; }
        private ThumbSlider SaturationSlider { get; set; }
        private TextBlock SaturationValue { get; set; }

        //Value
        public int Value { get; set; }
        private ThumbSlider ValueSlider { get; set; }
        private TextBlock ValValue { get; set; }


        public HsvColorPicker(double width)
        {
            Width = width;
            Initialize();
            Hue = 0;
            Saturation = 100;
            Value = 100;
        }


        private void Initialize()
        {
            PreviewColorRect = new Rectangle
            {
                Width = Width,
                Height = 30,
            };
            PreviewColorBrush = new SolidColorBrush(Colors.Red);
            PreviewColorRect.Fill = PreviewColorBrush;
            Children.Add(PreviewColorRect);

            //Add Hue Customizer
            Children.Add(GetHueSliderGrid());

            //Add Saturation Customizer
            Children.Add(GetSaturationSliderGrid());

            //Add Saturation Customizer
            Children.Add(GetValueSliderGrid());
        }

        private Grid GetHueSliderGrid()
        {
            Grid hueGrid = new Grid();
            hueGrid.Width = Width;
            hueGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(0.15, GridUnitType.Star)
            });
            hueGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(0.7, GridUnitType.Star)
            });
            hueGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(0.15, GridUnitType.Star)
            });

            //Label
            TextBlock hueLabel = new TextBlock();
            Grid.SetColumn(hueLabel, 0);
            hueLabel.VerticalAlignment = VerticalAlignment.Center;
            hueLabel.Padding = new Thickness(0, 10, 0, 10);
            hueLabel.Text = "Hue:";
            hueLabel.FontSize = 12;
            hueLabel.FontFamily = MainPage.GlobalFont;
            hueLabel.Foreground = new SolidColorBrush(Colors.White);
            hueGrid.Children.Add(hueLabel);

            //Slider
            HueSlider = new ThumbSlider(hueGrid.Width * 0.65, 7);
            Grid.SetColumn(HueSlider, 1);
            HueSlider.Minimum = 0;
            HueSlider.Maximum = 359;
            HueSlider.Margin = new Thickness(0, 10, 0, 0);
            HueSlider.ValueChanged += HueSlider_ValueChanged;
            hueGrid.Children.Add(HueSlider);

            //Value
            HueValue = new TextBlock();
            Grid.SetColumn(HueValue, 2);
            HueValue.HorizontalAlignment = HorizontalAlignment.Center;
            HueValue.VerticalAlignment = VerticalAlignment.Center;
            HueValue.Padding = new Thickness(0, 10, 0, 10);
            HueValue.Text = HueSlider.Value + "";
            HueValue.FontSize = 12;
            HueValue.FontFamily = MainPage.GlobalFont;
            HueValue.Foreground = new SolidColorBrush(Colors.White);
            hueGrid.Children.Add(HueValue);

            return hueGrid;
        }

        private Grid GetSaturationSliderGrid()
        {
            Grid satGrid = new Grid();
            satGrid.Width = Width;
            satGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(0.15, GridUnitType.Star)
            });
            satGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(0.7, GridUnitType.Star)
            });
            satGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(0.15, GridUnitType.Star)
            });

            //Label
            TextBlock satLabel = new TextBlock();
            Grid.SetColumn(satLabel, 0);
            satLabel.VerticalAlignment = VerticalAlignment.Center;
            satLabel.Padding = new Thickness(0, 10, 0, 10);
            satLabel.Text = "Sat:";
            satLabel.FontSize = 12;
            satLabel.FontFamily = MainPage.GlobalFont;
            satLabel.Foreground = new SolidColorBrush(Colors.White);
            satGrid.Children.Add(satLabel);

            //Slider
            SaturationSlider = new ThumbSlider(satGrid.Width * 0.65, 7);
            Grid.SetColumn(SaturationSlider, 1);
            SaturationSlider.Minimum = 0;
            SaturationSlider.Maximum = 100;
            SaturationSlider.Margin = new Thickness(0, 10, 0, 0);
            SaturationSlider.ValueChanged += SaturationSlider_ValueChanged;
            satGrid.Children.Add(SaturationSlider);

            //Value
            SaturationValue = new TextBlock();
            Grid.SetColumn(SaturationValue, 2);
            SaturationValue.HorizontalAlignment = HorizontalAlignment.Center;
            SaturationValue.VerticalAlignment = VerticalAlignment.Center;
            SaturationValue.Padding = new Thickness(0, 10, 0, 10);
            SaturationValue.Text = SaturationSlider.Value + "";
            SaturationValue.FontSize = 12;
            SaturationValue.FontFamily = MainPage.GlobalFont;
            SaturationValue.Foreground = new SolidColorBrush(Colors.White);
            satGrid.Children.Add(SaturationValue);

            return satGrid;
        }

        private Grid GetValueSliderGrid()
        {
            Grid valueGrid = new Grid();
            valueGrid.Width = Width;
            valueGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(0.15, GridUnitType.Star)
            });
            valueGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(0.7, GridUnitType.Star)
            });
            valueGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(0.15, GridUnitType.Star)
            });

            //Label
            TextBlock valLabel = new TextBlock();
            Grid.SetColumn(valLabel, 0);
            valLabel.VerticalAlignment = VerticalAlignment.Center;
            valLabel.Padding = new Thickness(0, 10, 0, 10);
            valLabel.Text = "Val:";
            valLabel.FontSize = 12;
            valLabel.FontFamily = MainPage.GlobalFont;
            valLabel.Foreground = new SolidColorBrush(Colors.White);
            valueGrid.Children.Add(valLabel);

            //Slider
            ValueSlider = new ThumbSlider(valueGrid.Width * 0.65, 7);
            Grid.SetColumn(ValueSlider, 1);
            ValueSlider.Minimum = 0;
            ValueSlider.Maximum = 100;
            ValueSlider.Margin = new Thickness(0, 10, 0, 0);
            ValueSlider.ValueChanged += ValueSlider_ValueChanged;
            valueGrid.Children.Add(ValueSlider);

            //Value
            ValValue = new TextBlock();
            Grid.SetColumn(ValValue, 2);
            ValValue.HorizontalAlignment = HorizontalAlignment.Center;
            ValValue.VerticalAlignment = VerticalAlignment.Center;
            ValValue.Padding = new Thickness(0, 10, 0, 10);
            ValValue.Text = ValueSlider.Value + "";
            ValValue.FontSize = 12;
            ValValue.FontFamily = MainPage.GlobalFont;
            ValValue.Foreground = new SolidColorBrush(Colors.White);
            valueGrid.Children.Add(ValValue);

            return valueGrid;
        }


        private void HueSlider_ValueChanged(object sender, EventArgs e)
        {
            Hue = (int)HueSlider.Value;
            HueValue.Text = Hue+"";
            double[] newColorRaw = ColorFunctions.HsvToRgb(Hue, Saturation / 100.0, Value / 100.0);
            Color newColor = Color.FromArgb(255, (byte)newColorRaw[0], (byte)newColorRaw[1], (byte)newColorRaw[2]);
            PreviewColorBrush.Color = newColor;
            if (ColorChanged != null)
                ColorChanged(this, EventArgs.Empty);
        }
        private void SaturationSlider_ValueChanged(object sender, EventArgs e)
        {
            Saturation = (int)SaturationSlider.Value;
            SaturationValue.Text = Saturation + "";
            double[] newColorRaw = ColorFunctions.HsvToRgb(Hue, Saturation / 100.0, Value / 100.0);
            Color newColor = Color.FromArgb(255, (byte)newColorRaw[0], (byte)newColorRaw[1], (byte)newColorRaw[2]);
            PreviewColorBrush.Color = newColor;
            if (ColorChanged != null)
                ColorChanged(this, EventArgs.Empty);

            HueSlider.IsEnabled = Saturation != 0;
        }
        private void ValueSlider_ValueChanged(object sender, EventArgs e)
        {
            Value = (int)ValueSlider.Value;
            ValValue.Text = Value + "";
            double[] newColorRaw = ColorFunctions.HsvToRgb(Hue, Saturation / 100.0, Value / 100.0);
            Color newColor = Color.FromArgb(255, (byte)newColorRaw[0], (byte)newColorRaw[1], (byte)newColorRaw[2]);
            PreviewColorBrush.Color = newColor;
            if (ColorChanged != null)
                ColorChanged(this, EventArgs.Empty);

            SaturationSlider.IsEnabled = Value != 0;
        }


        public void SetColor(Color color)
        {
            double[] hsvColor = ColorFunctions.RgbToHsv(color.R, color.G, color.B);
            Color newColor = color;
            PreviewColorBrush.Color = newColor;

            Hue = (int)hsvColor[0];
            HueValue.Text = Hue + "";
            Canvas.SetLeft(HueSlider.Thumb, (double)Hue / (double)359 * SaturationSlider.Width - HueSlider.Thumb.Width / 2.0);

            Saturation = (int)(hsvColor[1] * 100.0);
            SaturationValue.Text = Saturation + "";
            Canvas.SetLeft(SaturationSlider.Thumb, (double)Saturation / (double)100 * SaturationSlider.Width - SaturationSlider.Thumb.Width / 2.0);
            HueSlider.IsEnabled = Saturation != 0;

            Value = (int)(hsvColor[2] * 100.0);
            ValValue.Text = Value + "";
            Canvas.SetLeft(ValueSlider.Thumb, (double)Value / (double)100 * ValueSlider.Width - ValueSlider.Thumb.Width / 2.0);
            SaturationSlider.IsEnabled = Value != 0;
        }
    }
}