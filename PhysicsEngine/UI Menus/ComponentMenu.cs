

using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace PhysicsEngine.UI_Menus
{
    public class ComponentMenu : VerticalMenu
    {
        public Component ParentComponent { get; set; } = null;

        private StackPanel SettingsStack {  get; set; }

        TextBlock CompTitle {  get; set; }

        HsvColorPicker ColorPicker { get; set; }

        Grid HasCollisionCheck { get; set; }
        CheckBox HasCollisionCheckbox { get; set; }

        public override void Initialize(double width, double menuX, Color bgColor)
        {
            base.Initialize(width, menuX, bgColor);
            SettingsStack = new StackPanel();
            SettingsStack.Margin = new Thickness(6);

            CompTitle = new TextBlock();
            CompTitle.Text = "N/A";
            CompTitle.FontFamily = MainPage.GlobalFont;
            CompTitle.FontSize = 32;
            CompTitle.Foreground = new SolidColorBrush(Colors.White);
            CompTitle.Margin = new Thickness(0, 0, 0, 20);
            CompTitle.HorizontalAlignment = HorizontalAlignment.Center;
            SettingsStack.Children.Add(CompTitle);


            //Make has collision label and checkbox
            HasCollisionCheck = new Grid();
            HasCollisionCheck.Margin = new Thickness(10);
            HasCollisionCheck.ColumnDefinitions.Add(new ColumnDefinition());
            HasCollisionCheck.ColumnDefinitions.Add(new ColumnDefinition());
            HasCollisionCheck.ColumnDefinitions.Add(new ColumnDefinition());
            HasCollisionCheck.ColumnDefinitions.Add(new ColumnDefinition());
            HasCollisionCheck.ColumnDefinitions.Add(new ColumnDefinition());

            TextBlock hasCollisionLabel = new TextBlock();
            Grid.SetColumn(hasCollisionLabel, 0);
            Grid.SetColumnSpan(hasCollisionLabel, 3);
            hasCollisionLabel.Text = "Toggle Collision";
            hasCollisionLabel.FontSize = 14;
            hasCollisionLabel.FontFamily = MainPage.GlobalFont;
            hasCollisionLabel.Foreground = new SolidColorBrush(Colors.White);
            hasCollisionLabel.VerticalAlignment = VerticalAlignment.Center;
            hasCollisionLabel.HorizontalAlignment = HorizontalAlignment.Center;
            HasCollisionCheck.Children.Add(hasCollisionLabel);

            HasCollisionCheckbox = new CheckBox();
            Grid.SetColumn(HasCollisionCheckbox, 4);
            HasCollisionCheckbox.IsChecked = true;
            HasCollisionCheckbox.Checked += (s, o) => { ParentComponent.IsCollisionEnabled = true; };
            HasCollisionCheckbox.Unchecked += (s, o) => { ParentComponent.IsCollisionEnabled = false; };
            HasCollisionCheck.Children.Add(HasCollisionCheckbox);
            SettingsStack.Children.Add(HasCollisionCheck);

            Children.Add(SettingsStack);
        }

        public void ReloadSettings()
        {
            SettingsStack.Children.Clear();
            SettingsStack.Children.Add(CompTitle);
            SettingsStack.Children.Add(HasCollisionCheck);

            if (ParentComponent != null)
            {
                if (ParentComponent is Particle)
                {
                    CompTitle.Text = "Particle";

                    Grid radiusGrid = GetParticleRadiusGrid();
                    SettingsStack.Children.Add(radiusGrid);

                    Grid elasticityGrid = GetParticlePhysElasticity();
                    SettingsStack.Children.Add(elasticityGrid);
                    Grid frictionGrid = GetParticlePhysFriction();
                    SettingsStack.Children.Add(frictionGrid);
                    //Apply Force

                    //Color Change Rate
                }
                else if (ParentComponent is CompRectangle)
                {
                    CompTitle.Text = "Rectangle";

                    Grid sizeGrid = GetRectSizeInputGrid();
                    SettingsStack.Children.Add(sizeGrid);

                    Grid rotateGrid = GetRectRotateInputGrid();
                    SettingsStack.Children.Add(rotateGrid);

                    Grid hasOutlineGrid = GetRectHasOutlineGrid();
                    SettingsStack.Children.Add(hasOutlineGrid);
                }
                else if (ParentComponent is CompLine)
                {
                    CompTitle.Text = "Line";

                    //Thickness
                    Grid thicknessGrid = new Grid();
                    thicknessGrid.Margin = new Thickness(10, 20, 10, 20);
                    thicknessGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    thicknessGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    thicknessGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    
                    TextBlock thicknessLabel = new TextBlock();
                    Grid.SetColumn(thicknessLabel, 0);
                    Grid.SetColumnSpan(thicknessLabel, 2);
                    thicknessLabel.Text = "Line Thickness";
                    thicknessLabel.FontFamily = MainPage.GlobalFont;
                    thicknessLabel.FontSize = 16;
                    thicknessLabel.Foreground = new SolidColorBrush(Colors.White);
                    thicknessLabel.VerticalAlignment = VerticalAlignment.Center;
                    thicknessLabel.HorizontalAlignment = HorizontalAlignment.Center;
                    thicknessGrid.Children.Add(thicknessLabel);

                    TextBox thicknessInput = new TextBox();
                    Grid.SetColumn(thicknessInput, 2);
                    thicknessInput.MaxLength = 2;
                    thicknessInput.TextAlignment = TextAlignment.Center;
                    thicknessInput.TextWrapping = TextWrapping.Wrap;
                    thicknessInput.Text = ((CompLine)ParentComponent).Thickness + "";
                    thicknessInput.GotFocus += (object o, RoutedEventArgs e) => thicknessInput.SelectAll();
                    thicknessInput.BeforeTextChanging += ThicknessInput_BeforeTextChanging;
                    thicknessGrid.Children.Add(thicknessInput);

                    SettingsStack.Children.Add(thicknessGrid);
                }
                else if (ParentComponent is ParticleEjector)
                {
                    CompTitle.Text = "Ejector";

                    //Particle Limit
                    //Particle Rate
                    //Particle Radius
                    //Particle Elasticity
                    //Particle Friction
                    //Particle Radius Range
                    //Particle Scatter Angle

                    //Fill Color
                    //Color Change Rate

                    //Is fill color based on particle color
                    //Particle Fill Color
                    //Particle Color Change Rate
                }
            }


            ColorPicker = new HsvColorPicker(MenuWidth - 40);
                ColorPicker.SetColor(ParentComponent.FillBrush.Color);
            ColorPicker.ColorChanged += (s, o) =>
            {
                if (ParentComponent != null)
                    ParentComponent.FillBrush.Color = ColorPicker.PreviewColorBrush.Color;
            };
            SettingsStack.Children.Add(ColorPicker);

            if (ParentComponent != null)
            {
                HasCollisionCheckbox.IsChecked = ParentComponent.IsCollisionEnabled;
                if (ParentComponent is ParticleEjector)
                    HasCollisionCheck.Visibility = Visibility.Collapsed;
                else
                    HasCollisionCheck.Visibility = Visibility.Visible;
            }
        }


        private void ThicknessInput_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (double.TryParse(args.NewText, out double parsed))
            {
                if (parsed > 10)
                {
                    parsed = 10;
                    ((CompLine)ParentComponent).Thickness = parsed;
                    sender.Text = parsed + "";
                }
                else if (parsed < 1)
                {
                    parsed = 1;
                    ((CompLine)ParentComponent).Thickness = parsed;
                    sender.Text = parsed + "";
                }
                else
                    ((CompLine)ParentComponent).Thickness = parsed;
            }
            else
            {
                args.Cancel = true;
            }
        }


        private Grid GetRectSizeInputGrid()
        {
            Grid inputGrid = new Grid();
            inputGrid.Margin = new Thickness(0, 10, 0, 10);
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition());
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition());
            inputGrid.RowDefinitions.Add(new RowDefinition());
            inputGrid.RowDefinitions.Add(new RowDefinition());

            TextBlock widthLabel = new TextBlock();
            Grid.SetColumn(widthLabel, 0);
            Grid.SetRow(widthLabel, 0);
            widthLabel.Text = "Width";
            widthLabel.FontSize = 16;
            widthLabel.FontFamily = MainPage.GlobalFont;
            widthLabel.HorizontalAlignment = HorizontalAlignment.Center;
            widthLabel.VerticalAlignment = VerticalAlignment.Center;
            widthLabel.Foreground = new SolidColorBrush(Colors.White);
            inputGrid.Children.Add(widthLabel);

            TextBlock heightLabel = new TextBlock();
            Grid.SetColumn(heightLabel, 1);
            Grid.SetRow(heightLabel, 0);
            heightLabel.Text = "Height";
            heightLabel.FontSize = 16;
            heightLabel.FontFamily = MainPage.GlobalFont;
            heightLabel.HorizontalAlignment = HorizontalAlignment.Center;
            heightLabel.VerticalAlignment = VerticalAlignment.Center;
            heightLabel.Foreground = new SolidColorBrush(Colors.White);
            inputGrid.Children.Add(heightLabel);

            TextBox widthInput = new TextBox();
            Grid.SetColumn(widthInput, 0);
            Grid.SetRow(widthInput, 1);
            widthInput.Margin = new Thickness(10);
            widthInput.MaxLength = 4;
            widthInput.TextAlignment = TextAlignment.Center;
            widthInput.TextWrapping = TextWrapping.Wrap;
            widthInput.Text = ((CompRectangle)ParentComponent).Size.Width + "";
            widthInput.GotFocus += (object o, RoutedEventArgs e) => widthInput.SelectAll();
            widthInput.BeforeTextChanging += WidthInput_BeforeTextChanging;
            inputGrid.Children.Add(widthInput);

            TextBox heightInput = new TextBox();
            Grid.SetColumn(heightInput, 1);
            Grid.SetRow(heightInput, 1);
            heightInput.Margin = new Thickness(10);
            heightInput.MaxLength = 4;
            heightInput.TextAlignment = TextAlignment.Center;
            heightInput.TextWrapping = TextWrapping.Wrap;
            heightInput.Text = ((CompRectangle)ParentComponent).Size.Height + "";
            heightInput.GotFocus += (object o, RoutedEventArgs e) => heightInput.SelectAll();
            heightInput.BeforeTextChanging += HeightInput_BeforeTextChanging;
            inputGrid.Children.Add(heightInput);

            return inputGrid;
        }

        private void WidthInput_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (double.TryParse(args.NewText, out double parsed))
            {
                if (parsed > 1000)
                {
                    parsed = 1000;
                    ((CompRectangle)ParentComponent).Size = new Windows.Foundation.Size(parsed, ((CompRectangle)ParentComponent).Size.Height);
                    sender.Text = parsed + "";
                }
                else if (parsed < 1)
                {
                    parsed = 1;
                    ((CompRectangle)ParentComponent).Size = new Windows.Foundation.Size(parsed, ((CompRectangle)ParentComponent).Size.Height);
                    sender.Text = parsed + "";
                }
                else
                    ((CompRectangle)ParentComponent).Size = new Windows.Foundation.Size(parsed, ((CompRectangle)ParentComponent).Size.Height);
            }
            else
            {
                args.Cancel = true;
            }
        }
        private void HeightInput_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (double.TryParse(args.NewText, out double parsed))
            {
                if (parsed > 1000)
                {
                    parsed = 1000;
                    ((CompRectangle)ParentComponent).Size = new Windows.Foundation.Size(((CompRectangle)ParentComponent).Size.Width, parsed);
                    sender.Text = parsed + "";
                }
                else if (parsed < 1)
                {
                    parsed = 1;
                    ((CompRectangle)ParentComponent).Size = new Windows.Foundation.Size(((CompRectangle)ParentComponent).Size.Width, parsed);
                    sender.Text = parsed + "";
                }
                else
                    ((CompRectangle)ParentComponent).Size = new Windows.Foundation.Size(((CompRectangle)ParentComponent).Size.Width, parsed);
            }
            else
            {
                args.Cancel = true;
            }
        }

        private Grid GetRectRotateInputGrid()
        {
            Grid rotateGrid = new Grid();
            rotateGrid.Margin = new Thickness(10, 20, 10, 20);
            rotateGrid.ColumnDefinitions.Add(new ColumnDefinition());
            rotateGrid.ColumnDefinitions.Add(new ColumnDefinition());
            rotateGrid.ColumnDefinitions.Add(new ColumnDefinition());

            TextBlock rotateLabel = new TextBlock();
            Grid.SetColumn(rotateLabel, 0);
            Grid.SetColumnSpan(rotateLabel, 2);
            rotateLabel.Text = "Rotate°";
            rotateLabel.FontFamily = MainPage.GlobalFont;
            rotateLabel.FontSize = 16;
            rotateLabel.Foreground = new SolidColorBrush(Colors.White);
            rotateLabel.VerticalAlignment = VerticalAlignment.Center;
            rotateLabel.HorizontalAlignment = HorizontalAlignment.Center;
            rotateGrid.Children.Add(rotateLabel);

            TextBox rotateInput = new TextBox();
            Grid.SetColumn(rotateInput, 2);
            rotateInput.MaxLength = 2;
            rotateInput.TextAlignment = TextAlignment.Center;
            rotateInput.TextWrapping = TextWrapping.Wrap;
            rotateInput.Text = ((CompRectangle)ParentComponent).RotationAngle + "";
            rotateInput.GotFocus += (object o, RoutedEventArgs e) => rotateInput.SelectAll();
            rotateInput.BeforeTextChanging += RotateInput_BeforeTextChanging;
            rotateGrid.Children.Add(rotateInput);

            return rotateGrid;
        }

        private void RotateInput_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (double.TryParse(args.NewText, out double parsed))
            {
                if (parsed > 359)
                {
                    parsed = 359;
                    ((CompRectangle)ParentComponent).RotationAngle = parsed;
                    sender.Text = parsed + "";
                }
                else if (parsed < 0)
                {
                    parsed = 0;
                    ((CompRectangle)ParentComponent).RotationAngle = parsed;
                    sender.Text = parsed + "";
                }
                else
                    ((CompRectangle)ParentComponent).RotationAngle = parsed;
            }
            else
            {
                args.Cancel = true;
            }
        }

        private Grid GetRectHasOutlineGrid()
        {
            Grid outlineGrid = new Grid();
            outlineGrid.Margin = new Thickness(10);
            outlineGrid.ColumnDefinitions.Add(new ColumnDefinition());
            outlineGrid.ColumnDefinitions.Add(new ColumnDefinition());
            outlineGrid.ColumnDefinitions.Add(new ColumnDefinition());
            outlineGrid.ColumnDefinitions.Add(new ColumnDefinition());
            outlineGrid.ColumnDefinitions.Add(new ColumnDefinition());

            TextBlock hasOutlineLabel = new TextBlock();
            Grid.SetColumn(hasOutlineLabel, 0);
            Grid.SetColumnSpan(hasOutlineLabel, 3);
            hasOutlineLabel.Text = "Toggle Outline";
            hasOutlineLabel.FontSize = 14;
            hasOutlineLabel.FontFamily = MainPage.GlobalFont;
            hasOutlineLabel.Foreground = new SolidColorBrush(Colors.White);
            hasOutlineLabel.VerticalAlignment = VerticalAlignment.Center;
            hasOutlineLabel.HorizontalAlignment = HorizontalAlignment.Center;
            outlineGrid.Children.Add(hasOutlineLabel);

            CheckBox hasOutlineCheckbox = new CheckBox();
            Grid.SetColumn(hasOutlineCheckbox, 4);
            hasOutlineCheckbox.IsChecked = true;
            hasOutlineCheckbox.Checked += (s, o) => { ((CompRectangle)ParentComponent).StrokeThickness = 1.0; };
            hasOutlineCheckbox.Unchecked += (s, o) => { ((CompRectangle)ParentComponent).StrokeThickness = 0.0; };
            outlineGrid.Children.Add(hasOutlineCheckbox);

            return outlineGrid;
        }

        private Grid GetParticleRadiusGrid()
        {
            Grid radiusGrid = new Grid();
            radiusGrid.Margin = new Thickness(10, 5, 10, 5);
            radiusGrid.ColumnDefinitions.Add(new ColumnDefinition());
            radiusGrid.ColumnDefinitions.Add(new ColumnDefinition());
            radiusGrid.ColumnDefinitions.Add(new ColumnDefinition());

            TextBlock radiusLabel = new TextBlock();
            Grid.SetColumn(radiusLabel, 0);
            Grid.SetColumnSpan(radiusLabel, 2);
            radiusLabel.Text = "Radius";
            radiusLabel.FontFamily = MainPage.GlobalFont;
            radiusLabel.FontSize = 16;
            radiusLabel.Foreground = new SolidColorBrush(Colors.White);
            radiusLabel.VerticalAlignment = VerticalAlignment.Center;
            radiusLabel.HorizontalAlignment = HorizontalAlignment.Center;
            radiusGrid.Children.Add(radiusLabel);

            TextBox radiusInput = new TextBox();
            Grid.SetColumn(radiusInput, 2);
            radiusInput.MaxLength = 3;
            radiusInput.TextAlignment = TextAlignment.Center;
            radiusInput.TextWrapping = TextWrapping.Wrap;
            radiusInput.Text = ((Particle)ParentComponent).Radius + "";
            radiusInput.GotFocus += (object o, RoutedEventArgs e) => radiusInput.SelectAll();
            radiusInput.BeforeTextChanging += RadiusInput_BeforeTextChanging;
            radiusGrid.Children.Add(radiusInput);

            return radiusGrid;
        }
        private void RadiusInput_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (double.TryParse(args.NewText, out double parsed))
            {
                if (parsed > 30)
                {
                    parsed = 30;
                    ((Particle)ParentComponent).Radius = parsed;
                    sender.Text = parsed + "";
                }
                else if (parsed < 1)
                {
                    parsed = 1;
                    ((Particle)ParentComponent).Radius = parsed;
                    sender.Text = parsed + "";
                }
                else
                    ((Particle)ParentComponent).Radius = parsed;
            }
            else
            {
                args.Cancel = true;
            }
        }
        private Grid GetParticlePhysElasticity()
        {
            Grid elasticityGrid = new Grid();
            elasticityGrid.Margin = new Thickness(10, 5, 10, 5);
            elasticityGrid.ColumnDefinitions.Add(new ColumnDefinition());
            elasticityGrid.ColumnDefinitions.Add(new ColumnDefinition());
            elasticityGrid.ColumnDefinitions.Add(new ColumnDefinition());

            TextBlock elasticityLabel = new TextBlock();
            Grid.SetColumn(elasticityLabel, 0);
            Grid.SetColumnSpan(elasticityLabel, 2);
            elasticityLabel.Text = "Elasticity";
            elasticityLabel.FontFamily = MainPage.GlobalFont;
            elasticityLabel.FontSize = 16;
            elasticityLabel.Foreground = new SolidColorBrush(Colors.White);
            elasticityLabel.VerticalAlignment = VerticalAlignment.Center;
            elasticityLabel.HorizontalAlignment = HorizontalAlignment.Center;
            elasticityGrid.Children.Add(elasticityLabel);

            TextBox elasticityInput = new TextBox();
            Grid.SetColumn(elasticityInput, 2);
            elasticityInput.MaxLength = 5;
            elasticityInput.TextAlignment = TextAlignment.Center;
            elasticityInput.TextWrapping = TextWrapping.Wrap;
            elasticityInput.Text = ((Particle)ParentComponent).Phys.Elasticity + "";
            elasticityInput.GotFocus += (object o, RoutedEventArgs e) => elasticityInput.SelectAll();
            elasticityInput.BeforeTextChanging += ElasticityInput_BeforeTextChanging;
            elasticityGrid.Children.Add(elasticityInput);

            return elasticityGrid;
        }

        private void ElasticityInput_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (double.TryParse(args.NewText, out double parsed))
            {
                if (parsed > 1)
                {
                    parsed = 1;
                    ((Particle)ParentComponent).Phys.Elasticity = parsed;
                    sender.Text = parsed + "";
                }
                else if (parsed < 0)
                {
                    parsed = 0;
                    ((Particle)ParentComponent).Phys.Elasticity = parsed;
                    sender.Text = parsed + "";
                }
                else
                    ((Particle)ParentComponent).Phys.Elasticity = parsed;
            }
            else
            {
                args.Cancel = true;
            }
        }

        private Grid GetParticlePhysFriction()
        {
            Grid frictionGrid = new Grid();
            frictionGrid.Margin = new Thickness(10, 5, 10, 5);
            frictionGrid.ColumnDefinitions.Add(new ColumnDefinition());
            frictionGrid.ColumnDefinitions.Add(new ColumnDefinition());
            frictionGrid.ColumnDefinitions.Add(new ColumnDefinition());

            TextBlock frictionLabel = new TextBlock();
            Grid.SetColumn(frictionLabel, 0);
            Grid.SetColumnSpan(frictionLabel, 2);
            frictionLabel.Text = "Friction";
            frictionLabel.FontFamily = MainPage.GlobalFont;
            frictionLabel.FontSize = 16;
            frictionLabel.Foreground = new SolidColorBrush(Colors.White);
            frictionLabel.VerticalAlignment = VerticalAlignment.Center;
            frictionLabel.HorizontalAlignment = HorizontalAlignment.Center;
            frictionGrid.Children.Add(frictionLabel);

            TextBox frictionInput = new TextBox();
            Grid.SetColumn(frictionInput, 2);
            frictionInput.MaxLength = 5;
            frictionInput.TextAlignment = TextAlignment.Center;
            frictionInput.TextWrapping = TextWrapping.Wrap;
            frictionInput.Text = ((Particle)ParentComponent).Phys.Friction + "";
            frictionInput.GotFocus += (object o, RoutedEventArgs e) => frictionInput.SelectAll();
            frictionInput.BeforeTextChanging += FrictionInput_BeforeTextChanging;
            frictionGrid.Children.Add(frictionInput);

            return frictionGrid;
        }

        private void FrictionInput_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (double.TryParse(args.NewText, out double parsed))
            {
                if (parsed > 1)
                {
                    parsed = 1;
                    ((Particle)ParentComponent).Phys.Friction = parsed;
                    sender.Text = parsed + "";
                }
                else if (parsed < 0)
                {
                    parsed = 0;
                    ((Particle)ParentComponent).Phys.Friction = parsed;
                    sender.Text = parsed + "";
                }
                else
                    ((Particle)ParentComponent).Phys.Friction = parsed;
            }
            else
            {
                args.Cancel = true;
            }
        }

        public override void ToggleMenuExpanded()
        {
            base.ToggleMenuExpanded();

            if (!IsMenuExpanded)
                ParentComponent = null;
        }
    }
}