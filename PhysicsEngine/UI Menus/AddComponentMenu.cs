

using PhysicsEngine.UI_Menus.AddCompOptions;
using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace PhysicsEngine.UI_Menus
{
    public class AddComponentMenu : HorizontalMenu
    {
        AddCompPanel AddRectangle;

        public override void Initialize(double height, double menuY, Color bgColor)
        {
            base.Initialize(height, menuY, bgColor);

            //Add rectangle panel
            Rectangle displayRect = new Rectangle();
            displayRect.Height = 100.0;
            displayRect.Width = 50.0;
            displayRect.Fill = new SolidColorBrush(Colors.Gray);
            displayRect.Stroke = new SolidColorBrush(Colors.Black);
            AddRectangle = new AddCompPanel(AddCompPanel.eShapeType.RECTANGLE, displayRect, "This is a test description sbg bogsudgwu gug sdjn sjbsbskjdbiubgsdbkjsfb  s gs d");
            Children.Add(AddRectangle);

            //Add line panel

            //Add particle panel

            //Add particle ejector panel
        }

        public override void ToggleMenuExpanded()
        {
            base.ToggleMenuExpanded();

            if (IsMenuExpanded)
                AddRectangle.Visibility = Visibility.Visible;
            else
                AddRectangle.Visibility = Visibility.Collapsed;
        }
    }
}