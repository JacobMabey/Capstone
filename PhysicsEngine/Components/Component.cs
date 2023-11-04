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
        private static long U_Id = 0;
        public long ID = -1;

        public virtual Coord Position { get; set; }
        public bool IsCollisionEnabled { get; set; } = true;
        protected bool IsBeingDragged { get; set; } = false;

        //Is true during drag if user is holding ctrl. will freely move component with no physics and snap to grid
        protected bool IsMouseDragMode { get; set; } = false;
        protected bool IsMouseRotateMode { get; set; } = false;
        protected Coord PointerDragPoint { get; set; } = new Coord(0, 0);
        protected SolidColorBrush FillBrush { get; set; }
        protected SolidColorBrush StrokeBrush { get; set; }

        protected bool HasPhysics { get; set; } = false;
        public Physics Phys { get; private set; }

        public abstract Shape GetUIElement();

        public virtual void Initialize()
        {
            ID = U_Id++;
            Phys = new Physics(this);
            if (this is Particle)
                HasPhysics = true;
        }
        public virtual void Update()
        {
            if (HasPhysics && !IsBeingDragged)
                Phys.Update();
        }
    }
}
