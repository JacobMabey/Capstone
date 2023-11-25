using PhysicsEngine.UI_Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace PhysicsEngine
{
    public static class Scene
    {
        public static Random Rand = new Random();
        public static bool IsComponentBeingDragged { get; set; } = false;
        public static Coord WindowCenter => new Coord(MainPage.WindowSize.Width / 2.0, MainPage.WindowSize.Height / 2.0);
        public static Dictionary<long, Component> Children { get; set; }

        //Main Global Canvas
        public static Canvas MainScene { get; private set; }

        public static readonly Thickness OUT_OF_BOUNDS_MARGIN = new Thickness(100);

        //Fills list of indexes for elements to be destroyed
        private static List<long> elementsToBeDestroyed = new List<long>();
        private static List<Component> elementsToBeAdded = new List<Component>();

        //Main Snappable Grid Global Values
        public static double SnapCellSize { get; set; } = 25.0;
        public static bool IsSnappableGridEnabled { get; set; } = true;

        //Main Space Partitioning Grid Global Values
        public static double MaxParticleRadius { get; private set; } = 100.0;
        public static double SpacePartitionCellSize => MaxParticleRadius * 2.0;
        public static Dictionary<Coord, List<Particle>> SpacePartitionGrid { get; set; } = new Dictionary<Coord, List<Particle>>();
        public static Coord CurrentCell = new Coord(0, 0);

        public static int MAX_PARTICLE_COUNT => 300;
        public static int ParticleCount { get; private set; } = 0;

        //Main global toolbar
        public static Toolbar Toolbar { get; set; }

        //Main Menus
        public static AddComponentMenu AddMenu { get; set; }
        public static WorldSettingsMenu WorldMenu { get; set; }
        public static ComponentMenu CompMenu { get; set; }

        //Border Objects
        private static CompLine borderTop;
        private static CompLine borderRight;
        private static CompLine borderBottom;
        private static CompLine borderLeft;

        private static Ellipse circleBorder;
        private static SolidColorBrush circleBorderBrush;
        public static double CircleBorderRadius { get; private set; } = 300;
        public static bool IsCircleBorderActive { get; set; } = false;


        public static void Initialize()
        {
            MainScene = new Canvas();

            //Initialize Hotkeys
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;

            //Set Size
            double width = 1080;
            double height = 720;
            MainPage.WindowSize = new Size(width, height);
            ApplicationView.PreferredLaunchViewSize = new Size(width, height);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            ApplicationView.GetForCurrentView().TryResizeView(new Size(width, height));
            Rectangle background = new Rectangle
            {
                Width = MainPage.WindowSize.Width,
                Height = MainPage.WindowSize.Height,
                Fill = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0))
            };
            Canvas.SetZIndex(background, 0);
            background.PointerPressed += MainScene_PointerPressed;
            background.PointerEntered += (s, o) =>
            {
                if (Toolbar.ComponentAddMode == eComponentAddMode.NONE)
                    Window.Current.CoreWindow.PointerCursor = new CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
                else
                    Window.Current.CoreWindow.PointerCursor = new CoreCursor(Windows.UI.Core.CoreCursorType.Cross, 1);
            };
            background.PointerExited += (s, o) =>
            {
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
            };
            MainScene.Children.Add(background);

            Children = new Dictionary<long, Component>();

            //Add Toolbar & Menus
            Toolbar = new Toolbar();
            MainScene.Children.Add(Toolbar);

            AddMenu = new AddComponentMenu();
            AddMenu.Initialize(280, Scene.MainScene.Height - Toolbar.ToolbarHeight + 10, Color.FromArgb(180, 0, 0, 0));
            MainScene.Children.Add(AddMenu);

            WorldMenu = new WorldSettingsMenu();
            WorldMenu.Initialize(320, Scene.MainScene.Width + 20, Color.FromArgb(180, 0, 0, 0));
            MainScene.Children.Add(WorldMenu);

            CompMenu = new ComponentMenu();
            CompMenu.Initialize(320, Scene.MainScene.Width + 20, Color.FromArgb(180, 0, 0, 0));
            MainScene.Children.Add(CompMenu);

            //Add collision border lines
            borderTop = new CompLine(new Coord(0, 0), new Coord(MainScene.Width, 0), Colors.Black, 1.0);
            borderRight = new CompLine(new Coord(MainScene.Width, 0), new Coord(MainScene.Width, MainScene.Height - Toolbar.ToolbarHeight), Colors.Black, 1.0);
            borderBottom = new CompLine(new Coord(0, MainScene.Height - Toolbar.ToolbarHeight), new Coord(MainScene.Width, MainScene.Height - Toolbar.ToolbarHeight), Colors.Black, 1.0);
            borderLeft = new CompLine(new Coord(0, 0), new Coord(0, MainScene.Height - Toolbar.ToolbarHeight), Colors.Black, 1.0);
            Add(borderTop);
            Add(borderRight);
            Add(borderBottom);
            Add(borderLeft);

            circleBorder = new Ellipse();
            Canvas.SetLeft(circleBorder, MainScene.Width / 2.0 - CircleBorderRadius);
            Canvas.SetTop(circleBorder, (MainScene.Height - Toolbar.ToolbarHeight) / 2.0 - CircleBorderRadius);
            Canvas.SetZIndex(circleBorder, -1);
            circleBorder.Width = CircleBorderRadius * 2.0;
            circleBorder.Height = CircleBorderRadius * 2.0;
            circleBorder.Fill = new SolidColorBrush(Colors.Transparent);
            circleBorder.StrokeThickness = 1;
            circleBorder.Opacity = 0;
            MainScene.Children.Add(circleBorder);
        }

        private static void MainScene_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (Scene.Toolbar != null)
            {
                if (Scene.Toolbar.ComponentAddMode == eComponentAddMode.NONE)
                {
                    if (Scene.AddMenu.IsMenuExpanded)
                        Scene.AddMenu.ToggleMenuExpanded();

                    if (Scene.WorldMenu.IsMenuExpanded)
                        Scene.WorldMenu.ToggleMenuExpanded();

                    if (Scene.CompMenu.IsMenuExpanded)
                        Scene.CompMenu.ToggleMenuExpanded();
                }
                else if (Scene.Toolbar.ComponentAddMode == eComponentAddMode.RECTANGLE)
                {
                    CompRectangle rect = new CompRectangle();
                    rect.StrokeThickness = 2;
                    rect.Fill = Color.FromArgb(255, 79, 137, 196);
                    Point pointerCoord = e.GetCurrentPoint(Scene.MainScene).Position;
                    rect.Position = Coord.FromPoint(pointerCoord);
                    rect.Size = new Windows.Foundation.Size(70.0, 100.0);
                    rect.Position = new Coord(rect.Position.X - rect.Size.Width / 2.0, rect.Position.Y - rect.Size.Height / 2.0);
                    rect.PointerDragPoint = new Coord(pointerCoord.X - rect.Position.X, pointerCoord.Y - rect.Position.Y);

                    rect.IsBeingDragged = true;
                    rect.IsBeingAdded = true;
                    rect.GetUIElement().CapturePointer(e.Pointer);
                    rect.IsMouseDragMode = true;
                    rect.GetUIElement().Opacity = 0.6;
                    Scene.Add(rect);
                }
                else if (Scene.Toolbar.ComponentAddMode == eComponentAddMode.LINE)
                {
                    CompLine line = new CompLine();
                    line.Thickness = 8;
                    line.Fill = Color.FromArgb(255, 52, 173, 79);
                    Coord centerPos = Coord.FromPoint(e.GetCurrentPoint(Scene.MainScene).Position);
                    line.PosA = new Coord(centerPos.X + 35.0, centerPos.Y - 50.0);
                    line.PosB = new Coord(centerPos.X - 35.0, centerPos.Y + 50.0);

                    line.IsBeingDragged = true;
                    line.IsBeingAdded = true;
                    line.GetUIElement().CapturePointer(e.Pointer);
                    line.FullLineBeingDragged = true;
                    Scene.Add(line);
                }
                else if (Scene.Toolbar.ComponentAddMode == eComponentAddMode.PARTICLE)
                {
                    Particle particle = new Particle();
                    particle.Radius = 10;
                    particle.Position = Coord.FromPoint(e.GetCurrentPoint(Scene.MainScene).Position);
                    particle.Fill = Color.FromArgb(255, 242, 80, 80);

                    particle.IsBeingDragged = true;
                    particle.IsBeingAdded = true;
                    particle.GetUIElement().CapturePointer(e.Pointer);
                    Scene.Add(particle);

                }
                else if (Scene.Toolbar.ComponentAddMode == eComponentAddMode.EJECTOR)
                {
                    Coord pointerCoord = Coord.FromPoint(e.GetCurrentPoint(Scene.MainScene).Position);
                    ParticleEjector ejector = new ParticleEjector(pointerCoord, 295.0, 100);
                    ejector.ParticleRadius = 10;
                    ejector.ParticleColor = Color.FromArgb(255, 169, 80, 242);
                    ejector.FillColorIsBasedOnParticle = true;
                    ejector.IsPaused = true;
                    ejector.Position = new Coord(ejector.Position.X - ParticleEjector.EJECTOR_SIZE.Width / 2.0, ejector.Position.Y - ParticleEjector.EJECTOR_SIZE.Height / 2.0);

                    ejector.PointerDragPoint = new Coord(pointerCoord.X - ejector.Position.X, pointerCoord.Y - ejector.Position.Y);
                    ejector.IsBeingDragged = true;
                    ejector.IsBeingAdded = true;
                    ejector.GetUIElement().CapturePointer(e.Pointer);
                    Scene.Add(ejector);
                }
            }
        }


        //HOT KEYS
        private static void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                case Windows.System.VirtualKey.G: // Toggle Gravity
                    Physics.IsGravityEnabled = !Physics.IsGravityEnabled;
                    WorldMenu.GravityCheckbox.IsChecked = Physics.IsGravityEnabled;
                    break;
                case Windows.System.VirtualKey.C: // Copy Selected Component
                    if (CompMenu.IsMenuExpanded && CompMenu.ParentComponent != null)
                        CompMenu.CloneSelectedComponent();

                    break;
                case Windows.System.VirtualKey.Delete: // Copy Selected Component
                    if (CompMenu.IsMenuExpanded && CompMenu.ParentComponent != null)
                        Scene.Remove(CompMenu.ParentComponent.ID);
                    break;
                case Windows.System.VirtualKey.X: // Clear Add Component Mode
                    Toolbar.ComponentAddMode = eComponentAddMode.NONE;
                    if (Window.Current.CoreWindow.PointerCursor.Type == CoreCursorType.Cross)
                        Window.Current.CoreWindow.PointerCursor = new CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
                    break;
                case Windows.System.VirtualKey.Space: // Toggle Scene Paused
                    Toolbar.TogglePauseScene();
                    break;
                case Windows.System.VirtualKey.Escape: // Close Any Menus

                    Toolbar.ComponentAddMode = eComponentAddMode.NONE;
                    if (Window.Current.CoreWindow.PointerCursor.Type == CoreCursorType.Cross)
                        Window.Current.CoreWindow.PointerCursor = new CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);

                    if (Scene.AddMenu.IsMenuExpanded)
                        Scene.AddMenu.ToggleMenuExpanded();

                    if (Scene.WorldMenu.IsMenuExpanded)
                        Scene.WorldMenu.ToggleMenuExpanded();

                    if (Scene.CompMenu.IsMenuExpanded)
                        Scene.CompMenu.ToggleMenuExpanded();

                    break;
                case Windows.System.VirtualKey.A: // Toggle Add Component Menu
                    AddMenu.ToggleMenuExpanded();

                    if (AddMenu.IsMenuExpanded)
                    {
                        //Close other menus
                        if (Scene.WorldMenu.IsMenuExpanded)
                            Scene.WorldMenu.ToggleMenuExpanded();

                        if (Scene.CompMenu.IsMenuExpanded)
                            Scene.CompMenu.ToggleMenuExpanded();
                    }
                    break;
                case Windows.System.VirtualKey.S: // Toggle World Settings Menu
                    WorldMenu.ToggleMenuExpanded();

                    if (WorldMenu.IsMenuExpanded)
                    {
                        //Close other menus
                        if (Scene.AddMenu.IsMenuExpanded)
                            Scene.AddMenu.ToggleMenuExpanded();

                        if (Scene.CompMenu.IsMenuExpanded)
                            Scene.CompMenu.ToggleMenuExpanded();
                    }
                    break;
            }
        }



        public static void Update()
        {
            if (Timer.Ticks < 15)
                return;

            if (elementsToBeDestroyed == null)
                elementsToBeDestroyed = new List<long>();
            else elementsToBeDestroyed.Clear();

            //Update all Children
            ClearSpacePartitionGrid();
            foreach (Component comp in Children.Values)
            {
                //Update all components

                //Check for out of bounds elements
                double x = Canvas.GetLeft(comp.GetUIElement());
                double y = Canvas.GetTop(comp.GetUIElement());
                if (comp is Particle)
                {
                    x += ((Particle)comp).Radius;
                    y += ((Particle)comp).Radius;
                }

                if (x < -OUT_OF_BOUNDS_MARGIN.Left || x > MainScene.Width + OUT_OF_BOUNDS_MARGIN.Right
                    || y < -OUT_OF_BOUNDS_MARGIN.Top || y > MainScene.Height + OUT_OF_BOUNDS_MARGIN.Bottom)
                {
                    //Mark element to be destroyed
                        RemoveLater(comp.ID);
                        continue;
                }

                //Add element to space partition grid
                if (comp is Particle)
                {
                    int row = (int)((comp.Position.Y - ((Particle)comp).Radius) / SpacePartitionCellSize);
                    int col = (int)((comp.Position.X - ((Particle)comp).Radius) / SpacePartitionCellSize);

                    Coord pos = new Coord(col, row);
                    if (!SpacePartitionGrid.ContainsKey(pos) || (SpacePartitionGrid.ContainsKey(pos) && !SpacePartitionGrid[pos].Contains(comp)))
                        AddToSpacePartitionGrid(pos, (Particle)comp);
                }
            }
            //Loop through all partitioned spaces in the main space partitioning grid
            foreach (KeyValuePair<Coord, List<Particle>> cell in Scene.SpacePartitionGrid)
            {
                CurrentCell = cell.Key;
                foreach (Particle p in cell.Value)
                {
                    //Update Element
                    p.Update();
                }
            }
            foreach (Component comp in Children.Values.Where(c => !(c is Particle)))
            {
                comp.Update();
            }

                //Destroy and remove all marked elements
            for (int i = 0; i < elementsToBeDestroyed.Count; i++)
            {
                MainScene.Children.Remove(Children[elementsToBeDestroyed[i]].GetUIElement());
                Children.Remove(elementsToBeDestroyed[i]);
            }
            elementsToBeDestroyed.Clear();

            //Add all marked elements
            for (int i = 0; i < elementsToBeAdded.Count; i++)
            {
                Children.Add(elementsToBeAdded[i].ID, elementsToBeAdded[i]);
                MainScene.Children.Add(elementsToBeAdded[i].GetUIElement());
            }
            elementsToBeAdded.Clear();

        }

        private static void AddToSpacePartitionGrid(Coord pos, Particle particle)
        {
            if (!SpacePartitionGrid.ContainsKey(pos))
                SpacePartitionGrid[pos] = new List<Particle>();
            SpacePartitionGrid[pos].Add(particle);
        }
        private static void ClearSpacePartitionGrid()
        {
            foreach (var cell in SpacePartitionGrid)
                cell.Value.Clear();
            SpacePartitionGrid.Clear();
        }

        public static void Add(Component comp)
        {
            if (comp == null) return;

            //check if component is particle
            if (comp is Particle)
            {
                if (ParticleCount < MAX_PARTICLE_COUNT)
                {
                    ParticleCount++;
                }
                else return;

                //check radius for max radius spawned to set grid partion size
                if (((Particle)comp).Radius > MaxParticleRadius)
                    MaxParticleRadius = ((Particle)comp).Radius;
            }

            Children.Add(comp.ID, comp);
            MainScene.Children.Add(comp.GetUIElement());
        }

        public static bool AddLater(Component comp)
        {
            if (comp == null) return false;

            //check if component is particle
            if (comp is Particle)
            {
                if (ParticleCount < MAX_PARTICLE_COUNT)
                {
                    ParticleCount++;
                }
                else return false;

                //check radius for max radius spawned to set grid partion size
                if (((Particle)comp).Radius > MaxParticleRadius)
                    MaxParticleRadius = ((Particle)comp).Radius;
            }

            elementsToBeAdded.Add(comp);
            return true;
        }

        public static void Remove(long id)
        {
            if (!Children.ContainsKey(id)) return;

            //check if component is particle
            if (Children[id] is Particle)
                ParticleCount--;

            MainScene.Children.Remove(Children[id].GetUIElement());
            Children.Remove(id);

            //Check if component has menu open
            if (Scene.CompMenu != null && Scene.CompMenu.IsMenuExpanded && Scene.CompMenu.ParentComponent.ID == id)
                Scene.CompMenu.ToggleMenuExpanded();
        }
        public static void RemoveLater(long id)
        {
            if (!Children.ContainsKey(id)) return;

            //check if component is particle
            if (Children[id] is Particle)
                ParticleCount--;

            elementsToBeDestroyed.Add(id);

            //Check if component has menu open
            if (Scene.CompMenu != null && Scene.CompMenu.IsMenuExpanded && Scene.CompMenu.ParentComponent.ID == id)
                Scene.CompMenu.ToggleMenuExpanded();
        }

        public static void ClearScene()
        {
            Children.Clear();
            for (int i = MainScene.Children.Count - 1; i >= 0; i--)
            {
                if (MainScene.Children[i] is Shape && ((Shape)MainScene.Children[i]).Tag is Component)
                    MainScene.Children.RemoveAt(i);
            }
            ParticleCount = 0;
            Add(borderTop);
            Add(borderRight);
            Add(borderBottom);
            Add(borderLeft);

            if (Scene.CompMenu.IsMenuExpanded)
                Scene.CompMenu.ToggleMenuExpanded();
        }


        public static async void SaveScene()
        {
            if (!Timer.IsPaused)
                Toolbar.TogglePauseScene();

            FileSavePicker savePicker = new FileSavePicker();
            savePicker.DefaultFileExtension = ".psm";
            savePicker.FileTypeChoices.Add("Particle Sim", new List<string>() { ".psm" });
            savePicker.SuggestedFileName = "scene";
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                //Following code was helpfully given by ChatGPT
                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        await writer.WriteAsync(GetSaveText());
                    }
                }
            }
        }

        public static async void LoadScene()
        {
            if (!Timer.IsPaused)
                Toolbar.TogglePauseScene();

            FileOpenPicker loadPicker = new FileOpenPicker();
            loadPicker.FileTypeFilter.Add(".psm");
            loadPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            StorageFile file = await loadPicker.PickSingleFileAsync();
            if (file == null) return;

            //Clear scene and load properties
            ClearScene();
            WorldMenu.ToggleMenuExpanded();
            Toolbar.ComponentAddMode = eComponentAddMode.NONE;

            string text = await FileIO.ReadTextAsync(file);

            string[] lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string[] prop = lines[i].Split(':');

                switch (prop[0])
                {
                    //Scene Properties
                    case "IsGravityEnabled":
                        Physics.IsGravityEnabled = bool.Parse(prop[1]);
                        Scene.WorldMenu.GravityCheckbox.IsChecked = Physics.IsGravityEnabled;
                        break;
                    case "GravityAcceleration":
                        Physics.GravityAcceleration = double.Parse(prop[1]);
                        Scene.WorldMenu.GravityInput.Text = Physics.GravityAcceleration+"";
                        break;
                    case "BackgroundColor":
                        Renderer.BackgroundColor = LoadColor(prop[1]);
                        break;
                    case "BorderCollisionEnabled":
                        SetBorderCollision(bool.Parse(prop[1]));
                        Scene.WorldMenu.EdgeBorderCheckbox.IsChecked = IsBorderCollisionEnabled();
                        break;
                    case "IsCircleBorderActive":
                        SetCircleBorderCollision(bool.Parse(prop[1]));
                        Scene.WorldMenu.CircBorderCheckbox.IsChecked = IsCircleBorderActive;
                        break;
                    case "CircleBorderRadius":
                        SetCircleBorderRadius(double.Parse(prop[1]));
                        Scene.WorldMenu.CircRadInput.Text = CircleBorderRadius+"";
                        break;

                    case "comp_rectangle": // Load Rectangle
                        CompRectangle rect = new CompRectangle();

                        //Is Collision Enabled
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("IsCollisionEnabled"))
                            rect.IsCollisionEnabled = bool.Parse(prop[1]);

                        //Position
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("Position"))
                            rect.Position = LoadPosition(prop[1]);

                        //Size
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("Size"))
                            rect.Size = LoadSize(prop[1]);

                        //Fill
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("Fill"))
                            rect.Fill = LoadColor(prop[1]);

                        //RotationAngle
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("RotationAngle"))
                            rect.RotationAngle = double.Parse(prop[1]);

                        //RotationCenter
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("RotationCenter"))
                            rect.RotationCenter = LoadPosition(prop[1]);

                        //StrokeColor
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("StrokeColor"))
                            rect.StrokeColor = LoadColor(prop[1]);

                        //StrokeThickness
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("StrokeThickness"))
                            rect.StrokeThickness = double.Parse(prop[1]);

                        if (lines[++i].Equals("-"))
                        {
                            Add(rect);
                        }
                        else throw new Exception("Error Loading Rectangle Component");
                        break;

                    case "comp_line": // Load Line
                        CompLine line = new CompLine();

                        //Is Collision Enabled
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("IsCollisionEnabled"))
                            line.IsCollisionEnabled = bool.Parse(prop[1]);

                        //PosA
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("PosA"))
                            line.PosA = LoadPosition(prop[1]);

                        //PosB
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("PosB"))
                            line.PosB = LoadPosition(prop[1]);

                        //Fill
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("Fill"))
                            line.Fill = LoadColor(prop[1]);

                        //Thickness
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("Thickness"))
                            line.Thickness = double.Parse(prop[1]);

                        if (lines[++i].Equals("-"))
                        {
                            Add(line);
                        }
                        else throw new Exception("Error Loading Line Component");
                        break;

                    case "comp_particle": // Load Particle
                        Particle particle = new Particle();

                        //Is Collision Enabled
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("IsCollisionEnabled"))
                            particle.IsCollisionEnabled = bool.Parse(prop[1]);

                        //Is Particle Collision Enabled
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("IsParticleCollisionEnabled"))
                            particle.IsParticleCollisionEnabled = bool.Parse(prop[1]);

                        //Fill
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("Fill"))
                            particle.Fill = LoadColor(prop[1]);

                        //ColorChangeRate
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("ColorChangeRate"))
                            particle.ColorChangeRate = double.Parse(prop[1]);

                        //HasPhysics
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("HasPhysics"))
                            particle.HasPhysics = bool.Parse(prop[1]);

                        //Radius
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("Radius"))
                            particle.Radius = double.Parse(prop[1]);

                        //Position
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("Position"))
                            particle.Position = LoadPosition(prop[1]);

                        //Mass
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("Mass"))
                            particle.Phys.Mass = double.Parse(prop[1]);

                        //Friction
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("Friction"))
                            particle.Phys.Friction = double.Parse(prop[1]);

                        //Elasticity
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("Elasticity"))
                            particle.Phys.Elasticity = double.Parse(prop[1]);

                        //Velocity
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("Velocity"))
                            particle.Phys.Velocity = LoadPosition(prop[1]);

                        //Acceleration
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("Acceleration"))
                            particle.Phys.Acceleration = LoadPosition(prop[1]);


                        if (lines[++i].Equals("-"))
                        {
                            Add(particle);
                        }
                        else throw new Exception("Error Loading Line Component");
                        break;

                    case "comp_ejector": // Load Particle
                        Coord ejectorPosition = new Coord(0, 0);
                        double rotateAngle = 0;
                        double particleLimit = 100;

                        //Position
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("Position"))
                            ejectorPosition = LoadPosition(prop[1]);

                        //RotationAngle
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("RotationAngle"))
                            rotateAngle = double.Parse(prop[1]);

                        //ParticleLimit
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("ParticleLimit"))
                            particleLimit = double.Parse(prop[1]);

                        ParticleEjector ejector = new ParticleEjector(ejectorPosition, rotateAngle, (int)particleLimit);

                        //ParticleRate
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("ParticleRate"))
                            ejector.ParticleRate = double.Parse(prop[1]);

                        //ParticleVelocity
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("ParticleVelocity"))
                            ejector.ParticleVelocity = double.Parse(prop[1]);

                        //ParticleElasticity
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("ParticleElasticity"))
                            ejector.ParticleElasticity = double.Parse(prop[1]);

                        //ParticleFriction
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("ParticleFriction"))
                            ejector.ParticleFriction = double.Parse(prop[1]);

                        //ColorChangeRate
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("ColorChangeRate"))
                            ejector.ColorChangeRate = double.Parse(prop[1]);

                        //ParticleColorChangeRate
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("ParticleColorChangeRate"))
                            ejector.ParticleColorChangeRate = double.Parse(prop[1]);

                        //Fill
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("Fill"))
                            ejector.Fill = LoadColor(prop[1]);

                        //ParticleColor
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("ParticleColor"))
                            ejector.ParticleColor = LoadColor(prop[1]);

                        //FillColorIsBasedOnParticle
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("FillColorIsBasedOnParticle"))
                            ejector.FillColorIsBasedOnParticle = bool.Parse(prop[1]);

                        //ParticleRadius
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("ParticleRadius"))
                            ejector.ParticleRadius = double.Parse(prop[1]);

                        //ParticleRadiusRange
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("ParticleRadiusRange"))
                            ejector.ParticleRadiusRange = double.Parse(prop[1]);

                        //ParticleScatterAngle
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("ParticleScatterAngle"))
                            ejector.ParticleScatterAngle = double.Parse(prop[1]);

                        //ParticlesEjected
                        prop = lines[++i].Split(':');
                        if (prop[0].Equals("ParticlesEjected"))
                            ejector.ParticlesEjected = int.Parse(prop[1]);


                        if (lines[++i].Equals("-"))
                        {
                            Add(ejector);
                        }
                        else throw new Exception("Error Loading Line Component");
                        break;
                }
            }
        }

        private static Color LoadColor(string color)
        {
            string[] c = color.Split(',');
            return Color.FromArgb((byte)double.Parse(c[0]), (byte)double.Parse(c[1]), (byte)double.Parse(c[2]), (byte)double.Parse(c[3]));
        }
        private static Coord LoadPosition(string position)
        {
            string[] coord = position.Split(',');
            return new Coord(double.Parse(coord[0]), double.Parse(coord[1]));
        }
        private static Size LoadSize(string size)
        {
            string[] coord = size.Split(',');
            return new Size(double.Parse(coord[0]), double.Parse(coord[1]));
        }


        private static string GetSaveText()
        {
            string output = "";

            output += "IsGravityEnabled:" + Physics.IsGravityEnabled + "\n";
            output += "GravityAcceleration:" + Physics.GravityAcceleration + "\n";
            output += "BackgroundColor:" + Renderer.BackgroundColor.A + "," + Renderer.BackgroundColor.R + "," + Renderer.BackgroundColor.G + "," + Renderer.BackgroundColor.B + "\n";
            output += "BorderCollisionEnabled:" + IsBorderCollisionEnabled() + "\n";
            output += "IsCircleBorderActive:" + IsCircleBorderActive + "\n";
            output += "CircleBorderRadius:" + CircleBorderRadius + "\n";

            foreach (Component comp in Children.Values)
            {
                if (comp.ID != borderTop.ID && comp.ID != borderBottom.ID && comp.ID != borderLeft.ID && comp.ID != borderRight.ID)
                output += comp.GetSaveText();
            }

            return output;
        }


        public static void ToggleBorderCollision()
        {
            borderTop.IsCollisionEnabled = !borderTop.IsCollisionEnabled;
            borderRight.IsCollisionEnabled = !borderRight.IsCollisionEnabled;
            borderBottom.IsCollisionEnabled = !borderBottom.IsCollisionEnabled;
            borderLeft.IsCollisionEnabled = !borderLeft.IsCollisionEnabled;
        }
        public static void SetBorderCollision(bool col)
        {
            borderTop.IsCollisionEnabled = col;
            borderRight.IsCollisionEnabled = col;
            borderBottom.IsCollisionEnabled = col;
            borderLeft.IsCollisionEnabled = col;
        }
        public static bool IsBorderCollisionEnabled()
        {
            return borderTop.IsCollisionEnabled;
        }

        public static void ToggleCircleBorderCollision()
        {
            IsCircleBorderActive = !IsCircleBorderActive;
            circleBorder.Opacity = IsCircleBorderActive ? 1 : 0;
        }
        public static void SetCircleBorderCollision(bool col)
        {
            IsCircleBorderActive = col;
            circleBorder.Opacity = IsCircleBorderActive ? 1 : 0;
        }
        public static void SetCircleBorderRadius(double radius)
        {
            CircleBorderRadius = radius;
            circleBorder.Width = radius * 2.0;
            circleBorder.Height = radius * 2.0;

            Canvas.SetLeft(circleBorder, MainScene.Width / 2.0 - CircleBorderRadius);
            Canvas.SetTop(circleBorder, (MainScene.Height - Toolbar.ToolbarHeight) / 2.0 - CircleBorderRadius);
        }
        public static double GetCircleBorderRadius()
        {
            return CircleBorderRadius;
        }

        public static void SetCircleBorderColor(Color color)
        {
            if (circleBorderBrush == null)
                circleBorderBrush = new SolidColorBrush(color);
            else
                circleBorderBrush.Color = color;

            if (circleBorder.Stroke != circleBorderBrush)
                circleBorder.Stroke = circleBorderBrush;
        }


        public static void ResetBorderPositions()
        {
            if (borderTop == null)
                return;

            borderTop.PosA = new Coord(0, 0);
            borderTop.PosB = new Coord(MainScene.Width, 0);
            borderRight.PosA = new Coord(MainScene.Width, 0);
            borderRight.PosB = new Coord(MainScene.Width, MainScene.Height - Toolbar.ToolbarHeight);
            borderBottom.PosA = new Coord(0, MainScene.Height - Toolbar.ToolbarHeight);
            borderBottom.PosB = new Coord(MainScene.Width, MainScene.Height - Toolbar.ToolbarHeight);
            borderLeft.PosA = new Coord(0, 0);
            borderLeft.PosB = new Coord(0, MainScene.Height - Toolbar.ToolbarHeight);
        }
    }
}
