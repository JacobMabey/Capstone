using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace PhysicsEngine.UI_Menus
{
    public class WorldSettingsMenu : VerticalMenu
    {
        public HsvColorPicker BgColorPicker { get; private set; }
        StackPanel SettingsStack {  get; set; }

        public TextBox GravityInput { get; set; }
        public CheckBox GravityCheckbox { get; set; }

        public CheckBox EdgeBorderCheckbox { get; set; }

        public CheckBox CircBorderCheckbox { get; set; }
        public TextBox CircRadInput { get; set; }

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


            //Save and load grid
            Grid saveLoadGrid = GetSaveLoadSceneGrid();
            SettingsStack.Children.Add(saveLoadGrid);


            //Hotkey Tooltips
            TextBlock hotkeyTitle = new TextBlock();
            hotkeyTitle.Margin = new Thickness(0, 0, 0, 5);
            hotkeyTitle.Text = "Hotkeys";
            hotkeyTitle.FontSize = 16;
            hotkeyTitle.FontFamily = MainPage.GlobalFont;
            hotkeyTitle.Foreground = new SolidColorBrush(Color.FromArgb(255, 240, 176, 146));
            hotkeyTitle.HorizontalAlignment = HorizontalAlignment.Center;
            SettingsStack.Children.Add(hotkeyTitle);

            TextBlock tooltips = new TextBlock();
            tooltips.Text =
                "  G\t- Toggle Gravity\n" +
                "  C\t- Copy Selected Component\n" +
                "Space\t- Toggle Time Paused\n" +
                "Esc\t- Close All Menus\n" +
                "  A\t- Open Add Component Menu\n" +
                "  S\t- Open World Settings Menu\n" +
                "  X\t- Clear Add Component Mode";
            tooltips.FontSize = 12;
            tooltips.FontFamily = MainPage.GlobalFont;
            tooltips.Foreground = new SolidColorBrush(Color.FromArgb(255, 240, 176, 146));
            tooltips.HorizontalAlignment = HorizontalAlignment.Center;
            SettingsStack.Children.Add(tooltips);


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
            GravityInput = new TextBox();
            Grid.SetColumn(GravityInput, 2);
            GravityInput.MaxLength = 6;
            GravityInput.Foreground = new SolidColorBrush(Colors.White);
            GravityInput.FontFamily = MainPage.GlobalFont;
            GravityInput.TextAlignment = TextAlignment.Center;
            GravityInput.TextWrapping = TextWrapping.Wrap;
            GravityInput.Text = Physics.GravityAcceleration + "";
            GravityInput.GotFocus += (object o, RoutedEventArgs e) => GravityInput.SelectAll();
            GravityInput.BeforeTextChanging += GravityInput_BeforeTextChanging;
            gravityInputGrid.Children.Add(GravityInput);

            //Make checkbox
            GravityCheckbox = new CheckBox();
            Grid.SetColumn(GravityCheckbox, 0);
            GravityCheckbox.IsChecked = Physics.IsGravityEnabled;
            GravityInput.IsEnabled = Physics.IsGravityEnabled;
            GravityCheckbox.Checked += (object s, RoutedEventArgs e) => { GravityInput.IsEnabled = true; Physics.IsGravityEnabled = true; };
            GravityCheckbox.Unchecked += (object s, RoutedEventArgs e) => { GravityInput.IsEnabled = false; Physics.IsGravityEnabled = false; };
            gravityInputGrid.Children.Add(GravityCheckbox);

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

            EdgeBorderCheckbox = new CheckBox();
            Grid.SetColumn(EdgeBorderCheckbox, 4);
            EdgeBorderCheckbox.IsChecked = true;
            EdgeBorderCheckbox.Checked += (s, o) =>
            {
                Scene.SetBorderCollision(true);
            };
            EdgeBorderCheckbox.Unchecked += (s, o) =>
            {
                Scene.SetBorderCollision(false);
            };
            edgeBorderGrid.Children.Add(EdgeBorderCheckbox);


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

            CircBorderCheckbox = new CheckBox();
            Grid.SetColumn(CircBorderCheckbox, 4);
            CircBorderCheckbox.IsChecked = Scene.IsCircleBorderActive;
            circBorderGrid.Children.Add(CircBorderCheckbox);


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
            CircRadInput = new TextBox();
            Grid.SetColumn(CircRadInput, 3);
            Grid.SetColumnSpan(CircRadInput, 2);
            Grid.SetRow(CircRadInput, 1);
            CircRadInput.MaxLength = 3;
            CircRadInput.IsEnabled = false;
            CircRadInput.Foreground = new SolidColorBrush(Colors.White);
            CircRadInput.FontFamily = MainPage.GlobalFont;
            CircRadInput.TextAlignment = TextAlignment.Center;
            CircRadInput.TextWrapping = TextWrapping.Wrap;
            CircRadInput.Text = Scene.CircleBorderRadius+"";
            CircRadInput.GotFocus += (object o, RoutedEventArgs e) => CircRadInput.SelectAll();
            CircRadInput.BeforeTextChanging += CircRadInput_BeforeTextChanging;
            circBorderGrid.Children.Add(CircRadInput);


            CircBorderCheckbox.Checked += (s, o) =>
            {
                Scene.SetCircleBorderCollision(true);
                lableColor.Color = Colors.LightGray;
                CircRadInput.IsEnabled = true;
            };
            CircBorderCheckbox.Unchecked += (s, o) =>
            {
                Scene.SetCircleBorderCollision(false);
                lableColor.Color = Colors.Gray;
                CircRadInput.IsEnabled = false;
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

        //Save and Load Scene Grid
        private Grid GetSaveLoadSceneGrid()
        {
            Grid saveLoadGrid = new Grid();
            saveLoadGrid.Margin = new Thickness(0, 10, 0, 0);
            saveLoadGrid.ColumnDefinitions.Add(new ColumnDefinition());
            saveLoadGrid.ColumnDefinitions.Add(new ColumnDefinition());
            saveLoadGrid.RowDefinitions.Add(new RowDefinition());
            saveLoadGrid.RowDefinitions.Add(new RowDefinition());

            //Title
            TextBlock saveLoadTitle = new TextBlock();
            Grid.SetColumn(saveLoadTitle, 0);
            Grid.SetColumnSpan(saveLoadTitle, 2);
            Grid.SetRow(saveLoadTitle, 0);
            saveLoadTitle.Text = "Save / Load Scene";
            saveLoadTitle.FontSize = 18;
            saveLoadTitle.FontFamily = MainPage.GlobalFont;
            saveLoadTitle.Foreground = new SolidColorBrush(Colors.White);
            saveLoadTitle.HorizontalAlignment = HorizontalAlignment.Center;
            saveLoadGrid.Children.Add(saveLoadTitle);

            //Save Button
            Grid saveButton = GetButton("Save", Colors.White);
            Grid.SetColumn(saveButton, 0);
            Grid.SetRow(saveButton, 1);
            saveButton.PointerReleased += SaveButton_PointerReleased;
            saveLoadGrid.Children.Add(saveButton);

            //Load Button
            Grid loadButton = GetButton("Load", Colors.White);
            Grid.SetColumn(loadButton, 1);
            Grid.SetRow(loadButton, 1);
            loadButton.PointerReleased += LoadButton_PointerReleased;
            saveLoadGrid.Children.Add(loadButton);

            return saveLoadGrid;
        }

        private void SaveButton_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Scene.SaveScene();
        }
        private void LoadButton_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Scene.LoadScene();
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