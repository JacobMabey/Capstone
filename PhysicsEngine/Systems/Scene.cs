﻿using PhysicsEngine.UI_Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using static PhysicsEngine.UI_Menus.AddCompOptions.AddCompPanel;

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
        public static double MaxParticleRadius { get; private set; } = 30.0;
        public static double SpacePartitionCellSize => MaxParticleRadius * 2.0;
        public static Dictionary<Coord, List<Particle>> SpacePartitionGrid { get; set; } = new Dictionary<Coord, List<Particle>>();
        public static Coord CurrentCell = new Coord(0, 0);

        public static int MAX_PARTICLE_COUNT => 400;
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
        public static double CircleBorderRadius { get; private set; } = 300;
        public static bool IsCircleBorderActive { get; set; } = false;


        public static void Initialize()
        {
            MainScene = new Canvas();

            //Initialize Hotkeys
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;

            //Set Size
            MainPage.WindowSize = new Size(1080, 720);
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
            circleBorder.Stroke = new SolidColorBrush(Colors.Black);
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
                    particle.Position = Coord.FromPoint(e.GetCurrentPoint(Scene.MainScene).Position);
                    particle.Position = new Coord(particle.Position.X - particle.Radius, particle.Position.Y - particle.Radius);
                    particle.Radius = 10;
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
