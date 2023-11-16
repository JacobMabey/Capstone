

using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

namespace PhysicsEngine.UI_Menus
{
    public class VerticalMenu : Canvas
    {
        //Animation
        protected Storyboard ExpandBoard = new Storyboard();
        protected Storyboard ContractBoard = new Storyboard();

        private double expandDuration = 0.2;
        private double contractDuration = 0.1;

        public double MenuWidth { get; set; }
        public double MenuX { get; set; }

        public bool IsMenuExpanded { get; set; } = false;

        public virtual void Initialize(double width, double menuX, Color bgColor)
        {
            ScrollViewer scroll = new ScrollViewer();
            scroll.Content = this;

            Canvas.SetZIndex(this, 99);
            MenuWidth = width;
            Height = Scene.MainScene.Height;
            Width = 0;
            MenuX = menuX;
            Canvas.SetTop(this, 0);
            Canvas.SetLeft(this, MenuX);

            //Add Background
            Rectangle bgRect = new Rectangle();
            bgRect.Width = Width;
            bgRect.Height = Height;
            bgRect.Fill = new SolidColorBrush(bgColor);
            bgRect.Stroke = new SolidColorBrush(Colors.Black);
            bgRect.StrokeThickness = 1;
            Children.Add(bgRect);

            //Expand Animation
            DoubleAnimation expand = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(expandDuration),
                From = 0,
                To = MenuWidth
            };
            expand.EnableDependentAnimation = true;
            Storyboard.SetTarget(expand, this);
            Storyboard.SetTargetProperty(expand, "Canvas.Width");

            DoubleAnimation expandBg = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(expandDuration),
                From = 0,
                To = MenuWidth
            };
            expandBg.EnableDependentAnimation = true;
            Storyboard.SetTarget(expandBg, bgRect);
            Storyboard.SetTargetProperty(expandBg, "Width");

            DoubleAnimation expandTop = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(expandDuration),
                From = MenuX,
                To = MenuX - MenuWidth
            };
            expandTop.EnableDependentAnimation = true;
            Storyboard.SetTarget(expandTop, this);
            Storyboard.SetTargetProperty(expandTop, "Canvas.Left");

            ExpandBoard = new Storyboard();
            ExpandBoard.Duration = expand.Duration;
            ExpandBoard.Children.Add(expand);
            ExpandBoard.Children.Add(expandBg);
            ExpandBoard.Children.Add(expandTop);


            //Contract Animation
            DoubleAnimation contract = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(contractDuration),
                From = MenuWidth,
                To = 0
            };
            contract.EnableDependentAnimation = true;
            Storyboard.SetTarget(contract, this);
            Storyboard.SetTargetProperty(contract, "Canvas.Width");

            DoubleAnimation contractBg = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(contractDuration),
                From = MenuWidth,
                To = 0
            };
            contractBg.EnableDependentAnimation = true;
            Storyboard.SetTarget(contractBg, bgRect);
            Storyboard.SetTargetProperty(contractBg, "Width");

            DoubleAnimation contractTop = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(contractDuration),
                From = MenuX - MenuWidth,
                To = MenuX
            };
            contractTop.EnableDependentAnimation = true;
            Storyboard.SetTarget(contractTop, this);
            Storyboard.SetTargetProperty(contractTop, "Canvas.Left");

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
            Height = Scene.MainScene.Height;
        }
    }
}