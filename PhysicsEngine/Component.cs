using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace PhysicsEngine
{
    public abstract class Component
    {
        protected bool IsBeingDragged { get; set; } = false;
        protected Coord PointerDragPoint { get; set; } = new Coord(0, 0);
        protected SolidColorBrush FillBrush { get; set; }
        protected SolidColorBrush StrokeBrush { get; set; }

        protected bool HasPhysics { get; set; } = false;

        public abstract Shape GetUIElement();
        public virtual void Update()
        {

        }
    }
}
