
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;

namespace PhysicsEngine.UI_Menus.AddCompOptions
{
    public class AddCompPanel : StackPanel
    {
        Shape ShapeDisplay {  get; set; }
        TextBlock Description {  get; set; }

        public AddCompPanel(Shape display, string desc)
        {
            Orientation = Orientation.Horizontal;
            ShapeDisplay = display;
            ShapeDisplay.PointerPressed += ShapeDisplay_PointerPressed;
            ShapeDisplay.PointerReleased += ShapeDisplay_PointerReleased;
            Description = new TextBlock();
            Description.Text = desc;
            this.Children.Add(ShapeDisplay);
            this.Children.Add(Description);
        }

        private void ShapeDisplay_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {

        }

        private void ShapeDisplay_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {

        }
    }
}