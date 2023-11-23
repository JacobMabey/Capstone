using System;
using Windows.UI;
using Windows.UI.Core;
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
        //Animation
        protected Storyboard ExpandBoard = new Storyboard();
        protected Storyboard ContractBoard = new Storyboard();

        private double expandDuration = 0.2;
        private double contractDuration = 0.1;

        public double MenuHeight { get; set; }
        public double MenuY { get; set; }

        public bool IsMenuExpanded { get; set; } = false;

        public virtual void Initialize(double height, double menuY, Color bgColor)
        {
            ScrollViewer scroll = new ScrollViewer();
            scroll.Content = this;

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
            bgRect.Stroke = new SolidColorBrush(Colors.Black);
            bgRect.StrokeThickness = 1;
            Children.Add(bgRect);

            //Add Close Button
            TextBlock closeText = new TextBlock();
            closeText.Text = "v";
            ScaleTransform scale = new ScaleTransform
            {
                ScaleX = 2
            };
            closeText.RenderTransform = scale;
            Canvas.SetTop(closeText, 0);
            closeText.FontSize = 20;
            closeText.PointerEntered += (s, o) =>
            {
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);
            };
            closeText.PointerExited += (s, o) =>
            {
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
            };
            closeText.Tapped += (s, o) => {
                ToggleMenuExpanded();
            };
            closeText.Foreground = new SolidColorBrush(Colors.LightGray);
            closeText.FontFamily = MainPage.GlobalFont;
            Children.Add(closeText);

            //Expand Animation
            DoubleAnimation expand = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(expandDuration),
                From = 0,
                To = MenuHeight
            };
            expand.EnableDependentAnimation = true;
            Storyboard.SetTarget(expand, this);
            Storyboard.SetTargetProperty(expand, "Canvas.Height");

            DoubleAnimation expandBg = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(expandDuration),
                From = 0,
                To = MenuHeight
            };
            expandBg.EnableDependentAnimation = true;
            Storyboard.SetTarget(expandBg, bgRect);
            Storyboard.SetTargetProperty(expandBg, "Height");

            DoubleAnimation expandTop = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(expandDuration),
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
                Duration = TimeSpan.FromSeconds(contractDuration),
                From = MenuHeight,
                To = 0
            };
            contract.EnableDependentAnimation = true;
            Storyboard.SetTarget(contract, this);
            Storyboard.SetTargetProperty(contract, "Canvas.Height");

            DoubleAnimation contractBg = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(contractDuration),
                From = MenuHeight,
                To = 0
            };
            contractBg.EnableDependentAnimation = true;
            Storyboard.SetTarget(contractBg, bgRect);
            Storyboard.SetTargetProperty(contractBg, "Height");

            DoubleAnimation contractTop = new DoubleAnimation
            {
                Duration = TimeSpan.FromSeconds(contractDuration),
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
            if (IsMenuExpanded)
                ContractBoard.Begin();
            else
                ExpandBoard.Begin();
            IsMenuExpanded = !IsMenuExpanded;
        }

        public virtual void ResetPosition()
        {
            Width = Scene.MainScene.Width;
        }
    }
}