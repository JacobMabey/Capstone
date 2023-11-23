using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace PhysicsEngine
{
    public abstract class Component
    {
        protected static long U_Id { get; set; } = 0;
        public long ID = -1;

        public virtual Color Fill { get; set; }
        public virtual Coord Position { get; set; }
        public bool IsCollisionEnabled { get; set; } = true;
        public bool IsBeingDragged { get; set; } = false;
        public bool IsBeingAdded { get; set; } = false;

        //Is true during drag if user is holding ctrl. will freely move component with no physics and snap to grid
        public bool IsMouseDragMode { get; set; } = false;
        protected bool IsMouseRotateMode { get; set; } = false;
        public Coord PointerDragPoint { get; set; } = new Coord(0, 0);
        public SolidColorBrush FillBrush { get; set; }
        public LinearGradientBrush GradientFillBrush { get; set; }
        public SolidColorBrush StrokeBrush { get; set; }

        public bool HasPhysics { get; set; } = false;
        public Physics Phys { get; protected set; }

        public abstract Shape GetUIElement();

        public abstract Component Clone();
        public abstract string GetSaveText();

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

        protected void OpenCompMenu()
        {
            Scene.CompMenu.ParentComponent = this;
            Scene.CompMenu.ReloadSettings();
            if (!Scene.CompMenu.IsMenuExpanded)
                Scene.CompMenu.ToggleMenuExpanded();

            //If other menus are open, close them
            if (Scene.WorldMenu.IsMenuExpanded)
                Scene.WorldMenu.ToggleMenuExpanded();

            if (Scene.AddMenu.IsMenuExpanded)
                Scene.AddMenu.ToggleMenuExpanded();
        }
    }
}
