
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using static PhysicsEngine.UI_Menus.AddCompOptions.AddCompPanel;

namespace PhysicsEngine.UI_Menus.AddCompOptions
{
    public class AddCompPanel : StackPanel
    {
        public enum eShapeType
        {
            RECTANGLE,
            ELLIPSE,
            LINE,
            EJECTOR
        }
        public Shape ShapeDisplay {  get; set; }
        public eShapeType ShapeType { get; set; }
        public TextBlock Description {  get; set; }
        public TextBlock CompTitle { get; set; }
        public static StackPanel DescStack {  get; set; }

        public AddCompPanel(eShapeType shapeType, Shape display, string desc)
        {
            this.PointerPressed += ShapeDisplay_PointerPressed;
            this.PointerEntered += (s, o) =>
            {
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);
            };
            this.PointerExited += (s, o) =>
            {
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
            };
            this.Background = new SolidColorBrush(Color.FromArgb(10, 0, 0, 0));
            this.BorderBrush = new SolidColorBrush(Colors.White);
            this.BorderThickness = new Thickness(2);
            Margin = new Thickness(20);
            Padding = new Thickness(10);
            Spacing = 20;

            //Add Shape Display
            Orientation = Orientation.Horizontal;
            ShapeType = shapeType;
            ShapeDisplay = display;
            this.Children.Add(ShapeDisplay);

            //Add Component Title
            CompTitle = new TextBlock();
            switch (ShapeType)
            {
                default:
                case eShapeType.RECTANGLE:
                    CompTitle.Text = "Rectangle";
                    break;
                case eShapeType.LINE:
                    CompTitle.Text = "Line";
                    break;
                case eShapeType.ELLIPSE:
                    CompTitle.Text = "Particle";
                    break;
                case eShapeType.EJECTOR:
                    CompTitle.Text = "Ejector";
                    break;
            }
            CompTitle.FontFamily = MainPage.GlobalFont;
            CompTitle.FontWeight = FontWeights.Bold;
            CompTitle.Height = (Height - Padding.Top - Padding.Bottom) * 0.15;
            CompTitle.FontSize = 26;
            CompTitle.Foreground = new SolidColorBrush(Colors.White);


            //Add Description
            Description = new TextBlock();
            Description.FontFamily = MainPage.GlobalFont;
            Description.Height = (Height - Padding.Top - Padding.Bottom) * 0.75;
            Description.Text = desc;
            Description.FontSize = 12;
            Description.Foreground = new SolidColorBrush(Colors.White);
            Description.TextWrapping = TextWrapping.Wrap;

            //Add Tooltips
            TextBlock rotateTooltip = new TextBlock();
            rotateTooltip.Text = "-Hold SHIFT to Rotate";
            rotateTooltip.FontFamily = MainPage.GlobalFont;
            rotateTooltip.TextWrapping = TextWrapping.Wrap;
            rotateTooltip.Foreground = new SolidColorBrush(Color.FromArgb(255, 240, 176, 146));
            rotateTooltip.FontSize = 10;

            TextBlock snapTooltip = new TextBlock();
            snapTooltip.Text = "-Hold CTRL to Snap to Grid";
            snapTooltip.FontFamily = MainPage.GlobalFont;
            snapTooltip.TextWrapping = TextWrapping.Wrap;
            snapTooltip.Foreground = new SolidColorBrush(Color.FromArgb(255, 240, 176, 146));
            snapTooltip.FontSize = 10;

            TextBlock lineTooltip = new TextBlock();
            lineTooltip.Text = "-Grab Edges or Center to Move";
            lineTooltip.FontFamily = new FontFamily("Cascadia Code");
            lineTooltip.TextWrapping = TextWrapping.Wrap;
            lineTooltip.Foreground = new SolidColorBrush(Color.FromArgb(255, 240, 176, 146));
            lineTooltip.FontSize = 10;

            TextBlock ejectorTooltip = new TextBlock();
            ejectorTooltip.Text = "-Right Click to Pause";
            ejectorTooltip.Padding = new Thickness(0, 12, 0, 0);
            ejectorTooltip.FontFamily = MainPage.GlobalFont;
            ejectorTooltip.FontSize = 10;
            ejectorTooltip.Foreground = new SolidColorBrush(Color.FromArgb(255, 240, 176, 146));

            TextBlock dragTooltip = new TextBlock();
            dragTooltip.Text = "Click and Drag Shape to Add";
            dragTooltip.Padding = new Thickness(0, 12, 0 , 0);
            dragTooltip.FontFamily = MainPage.GlobalFont;
            dragTooltip.TextWrapping = TextWrapping.Wrap;
            dragTooltip.FontSize = 10;
            dragTooltip.Foreground = new SolidColorBrush(Colors.LightGray);

            //Create Title / Description Stack
            DescStack = new StackPanel();
            DescStack.Width = MainPage.WindowSize.Width / 4.0 - (Margin.Left + Margin.Right) - (Padding.Left + Padding.Right) - Spacing - BorderThickness.Right * 2.0 - AddComponentMenu.ShapeDisplaySize.Width;
            DescStack.Children.Add(CompTitle);
            DescStack.Children.Add(Description);
            if (ShapeType != eShapeType.LINE && ShapeType != eShapeType.ELLIPSE)
                DescStack.Children.Add(rotateTooltip);
            if (ShapeType == eShapeType.LINE)
                DescStack.Children.Add(lineTooltip);
            if (ShapeType == eShapeType.EJECTOR)
                DescStack.Children.Add(ejectorTooltip);
            DescStack.Children.Add(snapTooltip);
            DescStack.Children.Add(dragTooltip);

            this.Children.Add(DescStack);
        }

        private void ShapeDisplay_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Scene.AddMenu.ToggleMenuExpanded();

            if (ShapeType == eShapeType.RECTANGLE)
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
            else if (ShapeType == eShapeType.LINE)
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
            else if (ShapeType == eShapeType.ELLIPSE)
            {
                Particle particle = new Particle();
                particle.Position = Coord.FromPoint(e.GetCurrentPoint(Scene.MainScene).Position);
                particle.Radius = 10;
                particle.Fill = Color.FromArgb(255, 242, 80, 80);

                particle.IsBeingDragged = true;
                particle.IsBeingAdded = true;
                particle.GetUIElement().CapturePointer(e.Pointer);
                Scene.Add(particle);

            }
            else if (ShapeType == eShapeType.EJECTOR)
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
}