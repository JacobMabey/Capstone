

using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

namespace PhysicsEngine.UI_Menus
{
    public enum eMenuType
    {
        HORIZONTAL,
        VERTICAL
    }
    public class HorizontalMenu : Canvas
    {
        //Toobar border
        private Rectangle rectBorder;

        //Animation
        protected Storyboard ExpandBoard = new Storyboard();
        protected Storyboard ContractBoard = new Storyboard();

        public double MenuHeight { get; set; }
        public double MenuY { get; set; }

        public bool IsMenuExpanded { get; set; } = false;

        public virtual void Initialize(double height, double menuY, Color bgColor)
        {
            Canvas.SetZIndex(this, 99);
            MenuHeight = height;
            Width = Scene.MainScene.Width;
            Height = 0;
            MenuY = menuY;
            Canvas.SetTop(this, MenuY);
            Canvas.SetLeft(this, 0);

            //Add Background
            Rectangle bgRect = new Rectangle();
            bgRect.Width = Width;
            bgRect.Height = Height;
            bgRect.Fill = new SolidColorBrush(bgColor);
            Children.Add(bgRect);

            //Expand Animation
            DoubleAnimation expand = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(0.3),
                From = 0,
                To = MenuHeight
            };
            expand.EnableDependentAnimation = true;
            Storyboard.SetTarget(expand, this);
            Storyboard.SetTargetProperty(expand, "Canvas.Height");

            DoubleAnimation expandBg = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(0.3),
                From = 0,
                To = MenuHeight
            };
            expandBg.EnableDependentAnimation = true;
            Storyboard.SetTarget(expandBg, bgRect);
            Storyboard.SetTargetProperty(expandBg, "Height");

            DoubleAnimation expandTop = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(0.3),
                From = MenuY,
                To = MenuY - MenuHeight
            };
            expandTop.EnableDependentAnimation = true;
            Storyboard.SetTarget(expandTop, this);
            Storyboard.SetTargetProperty(expandTop, "Canvas.Top");

            ExpandBoard = new Storyboard();
            ExpandBoard.Duration = expand.Duration;
            ExpandBoard.Children.Add(expand);
            ExpandBoard.Children.Add(expandBg);
            ExpandBoard.Children.Add(expandTop);


            //Contract Animation
            DoubleAnimation contract = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(0.15),
                From = MenuHeight,
                To = 0
            };
            contract.EnableDependentAnimation = true;
            Storyboard.SetTarget(contract, this);
            Storyboard.SetTargetProperty(contract, "Canvas.Height");

            DoubleAnimation contractBg = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(0.15),
                From = MenuHeight,
                To = 0
            };
            contractBg.EnableDependentAnimation = true;
            Storyboard.SetTarget(contractBg, bgRect);
            Storyboard.SetTargetProperty(contractBg, "Height");

            DoubleAnimation contractTop = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(0.15),
                From = MenuY - MenuHeight,
                To = MenuY
            };
            contractTop.EnableDependentAnimation = true;
            Storyboard.SetTarget(contractTop, this);
            Storyboard.SetTargetProperty(contractTop, "Canvas.Top");

            ContractBoard = new Storyboard();
            ContractBoard.Duration = contract.Duration;
            ContractBoard.Children.Add(contract);
            ContractBoard.Children.Add(contractBg);
            ContractBoard.Children.Add(contractTop);
        }

        public virtual void ToggleMenuExpanded()
        {
            if (ContractBoard.GetCurrentState() == ClockState.Active || ExpandBoard.GetCurrentState() == ClockState.Active)
                return;

            if (IsMenuExpanded)
                ContractBoard.Begin();
            else
                ExpandBoard.Begin();
            IsMenuExpanded = !IsMenuExpanded;
        }

        public virtual void ResetPosition()
        {

        }
    }
}