

using System;
using System.Linq;
using Windows.UI;
using Windows.UI.Core;
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

        public HsvColorPicker ColorPicker { get; set; }

        //Has collision check
        Grid HasCollisionCheck { get; set; }
        CheckBox HasCollisionCheckbox { get; set; }

        //Has particle collision check
        Grid HasParticleCollisionCheck { get; set; }
        SolidColorBrush HasParticleCollisionLabelFill { get; set; }
        CheckBox HasParticleCollisionCheckbox { get; set; }

        //Copy / Delete Component
        Grid CopyDeleteCompGrid { get; set; }

        //Particle Inputs and Settings
        //Particle Stop Velocity Button
        Grid StopVelocityGrid { get; set; }

        //Particle Properties Input
        Grid ParticlePropertiesGrid { get; set; }
        //Particle Radius Input
        Grid RadiusGrid { get; set; }
        //Particle Elasticity Input
        Grid ElasticityGrid { get; set; }
        //Particle Friction Input
        Grid FrictionGrid { get; set; }
        //Apply Force Grid
        Grid ApplyForceGrid { get; set; }
        Grid ColorChangeRateGrid {  get; set; }



        //Rectangle Inputs and Settings
        //Rectangle Size Input
        Grid SizeGrid { get; set; }
        Grid RotateGrid { get; set; }
        public TextBox RotateInput { get; set; }
        Grid HasOutlineGrid { get; set; }



        //Line Inputs And Settings
        Grid ThicknessGrid { get; set; }

        //Particle Ejector Inputs And Settings
        TextBlock ParticleColorTitle { get; set; }
        public HsvColorPicker EjectorParticleColorPicker { get; set; }
        Grid ParticleEjectGrid { get; set; }
        public TextBlock ParticlesEjectedValue {  get; set; }
        TextBox ParticleLimitInput { get; set; }
        Grid EjectorPropertiesGrid { get; set; }
        Grid EjectorParticleColorChangeRateGrid { get; set; }


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


            //Create All labels and inputs
            HasCollisionCheck = GetHasCollisionCheckGrid();

            //Particles
            HasParticleCollisionCheck = GetHasParticleCollisionCheckGrid();
            StopVelocityGrid = GetButton("Stop Velocity", Colors.White);
            StopVelocityGrid.Width = MenuWidth - 50;
            StopVelocityGrid.Tapped += (s, o) =>
            {
                if (ParentComponent != null && ParentComponent is Particle)
                    ((Particle)ParentComponent).Phys.Velocity = new Coord(0, 0);
            };
            ParticlePropertiesGrid = GetParticlePropertiesGrid();
            ApplyForceGrid = GetApplyForceGrid();
            ColorChangeRateGrid = GetColorChangeRateGrid();

            //Rectangles
            SizeGrid = GetRectSizeInputGrid();
            RotateGrid = GetRectRotateInputGrid();
            HasOutlineGrid = GetRectHasOutlineGrid();

            //Lines
            ThicknessGrid = GetLineThicknessGrid();

            //Particle Ejectors
            ParticleColorTitle = new TextBlock();
            ParticleColorTitle.Text = "Particle Color";
            ParticleColorTitle.FontSize = 24;
            ParticleColorTitle.FontFamily = MainPage.GlobalFont;
            ParticleColorTitle.Foreground = new SolidColorBrush(Colors.White);
            ParticleColorTitle.HorizontalAlignment = HorizontalAlignment.Center;
            ParticleEjectGrid = GetParticleEjectGrid();
            EjectorPropertiesGrid = GetEjectorPropertiesGrid();
            EjectorParticleColorChangeRateGrid = GetEjectorParticleColorChangeRateGrid();

            //Copy/Delete
            CopyDeleteCompGrid = GetCopyDeleteComponentGrid();


            //Color Pickers
            ColorPicker = new HsvColorPicker(MenuWidth - 40);
            ColorPicker.ColorChanged += (s, o) =>
            {
                if (ParentComponent != null)
                    ParentComponent.Fill = ColorPicker.PreviewColorBrush.Color;
            };

            EjectorParticleColorPicker = new HsvColorPicker(MenuWidth - 40);
            EjectorParticleColorPicker.ColorChanged += (s, o) =>
            {
                if (ParentComponent != null)
                    ((ParticleEjector)ParentComponent).ParticleColor = EjectorParticleColorPicker.PreviewColorBrush.Color;
            };

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

                    HasParticleCollisionCheckbox.IsChecked = ((Particle)ParentComponent).IsParticleCollisionEnabled;
                    SettingsStack.Children.Add(HasParticleCollisionCheck);

                    SettingsStack.Children.Add(StopVelocityGrid);

                    (ParticlePropertiesGrid.Children.Where(c => c is TextBox).ElementAt(0) as TextBox).Text = ((Particle)ParentComponent).Radius+"";
                    (ParticlePropertiesGrid.Children.Where(c => c is TextBox).ElementAt(1) as TextBox).Text = ((Particle)ParentComponent).Phys.Elasticity + "";
                    (ParticlePropertiesGrid.Children.Where(c => c is TextBox).ElementAt(2) as TextBox).Text = ((Particle)ParentComponent).Phys.Friction + "";
                    SettingsStack.Children.Add(ParticlePropertiesGrid);

                    (ApplyForceGrid.Children.Where(c => c is TextBox).ElementAt(0) as TextBox).Text = "0";
                    (ApplyForceGrid.Children.Where(c => c is TextBox).ElementAt(1) as TextBox).Text = "0";
                    SettingsStack.Children.Add(ApplyForceGrid);


                    (ColorChangeRateGrid.Children.Where(c => c is TextBox).First() as TextBox).Text = ((Particle)ParentComponent).ColorChangeRate + "";
                    SettingsStack.Children.Add(ColorChangeRateGrid);
                }
                else if (ParentComponent is CompRectangle)
                {
                    CompTitle.Text = "Rectangle";

                    (SizeGrid.Children.Where(c => c is TextBox).First() as TextBox).Text = ((CompRectangle)ParentComponent).Size.Width + "";
                    (SizeGrid.Children.Where(c => c is TextBox).ElementAt(1) as TextBox).Text = ((CompRectangle)ParentComponent).Size.Height + "";
                    SettingsStack.Children.Add(SizeGrid);

                    RotateInput.Text = ((CompRectangle)ParentComponent).RotationAngle + "";
                    SettingsStack.Children.Add(RotateGrid);

                    (HasOutlineGrid.Children.Where(c => c is CheckBox).First() as CheckBox).IsChecked = ((CompRectangle)ParentComponent).StrokeThickness != 0;
                    SettingsStack.Children.Add(HasOutlineGrid);
                }
                else if (ParentComponent is CompLine)
                {
                    CompTitle.Text = "Line";

                    (ThicknessGrid.Children.Where(c => c is TextBox).First() as TextBox).Text = ((CompLine)ParentComponent).Thickness + "";
                    SettingsStack.Children.Add(ThicknessGrid);
                }
                else if (ParentComponent is ParticleEjector)
                {
                    CompTitle.Text = "Ejector";

                    ParticlesEjectedValue.Text = ((ParticleEjector)ParentComponent).ParticlesEjected+"";
                    ParticleLimitInput.Text = ((ParticleEjector)ParentComponent).ParticleLimit + "";
                    SettingsStack.Children.Add(ParticleEjectGrid);

                    (ParticlePropertiesGrid.Children.Where(c => c is TextBox).ElementAt(0) as TextBox).Text = ((ParticleEjector)ParentComponent).ParticleRadius + "";
                    (ParticlePropertiesGrid.Children.Where(c => c is TextBox).ElementAt(1) as TextBox).Text = ((ParticleEjector)ParentComponent).ParticleElasticity + "";
                    (ParticlePropertiesGrid.Children.Where(c => c is TextBox).ElementAt(2) as TextBox).Text = ((ParticleEjector)ParentComponent).ParticleFriction + "";
                    SettingsStack.Children.Add(ParticlePropertiesGrid);

                    (EjectorPropertiesGrid.Children.Where(c => c is TextBox).ElementAt(0) as TextBox).Text = ((ParticleEjector)ParentComponent).ParticleRate + "";
                    (EjectorPropertiesGrid.Children.Where(c => c is TextBox).ElementAt(1) as TextBox).Text = ((ParticleEjector)ParentComponent).ParticleRadiusRange + "";
                    (EjectorPropertiesGrid.Children.Where(c => c is TextBox).ElementAt(2) as TextBox).Text = ((ParticleEjector)ParentComponent).ParticleScatterAngle + "";
                    SettingsStack.Children.Add(EjectorPropertiesGrid);

                    RotateInput.Text = ((ParticleEjector)ParentComponent).RotationAngle + "";
                    SettingsStack.Children.Add(RotateGrid);

                    (ColorChangeRateGrid.Children.Where(c => c is TextBox).First() as TextBox).Text = ((ParticleEjector)ParentComponent).ColorChangeRate + "";
                    SettingsStack.Children.Add(ColorChangeRateGrid);

                    (EjectorParticleColorChangeRateGrid.Children.Where(c => c is TextBox).First() as TextBox).Text = ((ParticleEjector)ParentComponent).ParticleColorChangeRate + "";
                    SettingsStack.Children.Add(EjectorParticleColorChangeRateGrid);
                }
            }


            //Add Color Picker
            if (!(ParentComponent is ParticleEjector))
            {
                ColorPicker.SetColor(ParentComponent.Fill);
                SettingsStack.Children.Add(ColorPicker);
            }

            //If component is a particle ejector, add additional colorPicker
            if (ParentComponent != null && ParentComponent is ParticleEjector)
            {
                SettingsStack.Children.Add(ParticleColorTitle);
                EjectorParticleColorPicker.SetColor(((ParticleEjector)ParentComponent).ParticleColor);
                SettingsStack.Children.Add(EjectorParticleColorPicker);
            }


            //Add Copy Delete Buttons
            SettingsStack.Children.Add(CopyDeleteCompGrid);

            if (ParentComponent != null)
            {
                HasCollisionCheckbox.IsChecked = ParentComponent.IsCollisionEnabled;
                if (ParentComponent is ParticleEjector)
                    HasCollisionCheck.Visibility = Visibility.Collapsed;
                else
                    HasCollisionCheck.Visibility = Visibility.Visible;
            }
        }


        //Collision Grid Functions
        private Grid GetHasCollisionCheckGrid()
        {
            Grid hasCollisionCheck = new Grid();
            hasCollisionCheck.Margin = new Thickness(10);

            TextBlock hasCollisionLabel = new TextBlock();
            Grid.SetColumn(hasCollisionLabel, 0);
            Grid.SetColumnSpan(hasCollisionLabel, 3);
            hasCollisionLabel.Text = "Toggle Collision";
            hasCollisionLabel.FontSize = 14;
            hasCollisionLabel.FontFamily = MainPage.GlobalFont;
            hasCollisionLabel.Foreground = new SolidColorBrush(Colors.White);
            hasCollisionLabel.VerticalAlignment = VerticalAlignment.Center;
            hasCollisionLabel.HorizontalAlignment = HorizontalAlignment.Center;
            hasCollisionCheck.Children.Add(hasCollisionLabel);

            HasCollisionCheckbox = new CheckBox();
            Grid.SetColumn(HasCollisionCheckbox, 4);
            HasCollisionCheckbox.IsChecked = true;
            HasCollisionCheckbox.Checked += (s, o) =>
            {
                ParentComponent.IsCollisionEnabled = true;

                if (HasParticleCollisionLabelFill != null)
                    HasParticleCollisionLabelFill.Color = Colors.White;
                if (HasParticleCollisionCheckbox != null)
                    HasParticleCollisionCheckbox.IsEnabled = true;
            };
            HasCollisionCheckbox.Unchecked += (s, o) =>
            {
                ParentComponent.IsCollisionEnabled = false;

                if (HasParticleCollisionLabelFill != null)
                    HasParticleCollisionLabelFill.Color = Colors.LightGray;
                if (HasParticleCollisionCheckbox != null)
                    HasParticleCollisionCheckbox.IsEnabled = false;
            };
            hasCollisionCheck.Children.Add(HasCollisionCheckbox);

            return hasCollisionCheck;
        }
        private Grid GetHasParticleCollisionCheckGrid()
        {
            Grid hasParticleCollision = new Grid();
            hasParticleCollision.Margin = new Thickness(10);

            TextBlock hasCollisionLabel = new TextBlock();
            Grid.SetColumn(hasCollisionLabel, 0);
            Grid.SetColumnSpan(hasCollisionLabel, 3);
            hasCollisionLabel.Text = "Toggle Particle Collision";
            hasCollisionLabel.FontSize = 14;
            hasCollisionLabel.FontFamily = MainPage.GlobalFont;
            HasParticleCollisionLabelFill = new SolidColorBrush(Colors.White);
            hasCollisionLabel.Foreground = HasParticleCollisionLabelFill;
            hasCollisionLabel.VerticalAlignment = VerticalAlignment.Center;
            hasCollisionLabel.HorizontalAlignment = HorizontalAlignment.Center;
            hasParticleCollision.Children.Add(hasCollisionLabel);

            HasParticleCollisionCheckbox = new CheckBox();
            Grid.SetColumn(HasParticleCollisionCheckbox, 4);
            HasParticleCollisionCheckbox.IsChecked = true;
            HasParticleCollisionCheckbox.Checked += (s, o) => { ((Particle)ParentComponent).IsParticleCollisionEnabled = true; };
            HasParticleCollisionCheckbox.Unchecked += (s, o) => { ((Particle)ParentComponent).IsParticleCollisionEnabled = false; };
            hasParticleCollision.Children.Add(HasParticleCollisionCheckbox);

            return hasParticleCollision;
        }


        //Partilce Grid Functions
        private Grid GetParticlePropertiesGrid()
        {
            Grid propGrid = new Grid();
            propGrid.ColumnDefinitions.Add(new ColumnDefinition());
            propGrid.ColumnDefinitions.Add(new ColumnDefinition());
            propGrid.ColumnDefinitions.Add(new ColumnDefinition());
            propGrid.RowDefinitions.Add(new RowDefinition());
            propGrid.RowDefinitions.Add(new RowDefinition());

            //Radius
            TextBlock radiusLabel = new TextBlock();
            Grid.SetColumn(radiusLabel, 0);
            Grid.SetRow(radiusLabel, 0);
            radiusLabel.Text = "Radius";
            radiusLabel.FontFamily = MainPage.GlobalFont;
            radiusLabel.FontSize = 16;
            radiusLabel.Foreground = new SolidColorBrush(Colors.White);
            radiusLabel.VerticalAlignment = VerticalAlignment.Center;
            radiusLabel.HorizontalAlignment = HorizontalAlignment.Center;
            propGrid.Children.Add(radiusLabel);

            TextBox radiusInput = new TextBox();
            Grid.SetColumn(radiusInput, 0);
            Grid.SetRow(radiusInput, 1);
            radiusInput.MaxLength = 3;
            radiusInput.TextAlignment = TextAlignment.Center;
            radiusInput.TextWrapping = TextWrapping.Wrap;
            radiusInput.GotFocus += (object o, RoutedEventArgs e) => radiusInput.SelectAll();
            radiusInput.BeforeTextChanging += RadiusInput_BeforeTextChanging;
            propGrid.Children.Add(radiusInput);

            //Elasticity
            TextBlock elasticityLabel = new TextBlock();
            Grid.SetColumn(elasticityLabel, 1);
            Grid.SetRow(elasticityLabel, 0);
            elasticityLabel.Text = "Elasticity";
            elasticityLabel.FontFamily = MainPage.GlobalFont;
            elasticityLabel.FontSize = 16;
            elasticityLabel.Foreground = new SolidColorBrush(Colors.White);
            elasticityLabel.VerticalAlignment = VerticalAlignment.Center;
            elasticityLabel.HorizontalAlignment = HorizontalAlignment.Center;
            propGrid.Children.Add(elasticityLabel);

            TextBox elasticityInput = new TextBox();
            Grid.SetColumn(elasticityInput, 1);
            Grid.SetRow(elasticityInput, 1);
            elasticityInput.MaxLength = 5;
            elasticityInput.TextAlignment = TextAlignment.Center;
            elasticityInput.TextWrapping = TextWrapping.Wrap;
            elasticityInput.GotFocus += (object o, RoutedEventArgs e) => elasticityInput.SelectAll();
            elasticityInput.BeforeTextChanging += ElasticityInput_BeforeTextChanging;
            propGrid.Children.Add(elasticityInput);

            //Friction
            TextBlock frictionLabel = new TextBlock();
            Grid.SetColumn(frictionLabel, 2);
            Grid.SetRow(frictionLabel, 0);
            frictionLabel.Text = "Friction";
            frictionLabel.FontFamily = MainPage.GlobalFont;
            frictionLabel.FontSize = 16;
            frictionLabel.Foreground = new SolidColorBrush(Colors.White);
            frictionLabel.VerticalAlignment = VerticalAlignment.Center;
            frictionLabel.HorizontalAlignment = HorizontalAlignment.Center;
            propGrid.Children.Add(frictionLabel);

            TextBox frictionInput = new TextBox();
            Grid.SetColumn(frictionInput, 2);
            Grid.SetRow(frictionInput, 1);
            frictionInput.MaxLength = 5;
            frictionInput.TextAlignment = TextAlignment.Center;
            frictionInput.TextWrapping = TextWrapping.Wrap;
            frictionInput.GotFocus += (object o, RoutedEventArgs e) => frictionInput.SelectAll();
            frictionInput.BeforeTextChanging += FrictionInput_BeforeTextChanging;
            propGrid.Children.Add(frictionInput);

            return propGrid;
        }
        private void RadiusInput_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (double.TryParse(args.NewText, out double parsed))
            {
                if (parsed > 30)
                {
                    parsed = 30;
                    if (ParentComponent is Particle)
                        ((Particle)ParentComponent).Radius = parsed;
                    else if (ParentComponent is ParticleEjector)
                        ((ParticleEjector)ParentComponent).ParticleRadius = parsed;
                    sender.Text = parsed + "";
                }
                else if (parsed < 1)
                {
                    parsed = 1;
                    if (ParentComponent is Particle)
                        ((Particle)ParentComponent).Radius = parsed;
                    else if (ParentComponent is ParticleEjector)
                        ((ParticleEjector)ParentComponent).ParticleRadius = parsed;
                    sender.Text = parsed + "";
                }
                else
                {
                    if (ParentComponent is Particle)
                        ((Particle)ParentComponent).Radius = parsed;
                    else if (ParentComponent is ParticleEjector)
                        ((ParticleEjector)ParentComponent).ParticleRadius = parsed;
                }
            }
            else
            {
                args.Cancel = true;
            }
        }
        private void ElasticityInput_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (double.TryParse(args.NewText, out double parsed))
            {
                if (parsed > 1)
                {
                    parsed = 1;
                    if (ParentComponent is Particle)
                        ((Particle)ParentComponent).Phys.Elasticity = parsed;
                    else if (ParentComponent is ParticleEjector)
                        ((ParticleEjector)ParentComponent).ParticleElasticity = parsed;
                    sender.Text = parsed + "";
                }
                else if (parsed < 0)
                {
                    parsed = 0;
                    if (ParentComponent is Particle)
                        ((Particle)ParentComponent).Phys.Elasticity = parsed;
                    else if (ParentComponent is ParticleEjector)
                        ((ParticleEjector)ParentComponent).ParticleElasticity = parsed;
                    sender.Text = parsed + "";
                }
                else
                {
                    if (ParentComponent is Particle)
                        ((Particle)ParentComponent).Phys.Elasticity = parsed;
                    else if (ParentComponent is ParticleEjector)
                        ((ParticleEjector)ParentComponent).ParticleElasticity = parsed;
                }
            }
            else
            {
                args.Cancel = true;
            }
        }
        private void FrictionInput_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (double.TryParse(args.NewText, out double parsed))
            {
                if (parsed > 1)
                {
                    parsed = 1;
                    if (ParentComponent is Particle)
                        ((Particle)ParentComponent).Phys.Friction = parsed;
                    else if (ParentComponent is ParticleEjector)
                        ((ParticleEjector)ParentComponent).ParticleFriction = parsed;
                    sender.Text = parsed + "";
                }
                else if (parsed < 0)
                {
                    parsed = 0;
                    if (ParentComponent is Particle)
                        ((Particle)ParentComponent).Phys.Friction = parsed;
                    else if (ParentComponent is ParticleEjector)
                        ((ParticleEjector)ParentComponent).ParticleFriction = parsed;
                    sender.Text = parsed + "";
                }
                else
                {
                    if (ParentComponent is Particle)
                        ((Particle)ParentComponent).Phys.Friction = parsed;
                    else if (ParentComponent is ParticleEjector)
                        ((ParticleEjector)ParentComponent).ParticleFriction = parsed;
                }
            }
            else
            {
                args.Cancel = true;
            }
        }
        private Grid GetApplyForceGrid()
        {
            Grid applyForceGrid = new Grid();
            applyForceGrid.ColumnDefinitions.Add(new ColumnDefinition());
            applyForceGrid.ColumnDefinitions.Add(new ColumnDefinition());
            applyForceGrid.ColumnDefinitions.Add(new ColumnDefinition());
            applyForceGrid.ColumnDefinitions.Add(new ColumnDefinition());
            applyForceGrid.RowDefinitions.Add(new RowDefinition());
            applyForceGrid.RowDefinitions.Add(new RowDefinition());
            applyForceGrid.Margin = new Thickness(0, 20, 0, 10);

            //Force Label
            TextBlock forceLabel = new TextBlock();
            Grid.SetColumn(forceLabel, 0);
            Grid.SetRow(forceLabel, 0);
            forceLabel.Text = "Force";
            forceLabel.FontSize = 14;
            forceLabel.FontFamily = MainPage.GlobalFont;
            forceLabel.HorizontalAlignment = HorizontalAlignment.Center;
            forceLabel.VerticalAlignment = VerticalAlignment.Center;
            forceLabel.Foreground = new SolidColorBrush(Colors.White);
            applyForceGrid.Children.Add(forceLabel);

            //Angle Label
            TextBlock angleLabel = new TextBlock();
            Grid.SetColumn(angleLabel, 1);
            Grid.SetRow(angleLabel, 0);
            angleLabel.Text = "Angle";
            angleLabel.FontSize = 14;
            angleLabel.FontFamily = MainPage.GlobalFont;
            angleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            angleLabel.VerticalAlignment = VerticalAlignment.Center;
            angleLabel.Foreground = new SolidColorBrush(Colors.White);
            applyForceGrid.Children.Add(angleLabel);

            //Force Input
            TextBox forceInput = new TextBox();
            Grid.SetColumn(forceInput, 0);
            Grid.SetRow(forceInput, 1);
            forceInput.Margin = new Thickness(10);
            forceInput.MaxLength = 5;
            forceInput.Text = "0";
            forceInput.TextAlignment = TextAlignment.Center;
            forceInput.TextWrapping = TextWrapping.Wrap;
            forceInput.GotFocus += (object o, RoutedEventArgs e) => forceInput.SelectAll();
            forceInput.BeforeTextChanging += ForceInput_BeforeTextChanging;
            applyForceGrid.Children.Add(forceInput);

            //Angle Input
            TextBox angleInput = new TextBox();
            Grid.SetColumn(angleInput, 1);
            Grid.SetRow(angleInput, 1);
            angleInput.Margin = new Thickness(10);
            angleInput.MaxLength = 5;
            angleInput.Text = "0";
            angleInput.TextAlignment = TextAlignment.Center;
            angleInput.TextWrapping = TextWrapping.Wrap;
            angleInput.GotFocus += (object o, RoutedEventArgs e) => angleInput.SelectAll();
            angleInput.BeforeTextChanging += AngleInput_BeforeTextChanging;
            applyForceGrid.Children.Add(angleInput);


            //Apply Force Button
            Grid applyForceButton = GetButton("ApplyForce", Colors.White);
            Grid.SetColumn(applyForceButton, 2);
            Grid.SetColumnSpan(applyForceButton, 2);
            Grid.SetRow(applyForceButton, 1);
            applyForceButton.Tapped += (s, o) =>
            {
                if (ParentComponent != null && ParentComponent is Particle)
                    ((Particle)ParentComponent).Phys.ApplyForce(new Coord(
                        Math.Cos(double.Parse(angleInput.Text) * Math.PI / 180.0) * double.Parse(forceInput.Text),
                        Math.Sin(-double.Parse(angleInput.Text) * Math.PI / 180.0) * double.Parse(forceInput.Text)
                    ));
            };

            applyForceGrid.Children.Add(applyForceButton);

            return applyForceGrid;
        }
        private void ForceInput_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (double.TryParse(args.NewText, out double parsed))
            {
                if (parsed > 1000)
                {
                    parsed = 1000;
                    sender.Text = parsed + "";
                }
                else if (parsed < 0)
                {
                    parsed = 0;
                    sender.Text = parsed + "";
                }
            }
            else
            {
                args.Cancel = true;
            }
        }
        private void AngleInput_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (double.TryParse(args.NewText, out double parsed))
            {
                if (parsed > 359)
                {
                    parsed = 359;
                    sender.Text = parsed + "";
                }
                else if (parsed < 0)
                {
                    parsed = 0;
                    sender.Text = parsed + "";
                }
            }
            else
            {
                args.Cancel = true;
            }
        }
        private Grid GetColorChangeRateGrid()
        {
            Grid colorRateGrid = new Grid();
            colorRateGrid.Margin = new Thickness(10, 20, 10, 5);
            colorRateGrid.ColumnDefinitions.Add(new ColumnDefinition());
            colorRateGrid.ColumnDefinitions.Add(new ColumnDefinition());
            colorRateGrid.ColumnDefinitions.Add(new ColumnDefinition());

            TextBlock colorRateLabel = new TextBlock();
            Grid.SetColumn(colorRateLabel, 0);
            Grid.SetColumnSpan(colorRateLabel, 2);
            colorRateLabel.Text = "Color Change";
            colorRateLabel.FontFamily = MainPage.GlobalFont;
            colorRateLabel.FontSize = 16;
            colorRateLabel.Foreground = new SolidColorBrush(Colors.White);
            colorRateLabel.VerticalAlignment = VerticalAlignment.Center;
            colorRateLabel.HorizontalAlignment = HorizontalAlignment.Center;
            colorRateGrid.Children.Add(colorRateLabel);

            TextBox colorChangeInput = new TextBox();
            Grid.SetColumn(colorChangeInput, 2);
            colorChangeInput.MaxLength = 3;
            colorChangeInput.TextAlignment = TextAlignment.Center;
            colorChangeInput.TextWrapping = TextWrapping.Wrap;
            colorChangeInput.GotFocus += (object o, RoutedEventArgs e) => colorChangeInput.SelectAll();
            colorChangeInput.BeforeTextChanging += ColorChangeInput_BeforeTextChanging;
            colorRateGrid.Children.Add(colorChangeInput);

            return colorRateGrid;
        }
        private void ColorChangeInput_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (double.TryParse(args.NewText, out double parsed))
            {
                if (parsed > 360)
                {
                    parsed = 360;
                    if (ParentComponent is Particle)
                        ((Particle)ParentComponent).ColorChangeRate = parsed;
                    else if (ParentComponent is ParticleEjector)
                        ((ParticleEjector)ParentComponent).ColorChangeRate = parsed;
                    sender.Text = parsed + "";
                }
                else if (parsed < 0)
                {
                    parsed = 0;
                    if (ParentComponent is Particle)
                        ((Particle)ParentComponent).ColorChangeRate = parsed;
                    else if (ParentComponent is ParticleEjector)
                        ((ParticleEjector)ParentComponent).ColorChangeRate = parsed;
                    sender.Text = parsed + "";
                }
                else
                {
                    if (ParentComponent is Particle)
                        ((Particle)ParentComponent).ColorChangeRate = parsed;
                    else if (ParentComponent is ParticleEjector)
                        ((ParticleEjector)ParentComponent).ColorChangeRate = parsed;
                }
            }
            else
            {
                args.Cancel = true;
            }
        }


        //Rectangle Grid Functions
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
            rotateGrid.Margin = new Thickness(10, 10, 10, 10);
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

            RotateInput = new TextBox();
            Grid.SetColumn(RotateInput, 2);
            RotateInput.MaxLength = 3;
            RotateInput.TextAlignment = TextAlignment.Center;
            RotateInput.TextWrapping = TextWrapping.Wrap;
            RotateInput.GotFocus += (object o, RoutedEventArgs e) => RotateInput.SelectAll();
            RotateInput.BeforeTextChanging += RotateInput_BeforeTextChanging;
            rotateGrid.Children.Add(RotateInput);

            return rotateGrid;
        }

        private void RotateInput_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (double.TryParse(args.NewText, out double parsed))
            {
                if (parsed > 359)
                {
                    parsed = 359;
                    if (ParentComponent is CompRectangle)
                        ((CompRectangle)ParentComponent).RotationAngle = parsed;
                    else if (ParentComponent is ParticleEjector)
                        ((ParticleEjector)ParentComponent).RotationAngle = parsed;
                    sender.Text = parsed + "";
                }
                else if (parsed < 0)
                {
                    parsed = 0;
                    if (ParentComponent is CompRectangle)
                        ((CompRectangle)ParentComponent).RotationAngle = parsed;
                    else if (ParentComponent is ParticleEjector)
                        ((ParticleEjector)ParentComponent).RotationAngle = parsed;
                    sender.Text = parsed + "";
                }
                else
                {
                    if (ParentComponent is CompRectangle)
                        ((CompRectangle)ParentComponent).RotationAngle = parsed;
                    else if (ParentComponent is ParticleEjector)
                        ((ParticleEjector)ParentComponent).RotationAngle = parsed;
                }
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
            hasOutlineCheckbox.Checked += (s, o) => { ((CompRectangle)ParentComponent).StrokeThickness = 2.0; };
            hasOutlineCheckbox.Unchecked += (s, o) => { ((CompRectangle)ParentComponent).StrokeThickness = 0.0; };
            outlineGrid.Children.Add(hasOutlineCheckbox);

            return outlineGrid;
        }


        //Line Grid Functions
        private Grid GetLineThicknessGrid()
        {
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
            thicknessInput.GotFocus += (object o, RoutedEventArgs e) => thicknessInput.SelectAll();
            thicknessInput.BeforeTextChanging += ThicknessInput_BeforeTextChanging;
            thicknessGrid.Children.Add(thicknessInput);

            return thicknessGrid;

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


        //Particle Ejector Grid Functions
        private Grid GetParticleEjectGrid()
        {
            Grid ejectGrid = new Grid();
            ejectGrid.ColumnDefinitions.Add(new ColumnDefinition());
            ejectGrid.ColumnDefinitions.Add(new ColumnDefinition());
            ejectGrid.ColumnDefinitions.Add(new ColumnDefinition());
            ejectGrid.RowDefinitions.Add(new RowDefinition());
            ejectGrid.RowDefinitions.Add(new RowDefinition());

            //Particle Limit Label
            TextBlock limitLabel = new TextBlock();
            Grid.SetColumn(limitLabel, 0);
            Grid.SetRow(limitLabel, 0);
            limitLabel.Text = "Limit:";
            limitLabel.FontFamily = MainPage.GlobalFont;
            limitLabel.FontSize = 14;
            limitLabel.Foreground = new SolidColorBrush(Colors.White);
            limitLabel.VerticalAlignment = VerticalAlignment.Center;
            limitLabel.HorizontalAlignment = HorizontalAlignment.Center;
            ejectGrid.Children.Add(limitLabel);

            //Particles Ejected Label
            TextBlock ejectedLabel = new TextBlock();
            Grid.SetColumn(ejectedLabel, 0);
            Grid.SetRow(ejectedLabel, 1);
            ejectedLabel.Text = "Ejected:";
            ejectedLabel.FontFamily = MainPage.GlobalFont;
            ejectedLabel.FontSize = 14;
            ejectedLabel.Foreground = new SolidColorBrush(Colors.White);
            ejectedLabel.VerticalAlignment = VerticalAlignment.Center;
            ejectedLabel.HorizontalAlignment = HorizontalAlignment.Center;
            ejectGrid.Children.Add(ejectedLabel);

            //Particle Limit Input
            ParticleLimitInput = new TextBox();
            Grid.SetColumn(ParticleLimitInput, 1);
            Grid.SetRow(ParticleLimitInput, 0);
            ParticleLimitInput.MaxLength = 5;
            ParticleLimitInput.Margin = new Thickness(10);
            ParticleLimitInput.TextAlignment = TextAlignment.Center;
            ParticleLimitInput.TextWrapping = TextWrapping.Wrap;
            ParticleLimitInput.GotFocus += (object o, RoutedEventArgs e) => ParticleLimitInput.SelectAll();
            ParticleLimitInput.BeforeTextChanging += LimitInput_BeforeTextChanging;
            ejectGrid.Children.Add(ParticleLimitInput);

            //Particles Ejected Value
            ParticlesEjectedValue = new TextBlock();
            Grid.SetColumn(ParticlesEjectedValue, 1);
            Grid.SetRow(ParticlesEjectedValue, 1);
            ParticlesEjectedValue.FontFamily = MainPage.GlobalFont;
            ParticlesEjectedValue.FontSize = 12;
            ParticlesEjectedValue.Foreground = new SolidColorBrush(Colors.White);
            ParticlesEjectedValue.VerticalAlignment = VerticalAlignment.Center;
            ParticlesEjectedValue.HorizontalAlignment = HorizontalAlignment.Center;
            ejectGrid.Children.Add(ParticlesEjectedValue);

            //Eject Particle Button
            Grid ejectButton = GetButton("Eject", Colors.White);
            Grid.SetColumn(ejectButton, 2);
            Grid.SetRow(ejectButton, 0);
            ejectButton.Tapped += (s, o) =>
            {
                if (ParentComponent != null && ParentComponent is ParticleEjector)
                    ((ParticleEjector)ParentComponent).EjectParticle();
            };
            ejectGrid.Children.Add(ejectButton);

            //Reset Particles Ejected Button
            Grid resetButton = GetButton("Reset", Colors.White);
            Grid.SetColumn(resetButton, 2);
            Grid.SetRow(resetButton, 1);
            resetButton.Tapped += (s, o) =>
            {
                if (ParentComponent != null && ParentComponent is ParticleEjector)
                    ((ParticleEjector)ParentComponent).ResetParticlesEjected();
            };
            ejectGrid.Children.Add(resetButton);

            return ejectGrid;
        }
        private void LimitInput_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (int.TryParse(args.NewText, out int parsed))
            {
                if (parsed > 10000)
                {
                    parsed = 10000;
                    ((ParticleEjector)ParentComponent).ParticleLimit = parsed;
                    sender.Text = parsed + "";
                }
                else if (parsed < 1)
                {
                    parsed = 1;
                    ((ParticleEjector)ParentComponent).ParticleLimit = parsed;
                    sender.Text = parsed + "";
                }
                else
                    ((ParticleEjector)ParentComponent).ParticleLimit = parsed;
            }
            else
            {
                args.Cancel = true;
            }
        }

        private Grid GetEjectorPropertiesGrid()
        {
            Grid propGrid = new Grid();
            propGrid.Margin = new Thickness(0, 20, 0, 20);
            propGrid.ColumnDefinitions.Add(new ColumnDefinition());
            propGrid.ColumnDefinitions.Add(new ColumnDefinition());
            propGrid.ColumnDefinitions.Add(new ColumnDefinition());
            propGrid.RowDefinitions.Add(new RowDefinition());
            propGrid.RowDefinitions.Add(new RowDefinition());


            //Particle Rate
            TextBlock rateLabel = new TextBlock();
            Grid.SetColumn(rateLabel, 0);
            Grid.SetRow(rateLabel, 0);
            rateLabel.Text = "Eject Rate";
            rateLabel.FontFamily = MainPage.GlobalFont;
            rateLabel.FontSize = 11;
            rateLabel.Foreground = new SolidColorBrush(Colors.White);
            rateLabel.VerticalAlignment = VerticalAlignment.Center;
            rateLabel.HorizontalAlignment = HorizontalAlignment.Center;
            propGrid.Children.Add(rateLabel);

            TextBox rateInput = new TextBox();
            Grid.SetColumn(rateInput, 0);
            Grid.SetRow(rateInput, 1);
            rateInput.MaxLength = 2;
            rateInput.TextAlignment = TextAlignment.Center;
            rateInput.TextWrapping = TextWrapping.Wrap;
            rateInput.GotFocus += (object o, RoutedEventArgs e) => rateInput.SelectAll();
            rateInput.BeforeTextChanging += RateInput_BeforeTextChanging;
            propGrid.Children.Add(rateInput);

            //Radius Range
            TextBlock radiusRangeLabel = new TextBlock();
            Grid.SetColumn(radiusRangeLabel, 1);
            Grid.SetRow(radiusRangeLabel, 0);
            radiusRangeLabel.Text = "Radius Range";
            radiusRangeLabel.FontFamily = MainPage.GlobalFont;
            radiusRangeLabel.FontSize = 11;
            radiusRangeLabel.Foreground = new SolidColorBrush(Colors.White);
            radiusRangeLabel.VerticalAlignment = VerticalAlignment.Center;
            radiusRangeLabel.HorizontalAlignment = HorizontalAlignment.Center;
            propGrid.Children.Add(radiusRangeLabel);

            TextBox radiusRangelInput = new TextBox();
            Grid.SetColumn(radiusRangelInput, 1);
            Grid.SetRow(radiusRangelInput, 1);
            radiusRangelInput.MaxLength = 3;
            radiusRangelInput.TextAlignment = TextAlignment.Center;
            radiusRangelInput.TextWrapping = TextWrapping.Wrap;
            radiusRangelInput.GotFocus += (object o, RoutedEventArgs e) => radiusRangelInput.SelectAll();
            radiusRangelInput.BeforeTextChanging += RadiusRangelInput_BeforeTextChanging;
            propGrid.Children.Add(radiusRangelInput);

            //Scatter Range
            TextBlock scatterLabel = new TextBlock();
            Grid.SetColumn(scatterLabel, 2);
            Grid.SetRow(scatterLabel, 0);
            scatterLabel.Text = "Scatter Range°";
            scatterLabel.FontFamily = MainPage.GlobalFont;
            scatterLabel.FontSize = 11;
            scatterLabel.Foreground = new SolidColorBrush(Colors.White);
            scatterLabel.VerticalAlignment = VerticalAlignment.Center;
            scatterLabel.HorizontalAlignment = HorizontalAlignment.Center;
            propGrid.Children.Add(scatterLabel);

            TextBox scatterInput = new TextBox();
            Grid.SetColumn(scatterInput, 2);
            Grid.SetRow(scatterInput, 1);
            scatterInput.MaxLength = 3;
            scatterInput.TextAlignment = TextAlignment.Center;
            scatterInput.TextWrapping = TextWrapping.Wrap;
            scatterInput.GotFocus += (object o, RoutedEventArgs e) => scatterInput.SelectAll();
            scatterInput.BeforeTextChanging += ScatterInput_BeforeTextChanging;
            propGrid.Children.Add(scatterInput);

            return propGrid;
        }
        private void RateInput_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (double.TryParse(args.NewText, out double parsed))
            {
                if (parsed > 10)
                {
                    parsed = 10;
                    ((ParticleEjector)ParentComponent).ParticleRate = parsed;
                    sender.Text = parsed + "";
                }
                else if (parsed < 0)
                {
                    parsed = 0;
                    ((ParticleEjector)ParentComponent).ParticleRate = parsed;
                    sender.Text = parsed + "";
                }
                else
                {
                    ((ParticleEjector)ParentComponent).ParticleRate = parsed;
                }
            }
            else
            {
                args.Cancel = true;
            }
        }
        private void RadiusRangelInput_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (double.TryParse(args.NewText, out double parsed))
            {
                if (parsed > 20)
                {
                    parsed = 20;
                    ((ParticleEjector)ParentComponent).ParticleRadiusRange = parsed;
                    sender.Text = parsed + "";
                }
                else if (parsed < 0)
                {
                    parsed = 0;
                    ((ParticleEjector)ParentComponent).ParticleRadiusRange = parsed;
                    sender.Text = parsed + "";
                }
                else
                {
                    ((ParticleEjector)ParentComponent).ParticleRadiusRange = parsed;
                }
            }
            else
            {
                args.Cancel = true;
            }
        }
        private void ScatterInput_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (double.TryParse(args.NewText, out double parsed))
            {
                if (parsed > 180)
                {
                    parsed = 180;
                    ((ParticleEjector)ParentComponent).ParticleScatterAngle = parsed;
                    sender.Text = parsed + "";
                }
                else if (parsed < 0)
                {
                    parsed = 0;
                    ((ParticleEjector)ParentComponent).ParticleScatterAngle = parsed;
                    sender.Text = parsed + "";
                }
                else
                {
                    ((ParticleEjector)ParentComponent).ParticleScatterAngle = parsed;
                }
            }
            else
            {
                args.Cancel = true;
            }
        }

        private Grid GetEjectorParticleColorChangeRateGrid()
        {
            Grid colorRateGrid = new Grid();
            colorRateGrid.Margin = new Thickness(10, 20, 10, 5);
            colorRateGrid.ColumnDefinitions.Add(new ColumnDefinition());
            colorRateGrid.ColumnDefinitions.Add(new ColumnDefinition());
            colorRateGrid.ColumnDefinitions.Add(new ColumnDefinition());

            TextBlock colorRateLabel = new TextBlock();
            Grid.SetColumn(colorRateLabel, 0);
            Grid.SetColumnSpan(colorRateLabel, 2);
            colorRateLabel.Text = "Particle Color Change";
            colorRateLabel.FontFamily = MainPage.GlobalFont;
            colorRateLabel.FontSize = 14;
            colorRateLabel.Foreground = new SolidColorBrush(Colors.White);
            colorRateLabel.VerticalAlignment = VerticalAlignment.Center;
            colorRateLabel.HorizontalAlignment = HorizontalAlignment.Center;
            colorRateGrid.Children.Add(colorRateLabel);

            TextBox colorChangeInput = new TextBox();
            Grid.SetColumn(colorChangeInput, 2);
            colorChangeInput.MaxLength = 3;
            colorChangeInput.TextAlignment = TextAlignment.Center;
            colorChangeInput.TextWrapping = TextWrapping.Wrap;
            colorChangeInput.GotFocus += (object o, RoutedEventArgs e) => colorChangeInput.SelectAll();
            colorChangeInput.BeforeTextChanging += EjectorParticleColorChangeInput_BeforeTextChanging;
            colorRateGrid.Children.Add(colorChangeInput);

            return colorRateGrid;
        }
        private void EjectorParticleColorChangeInput_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (double.TryParse(args.NewText, out double parsed))
            {
                if (parsed > 360)
                {
                    parsed = 360;
                    ((ParticleEjector)ParentComponent).ParticleColorChangeRate = parsed;
                    sender.Text = parsed + "";
                }
                else if (parsed < 0)
                {
                    parsed = 0;
                    ((ParticleEjector)ParentComponent).ParticleColorChangeRate = parsed;
                    sender.Text = parsed + "";
                }
                else
                    ((ParticleEjector)ParentComponent).ParticleColorChangeRate = parsed;
            }
            else
            {
                args.Cancel = true;
            }
        }

        //Misc Grid Functions
        private Grid GetCopyDeleteComponentGrid()
        {
            Grid copyDelGrid = new Grid();
            copyDelGrid.Width = MenuWidth - 50;
            copyDelGrid.ColumnDefinitions.Add(new ColumnDefinition());
            copyDelGrid.ColumnDefinitions.Add(new ColumnDefinition());

            //Copy Button
            Grid copyButton = GetButton("Copy", Colors.White);
            Grid.SetColumn(copyButton, 0);
            copyButton.Tapped += (s, o) =>
            {
                Component clone = ParentComponent.Clone();
                if (clone is CompLine)
                {
                    ((CompLine)clone).PosA = Physics.MovePoint(Scene.WindowCenter, 50.0, 45.0);
                    ((CompLine)clone).PosB = Physics.MovePoint(Scene.WindowCenter, 50.0, 225.0);
                } else
                    clone.Position = Scene.WindowCenter;

                Scene.AddLater(clone);
            };

            //Delete Button
            Grid deleteButton = GetButton("Delete", Colors.Red);
            Grid.SetColumn(deleteButton, 1);
            deleteButton.Tapped += (s, o) =>
            {
                Scene.Remove(ParentComponent.ID);
            };

            copyDelGrid.Children.Add(copyButton);
            copyDelGrid.Children.Add(deleteButton);

            return copyDelGrid;
        }

        private Grid GetButton(String text, Color color)
        {
            Grid buttonGrid = new Grid();
            buttonGrid.Margin = new Thickness(0, 10, 0, 10);
            buttonGrid.PointerEntered += (s, o) =>
            {
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);
            };
            buttonGrid.PointerExited += (s, o) =>
            {
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
            };

            Rectangle buttonRect = new Rectangle();
            buttonRect.Height = 30;
            buttonRect.Fill = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255));
            buttonRect.Stroke = new SolidColorBrush(Colors.Black);
            buttonRect.StrokeThickness = 1;
            buttonGrid.Children.Add(buttonRect);

            TextBlock buttonText = new TextBlock();
            buttonText.Text = text;
            buttonText.FontSize = 16;
            buttonText.FontFamily = MainPage.GlobalFont;
            buttonText.Foreground = new SolidColorBrush(color);
            buttonText.VerticalAlignment = VerticalAlignment.Center;
            buttonText.HorizontalAlignment = HorizontalAlignment.Center;
            buttonGrid.Children.Add(buttonText);

            return buttonGrid;
        }


        public override void ToggleMenuExpanded()
        {
            base.ToggleMenuExpanded();

            if (!IsMenuExpanded)
                ParentComponent = null;
        }
    }
}