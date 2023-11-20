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
            SettingsStack.Width = MenuWidth - 50;
            SettingsStack.Margin = new Thickness(10, 35, 10, 10);
            SettingsStack.Visibility = Visibility.Collapsed;

            //Set settings title
            TextBlock settingsTitle = new TextBlock();
            settingsTitle.Text = "Scene Settings";
            settingsTitle.FontFamily = MainPage.GlobalFont;
            settingsTitle.FontSize = 32;
            settingsTitle.Foreground = new SolidColorBrush(Colors.White);
            settingsTitle.Margin = new Thickness(0, 0, 0, 20);
            settingsTitle.HorizontalAlignment = HorizontalAlignment.Center;
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
            TextBlock bgColorTitle = new TextBlock();
            bgColorTitle.Text = "Background Color";
            bgColorTitle.FontSize = 20;
            bgColorTitle.FontFamily = MainPage.GlobalFont;
            bgColorTitle.Foreground = new SolidColorBrush(Colors.White);
            bgColorTitle.Margin = new Thickness(0, 30, 0, 5);
            bgColorTitle.HorizontalAlignment = HorizontalAlignment.Center;
            SettingsStack.Children.Add(bgColorTitle);

            BgColorPicker = new HsvColorPicker(MenuWidth - 40);
            BgColorPicker.ColorChanged += (s, e) => { Renderer.BackgroundColor = BgColorPicker.PreviewColorBrush.Color; };
            SettingsStack.Children.Add(BgColorPicker);


            //Set toggleable Borders
            //Edge Border
            Grid edgeBorderGrid = GetEdgeBorderToggleGrid();
            SettingsStack.Children.Add(edgeBorderGrid);

            //Circle Border
            Grid circBorderGrid = GetCircleBorderToggleGrid();
            SettingsStack.Children.Add(circBorderGrid);


            //Window Size Customization



            this.Children.Add(SettingsStack);
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
            gravityCheckbox.IsChecked = Physics.IsGravityEnabled;
            gravityInput.IsEnabled = Physics.IsGravityEnabled;
            gravityCheckbox.Checked += (object s, RoutedEventArgs e) => { gravityInput.IsEnabled = true; Physics.IsGravityEnabled = true; };
            gravityCheckbox.Unchecked += (object s, RoutedEventArgs e) => { gravityInput.IsEnabled = false; Physics.IsGravityEnabled = false; };
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


        //Toggle Edge Border Input
        private Grid GetEdgeBorderToggleGrid()
        {
            Grid edgeBorderGrid = new Grid();
            edgeBorderGrid.Width = MenuWidth - 40;
            edgeBorderGrid.Margin = new Thickness(0, 20, 0, 0);
            edgeBorderGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            edgeBorderGrid.ColumnDefinitions.Add(new ColumnDefinition());
            edgeBorderGrid.ColumnDefinitions.Add(new ColumnDefinition());
            edgeBorderGrid.ColumnDefinitions.Add(new ColumnDefinition());
            edgeBorderGrid.ColumnDefinitions.Add(new ColumnDefinition());
            edgeBorderGrid.ColumnDefinitions.Add(new ColumnDefinition());

            TextBlock edgeBorderLabel = new TextBlock();
            Grid.SetColumn(edgeBorderLabel, 0);
            Grid.SetColumnSpan(edgeBorderLabel, 4);
            edgeBorderLabel.HorizontalAlignment = HorizontalAlignment.Center;
            edgeBorderLabel.VerticalAlignment = VerticalAlignment.Center;
            edgeBorderLabel.Text = "Toggle Edge Borders";
            edgeBorderLabel.FontSize = 16;
            edgeBorderLabel.FontFamily = MainPage.GlobalFont;
            edgeBorderLabel.Foreground = new SolidColorBrush(Colors.White);
            edgeBorderGrid.Children.Add(edgeBorderLabel);

            CheckBox edgeBorderCheckbox = new CheckBox();
            Grid.SetColumn(edgeBorderCheckbox, 4);
            edgeBorderCheckbox.IsChecked = true;
            edgeBorderCheckbox.Checked += (s, o) =>
            {
                Scene.SetBorderCollision(true);
            };
            edgeBorderCheckbox.Unchecked += (s, o) =>
            {
                Scene.SetBorderCollision(false);
            };
            edgeBorderGrid.Children.Add(edgeBorderCheckbox);


            return edgeBorderGrid;
        }


        //Toggle Edge Border Input
        private Grid GetCircleBorderToggleGrid()
        {
            Grid circBorderGrid = new Grid();
            circBorderGrid.Width = MenuWidth - 40;
            circBorderGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            circBorderGrid.ColumnDefinitions.Add(new ColumnDefinition());
            circBorderGrid.ColumnDefinitions.Add(new ColumnDefinition());
            circBorderGrid.ColumnDefinitions.Add(new ColumnDefinition());
            circBorderGrid.ColumnDefinitions.Add(new ColumnDefinition());
            circBorderGrid.ColumnDefinitions.Add(new ColumnDefinition());
            circBorderGrid.RowDefinitions.Add(new RowDefinition());
            circBorderGrid.RowDefinitions.Add(new RowDefinition());

            TextBlock circBorderLabel = new TextBlock();
            Grid.SetColumn(circBorderLabel, 0);
            Grid.SetColumnSpan(circBorderLabel, 4);
            circBorderLabel.HorizontalAlignment = HorizontalAlignment.Center;
            circBorderLabel.VerticalAlignment = VerticalAlignment.Center;
            circBorderLabel.Text = "Toggle Circle Border";
            circBorderLabel.FontSize = 16;
            circBorderLabel.FontFamily = MainPage.GlobalFont;
            circBorderLabel.Foreground = new SolidColorBrush(Colors.White);
            circBorderGrid.Children.Add(circBorderLabel);

            CheckBox circBorderCheckbox = new CheckBox();
            Grid.SetColumn(circBorderCheckbox, 4);
            circBorderCheckbox.IsChecked = Scene.IsCircleBorderActive;
            circBorderGrid.Children.Add(circBorderCheckbox);


            //Second Row
            TextBlock circRadLabel = new TextBlock();
            Grid.SetColumn(circRadLabel, 0);
            Grid.SetColumnSpan(circRadLabel, 3);
            Grid.SetRow(circRadLabel, 1);
            circRadLabel.VerticalAlignment = VerticalAlignment.Center;
            circRadLabel.Text = "Set Circle Radius";
            circRadLabel.FontSize = 14;
            circRadLabel.FontFamily = MainPage.GlobalFont;
            SolidColorBrush lableColor = new SolidColorBrush(Scene.IsCircleBorderActive ? Colors.LightGray : Colors.Gray);
            circRadLabel.Foreground = lableColor;
            circBorderGrid.Children.Add(circRadLabel);

            //Make text input
            TextBox circRadInput = new TextBox();
            Grid.SetColumn(circRadInput, 3);
            Grid.SetColumnSpan(circRadInput, 2);
            Grid.SetRow(circRadInput, 1);
            circRadInput.MaxLength = 3;
            circRadInput.IsEnabled = false;
            circRadInput.TextAlignment = TextAlignment.Center;
            circRadInput.TextWrapping = TextWrapping.Wrap;
            circRadInput.Text = Scene.CircleBorderRadius+"";
            circRadInput.GotFocus += (object o, RoutedEventArgs e) => circRadInput.SelectAll();
            circRadInput.BeforeTextChanging += CircRadInput_BeforeTextChanging;
            circBorderGrid.Children.Add(circRadInput);


            circBorderCheckbox.Checked += (s, o) =>
            {
                Scene.SetCircleBorderCollision(true);
                lableColor.Color = Colors.LightGray;
                circRadInput.IsEnabled = true;
            };
            circBorderCheckbox.Unchecked += (s, o) =>
            {
                Scene.SetCircleBorderCollision(false);
                lableColor.Color = Colors.Gray;
                circRadInput.IsEnabled = false;
            };

            return circBorderGrid;
        }

        private void CircRadInput_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (double.TryParse(args.NewText, out double parsed))
            {
                if (parsed > 999)
                {
                    parsed = Math.Sign(parsed) * 999;
                    Scene.SetCircleBorderRadius(parsed);
                    sender.Text = parsed + "";
                }
                else if (parsed < 10)
                {
                    parsed = 10;
                    Scene.SetCircleBorderRadius(parsed);
                    sender.Text = parsed + "";
                }
                else
                    Scene.SetCircleBorderRadius(parsed);
            }
            else
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


        public override void ResetPosition()
        {
            base.ResetPosition();
            MenuX = MainPage.WindowSize.Width;
        }
    }
}