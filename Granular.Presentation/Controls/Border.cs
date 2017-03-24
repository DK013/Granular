﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace System.Windows.Controls
{
    public class Border : Decorator
    {
        public static readonly DependencyProperty BackgroundProperty = Panel.BackgroundProperty.AddOwner(typeof(Border), new FrameworkPropertyMetadata(null, (sender, e) => ((Border)sender).borderRenderElements.SetRenderElementsProperty(renderElement => renderElement.Background = (Brush)e.NewValue)));
        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(Border), new FrameworkPropertyMetadata(null, (sender, e) => ((Border)sender).borderRenderElements.SetRenderElementsProperty(renderElement => renderElement.BorderBrush = (Brush)e.NewValue)));
        public Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(Border), new FrameworkPropertyMetadata(Thickness.Zero, FrameworkPropertyMetadataOptions.AffectsMeasure, propertyChangedCallback: (sender, e) => ((Border)sender).borderRenderElements.SetRenderElementsProperty(renderElement => renderElement.BorderThickness = (Thickness)e.NewValue)));
        public Thickness BorderThickness
        {
            get { return (Thickness)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(Border), new FrameworkPropertyMetadata(CornerRadius.Zero, (sender, e) => ((Border)sender).borderRenderElements.SetRenderElementsProperty(renderElement => renderElement.CornerRadius = (CornerRadius)e.NewValue)));
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register("Padding", typeof(Thickness), typeof(Border), new FrameworkPropertyMetadata(Thickness.Zero, FrameworkPropertyMetadataOptions.AffectsMeasure));
        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        private RenderElementDictionary<IBorderRenderElement> borderRenderElements;

        static Border()
        {
            UIElement.IsHitTestVisibleProperty.OverrideMetadata(typeof(Border), new FrameworkPropertyMetadata(propertyChangedCallback: (sender, e) => ((Border)sender).borderRenderElements.SetRenderElementsProperty(renderElement => renderElement.IsHitTestVisible = (bool)e.NewValue)));
        }

        public Border()
        {
            borderRenderElements = new RenderElementDictionary<IBorderRenderElement>(CreateRenderElement);
        }

        protected override object CreateContentRenderElementOverride(IRenderElementFactory factory)
        {
            return borderRenderElements.GetRenderElement(factory);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return base.MeasureOverride((availableSize - BorderThickness.Size - Padding.Size).Max(Size.Zero)) + BorderThickness.Size + Padding.Size;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Child != null)
            {
                Child.Arrange(new Rect(BorderThickness.Location + Padding.Location, (finalSize - BorderThickness.Size - Padding.Size).Max(Size.Zero)));
            }

            foreach (IBorderRenderElement borderRenderElement in borderRenderElements.Elements)
            {
                borderRenderElement.Bounds = new Rect(finalSize);
            }

            return finalSize;
        }

        protected override bool HitTestOverride(Point position)
        {
            return VisualSize.Contains(position) &&
                (Background != null || BorderBrush != null && IsOverBorder(position, VisualSize, BorderThickness, CornerRadius));
        }

        private static bool IsOverBorder(Point position, Size borderSize, Thickness borderTickness, CornerRadius cornerRadius)
        {
            return position.X < borderTickness.Left ||
                position.Y < borderTickness.Top ||
                borderSize.Width - position.X < borderTickness.Right ||
                borderSize.Height - position.Y < borderTickness.Bottom; // cornerRadius is ignored
        }

        private IBorderRenderElement CreateRenderElement(IRenderElementFactory factory)
        {
            IBorderRenderElement borderRenderElement = factory.CreateBorderRenderElement(this);

            borderRenderElement.Background = Background;
            borderRenderElement.BorderBrush = BorderBrush;
            borderRenderElement.BorderThickness = BorderThickness;
            borderRenderElement.Bounds = new Rect(VisualSize);
            borderRenderElement.CornerRadius = CornerRadius;
            borderRenderElement.IsHitTestVisible = IsHitTestVisible;

            return borderRenderElement;
        }
    }
}
