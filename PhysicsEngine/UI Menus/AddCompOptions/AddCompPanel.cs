
using Windows.Foundation;
using Windows.UI;
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

        public AddCompPanel(eShapeType shapeType, Shape display, string desc)
        {
            Orientation = Orientation.Horizontal;
            ShapeType = shapeType;
            ShapeDisplay = display;
            ShapeDisplay.PointerPressed += ShapeDisplay_PointerPressed;
            Description = new TextBlock();
            Description.Text = desc;
            Description.Foreground = new SolidColorBrush(Colors.White);
            Description.TextWrapping = TextWrapping.Wrap;
            Padding = new Thickness(50);
            Spacing = 20;
            Description.Width = ((MainPage.WindowSize.Width - (Padding.Left + Padding.Right)) / 4.0) - ShapeDisplay.Width - Spacing;
            this.Children.Add(ShapeDisplay);
            this.Children.Add(Description);
        }

        private void ShapeDisplay_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Scene.AddMenu.ToggleMenuExpanded();

            if (ShapeType == eShapeType.RECTANGLE)
            {
                CompRectangle rect = new CompRectangle();
                Point pointerCoord = e.GetCurrentPoint(Scene.MainScene).Position;
                rect.Position = Coord.FromPoint(pointerCoord);
                rect.Size = new Windows.Foundation.Size(50.0, 50.0);
                rect.Position = new Coord(rect.Position.X - rect.Size.Width / 2.0, rect.Position.Y - rect.Size.Height / 2.0);
                rect.PointerDragPoint = new Coord(pointerCoord.X - rect.Position.X, pointerCoord.Y - rect.Position.Y);
                rect.IsBeingDragged = true;
                rect.GetUIElement().CapturePointer(e.Pointer);
                rect.IsMouseDragMode = true;
                rect.GetUIElement().Opacity = 0.6;
                Scene.Add(rect);
            }
            else if (ShapeType == eShapeType.LINE)
            {

            }
            else if (ShapeType == eShapeType.ELLIPSE)
            {

            }
            else if (ShapeType == eShapeType.EJECTOR)
            {

            }
        }
    }
}