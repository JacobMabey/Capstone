using PhysicsEngine.UI_Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace PhysicsEngine
{
    public static class Scene
    {
        public static Random Rand = new Random();

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
        public static double MaxParticleRadius { get; private set; } = 5.0;
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

            //Set Size
            MainPage.WindowSize = new Size(1080, 720);
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
            circleBorder.Width = CircleBorderRadius * 2.0;
            circleBorder.Height = CircleBorderRadius * 2.0;
            circleBorder.Fill = new SolidColorBrush(Colors.Transparent);
            circleBorder.Stroke = new SolidColorBrush(Colors.Black);
            circleBorder.StrokeThickness = 1;
            circleBorder.Opacity = 0;
            MainScene.Children.Add(circleBorder);
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

            MainScene.Children.Remove(Children[id].GetUIElement());
            Children.Remove(id);

            //check if component is particle
            if (Children[id] is Particle)
                ParticleCount--;
        }
        public static void RemoveLater(long id)
        {
            if (!Children.ContainsKey(id)) return;

            elementsToBeDestroyed.Add(id);

            //check if component is particle
            if (Children[id] is Particle)
                ParticleCount--;
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
