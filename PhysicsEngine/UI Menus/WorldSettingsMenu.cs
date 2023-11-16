using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace PhysicsEngine.UI_Menus
{
    public class WorldSettingsMenu : VerticalMenu
    {
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
            StackPanel gravityInputStack = new StackPanel();
            gravityInputStack.Orientation = Orientation.Horizontal;
            gravityInputStack.Spacing = 10;
            gravityInputStack.Width = MenuWidth;
            gravityInputStack.HorizontalAlignment = HorizontalAlignment.Center;

            TextBlock gravityLabel = new TextBlock();
            gravityLabel.Padding = new Thickness(0, 5, 0, 5);
            gravityLabel.Text = "Gravity:";
            gravityLabel.FontSize = 16;
            gravityLabel.FontFamily = MainPage.GlobalFont;
            gravityLabel.Foreground = new SolidColorBrush(Colors.White);
            gravityInputStack.Children.Add(gravityLabel);

            TextBox gravityInput = new TextBox();
            gravityInput.MaxLength = 6;
            gravityInput.Width = MenuWidth * 0.45;
            gravityInput.TextWrapping = TextWrapping.Wrap;
            gravityInput.Text = Physics.GravityAcceleration+"";
            gravityInput.GotFocus += (object o, RoutedEventArgs e) => gravityInput.SelectAll();
            gravityInput.BeforeTextChanging += GravityInput_BeforeTextChanging;
            gravityInputStack.Children.Add(gravityInput);

            TextBlock ppsLabel = new TextBlock();
            ppsLabel.Padding = new Thickness(0, 10, 0, 10);
            ppsLabel.Text = "p/s/s";
            ppsLabel.FontSize = 12;
            ppsLabel.FontFamily = MainPage.GlobalFont;
            ppsLabel.Foreground = new SolidColorBrush(Colors.White);
            gravityInputStack.Children.Add(ppsLabel);

            SettingsStack.Children.Add(gravityInputStack);

            TextBlock gravityTooltip = new TextBlock();
            gravityTooltip.Text = "-1000 < g < 1000";
            gravityTooltip.FontSize = 10;
            gravityTooltip.FontFamily = MainPage.GlobalFont;
            gravityTooltip.Foreground = new SolidColorBrush(Colors.LightGray);
            gravityTooltip.HorizontalAlignment = HorizontalAlignment.Center;
            SettingsStack.Children.Add(gravityTooltip);


            this.Children.Add(SettingsStack);
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