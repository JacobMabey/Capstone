using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace PhysicsEngine.UI_Menus
{
    public class WorldSettingsMenu : VerticalMenu
    {
        public HsvColorPicker BgColorPicker { get; private set; }
        StackPanel SettingsStack {  get; set; }

        public override void Initialize(double width, double menuX, Color bgColor)
        {
            base.Initialize(width, menuX, bgColor);
            ExpandBoard.Completed += ExpandBoard_Completed;

            SettingsStack = new StackPanel();
            SettingsStack.Margin = new Thickness(10);
            SettingsStack.Visibility = Visibility.Collapsed;

            //Set settings title
            TextBlock settingsTitle = new TextBlock();
            settingsTitle.Text = "Scene Settings";
            settingsTitle.FontFamily = MainPage.GlobalFont;
            settingsTitle.FontSize = 32;
            settingsTitle.Foreground = new SolidColorBrush(Colors.White);
            settingsTitle.Margin = new Thickness(0, 0, 0, 20);
            SettingsStack.Children.Add(settingsTitle);

            //Set Gravity Customization
            Grid gravityInputStack = GetGravityInputGrid(); //Gravity labels and input
            SettingsStack.Children.Add(gravityInputStack);

            TextBlock gravityTooltip = new TextBlock(); //Gravity Tooltip
            gravityTooltip.Text = "-1000 < g < 1000";
            gravityTooltip.FontSize = 10;
            gravityTooltip.FontFamily = MainPage.GlobalFont;
            gravityTooltip.Foreground = new SolidColorBrush(Colors.LightGray);
            gravityTooltip.HorizontalAlignment = HorizontalAlignment.Center;
            SettingsStack.Children.Add(gravityTooltip);


            //Set Background Color Customization
            BgColorPicker = new HsvColorPicker(MenuWidth - 40);
            BgColorPicker.ColorChanged += (s, e) => { Renderer.BackgroundColor = BgColorPicker.PreviewColorBrush.Color; };
            SettingsStack.Children.Add(BgColorPicker);


            this.Children.Add(SettingsStack);
        }

        private void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            Renderer.BackgroundColor = sender.Color;
        }

        private void GravityCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            
        }

        private void GravityCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }


        //Gravity Input
        private Grid GetGravityInputGrid()
        {
            Grid gravityInputGrid = new Grid();
            gravityInputGrid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(0.25, GridUnitType.Star)
            });
            gravityInputGrid.ColumnDefinitions.Add(new ColumnDefinition());
            gravityInputGrid.ColumnDefinitions.Add(new ColumnDefinition());
            gravityInputGrid.ColumnDefinitions.Add(new ColumnDefinition());
            gravityInputGrid.Padding = new Thickness(0, 5, 0, 5);
            gravityInputGrid.Width = MenuWidth - (30 + SettingsStack.Margin.Right);

            //Make gravity label
            TextBlock gravityLabel = new TextBlock();
            Grid.SetColumn(gravityLabel, 1);
            gravityLabel.VerticalAlignment = VerticalAlignment.Center;
            gravityLabel.Padding = new Thickness(5);
            gravityLabel.Text = "Gravity:";
            gravityLabel.FontSize = 16;
            gravityLabel.FontFamily = MainPage.GlobalFont;
            gravityLabel.Foreground = new SolidColorBrush(Colors.White);
            gravityInputGrid.Children.Add(gravityLabel);

            //Make text input
            TextBox gravityInput = new TextBox();
            Grid.SetColumn(gravityInput, 2);
            gravityInput.MaxLength = 6;
            gravityInput.TextAlignment = TextAlignment.Center;
            gravityInput.TextWrapping = TextWrapping.Wrap;
            gravityInput.Text = Physics.GravityAcceleration + "";
            gravityInput.GotFocus += (object o, RoutedEventArgs e) => gravityInput.SelectAll();
            gravityInput.BeforeTextChanging += GravityInput_BeforeTextChanging;
            gravityInputGrid.Children.Add(gravityInput);

            //Make checkbox
            CheckBox gravityCheckbox = new CheckBox();
            Grid.SetColumn(gravityCheckbox, 0);
            gravityCheckbox.IsChecked = true;
            gravityCheckbox.Checked += (object s, RoutedEventArgs e) => { gravityInput.IsEnabled = true; Physics.GravityAcceleration = double.Parse(gravityInput.Text); };
            gravityCheckbox.Unchecked += (object s, RoutedEventArgs e) => { gravityInput.IsEnabled = false; Physics.GravityAcceleration = 0.0; };
            gravityInputGrid.Children.Add(gravityCheckbox);

            //Make pps label
            TextBlock ppsLabel = new TextBlock();
            Grid.SetColumn(ppsLabel, 3);
            ppsLabel.VerticalAlignment = VerticalAlignment.Center;
            ppsLabel.HorizontalAlignment = HorizontalAlignment.Center;
            ppsLabel.Padding = new Thickness(10);
            ppsLabel.Text = "p/s/s";
            ppsLabel.FontSize = 12;
            ppsLabel.FontFamily = MainPage.GlobalFont;
            ppsLabel.Foreground = new SolidColorBrush(Colors.White);
            gravityInputGrid.Children.Add(ppsLabel);

            return gravityInputGrid;
        }
        private void GravityInput_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (double.TryParse(args.NewText, out double parsed))
            {
                if (Math.Abs(parsed) > Physics.MaxGravity)
                {

                    parsed = Math.Sign(parsed) * Physics.MaxGravity;
                    Physics.GravityAcceleration = parsed;
                    sender.Text = parsed + "";
                }
                else
                    Physics.GravityAcceleration = parsed;
            } else
            {
                args.Cancel = true;
            }
        }




        //Expansion
        private void ExpandBoard_Completed(object sender, object e)
        {
            SettingsStack.Visibility = Visibility.Visible;
        }
        public override void ToggleMenuExpanded()
        {
            base.ToggleMenuExpanded();

            if (!IsMenuExpanded)
            {
                SettingsStack.Visibility = Visibility.Collapsed;
            }
        }
    }
}