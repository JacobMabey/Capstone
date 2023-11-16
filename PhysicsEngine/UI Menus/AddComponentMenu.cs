

using PhysicsEngine.UI_Menus.AddCompOptions;
using System;
using Windows.Foundation;
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
        AddCompPanel AddLine;
        AddCompPanel AddParticle;
        AddCompPanel AddEjector;

        public static Size ShapeDisplaySize = new Size(50.0, 75.0);

        public override void Initialize(double height, double menuY, Color bgColor)
        {
            base.Initialize(height, menuY, bgColor);
            ExpandBoard.Completed += ExpandBoard_Completed;

            //Setup Component Panel
            StackPanel CompPanel = new StackPanel();
            CompPanel.Orientation = Orientation.Horizontal;

            //Add rectangle panel
            Rectangle displayRect = new Rectangle();
            displayRect.Height = ShapeDisplaySize.Height;
            displayRect.Width = ShapeDisplaySize.Width;
            displayRect.Fill = new SolidColorBrush(Color.FromArgb(255, 79, 137, 196));
            displayRect.Stroke = new SolidColorBrush(Colors.Black);
            displayRect.StrokeThickness = 2;
            AddRectangle = new AddCompPanel(AddCompPanel.eShapeType.RECTANGLE, displayRect,
                "The Rectangle component allows you to add structured geometry to your scene."
            );
            AddRectangle.Visibility = Visibility.Collapsed;
            CompPanel.Children.Add(AddRectangle);

            //Add line panel
            Line displayLine = new Line();
            displayLine.X1 = ShapeDisplaySize.Width;
            displayLine.Y1 = 30.0;
            displayLine.X2 = 0.0;
            displayLine.Y2 = ShapeDisplaySize.Height + 30.0;
            displayLine.StrokeThickness = 8;
            displayLine.Stroke = new SolidColorBrush(Color.FromArgb(255, 52, 173, 79));
            AddLine = new AddCompPanel(AddCompPanel.eShapeType.LINE, displayLine,
                "The Line component allows you to add linear structure to your scene."
            );
            AddLine.Visibility = Visibility.Collapsed;
            CompPanel.Children.Add(AddLine);

            //Add particle panel
            Ellipse displayEllipse = new Ellipse();
            displayEllipse.Width = Math.Min(ShapeDisplaySize.Width, ShapeDisplaySize.Height) * 0.9;
            displayEllipse.Height = displayEllipse.Width;
            displayEllipse.Fill = new SolidColorBrush(Color.FromArgb(255, 242, 80, 80));
            AddParticle = new AddCompPanel(AddCompPanel.eShapeType.ELLIPSE, displayEllipse,
                "The Particle component comes with physics and collision capabilities."
            );
            AddParticle.Visibility = Visibility.Collapsed;
            CompPanel.Children.Add(AddParticle);

            //Add particle ejector panel
            Rectangle displayEjector = new Rectangle();
            displayEjector.Width = ShapeDisplaySize.Width * 0.7;
            displayEjector.Height = ShapeDisplaySize.Height * 0.8;
            RotateTransform ejectorRotate = new RotateTransform();
            ejectorRotate.Angle = 165.0;
            ejectorRotate.CenterX = ShapeDisplaySize.Width / 2.0;
            ejectorRotate.CenterY = ShapeDisplaySize.Height / 2.0;
            displayEjector.RenderTransform = ejectorRotate;
            displayEjector.StrokeThickness = 3.0;
            displayEjector.Stroke = new SolidColorBrush(Colors.Black);
            displayEjector.Fill = new SolidColorBrush(Color.FromArgb(255, 169, 80, 242));
            AddEjector = new AddCompPanel(AddCompPanel.eShapeType.EJECTOR, displayEjector,
                "The Particle Ejector shoots particles with customizable properties."
            );
            AddEjector.Visibility = Visibility.Collapsed;
            CompPanel.Children.Add(AddEjector);

            Children.Add(CompPanel);
        }

        private void ExpandBoard_Completed(object sender, object e)
        {
            AddRectangle.Visibility = Visibility.Visible;
            AddLine.Visibility = Visibility.Visible;
            AddParticle.Visibility = Visibility.Visible;
            AddEjector.Visibility = Visibility.Visible;
        }

        public override void ToggleMenuExpanded()
        {
            base.ToggleMenuExpanded();

            if (!IsMenuExpanded)
            {
                AddRectangle.Visibility = Visibility.Collapsed;
                AddLine.Visibility = Visibility.Collapsed;
                AddParticle.Visibility = Visibility.Collapsed;
                AddEjector.Visibility = Visibility.Collapsed;
            }
        }
    }
}