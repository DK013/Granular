﻿using System;
using System.Collections.Generic;
using System.Linq;
using Granular.Extensions;

namespace System.Windows.Controls
{
    public class Grid : Panel
    {
        public static readonly DependencyProperty RowProperty = DependencyProperty.RegisterAttached("Row", typeof(int), typeof(Grid), new FrameworkPropertyMetadata());

        public static int GetRow(DependencyObject obj)
        {
            return (int)obj.GetValue(RowProperty);
        }

        public static void SetRow(DependencyObject obj, int value)
        {
            obj.SetValue(RowProperty, value);
        }

        public static readonly DependencyProperty ColumnProperty = DependencyProperty.RegisterAttached("Column", typeof(int), typeof(Grid), new FrameworkPropertyMetadata());

        public static int GetColumn(DependencyObject obj)
        {
            return (int)obj.GetValue(ColumnProperty);
        }

        public static void SetColumn(DependencyObject obj, int value)
        {
            obj.SetValue(ColumnProperty, value);
        }

        public static readonly DependencyProperty RowSpanProperty = DependencyProperty.RegisterAttached("RowSpan", typeof(int), typeof(Grid), new FrameworkPropertyMetadata(1));

        public static int GetRowSpan(DependencyObject obj)
        {
            return (int)obj.GetValue(RowSpanProperty);
        }

        public static void SetRowSpan(DependencyObject obj, int value)
        {
            obj.SetValue(RowSpanProperty, value);
        }

        public static readonly DependencyProperty ColumnSpanProperty = DependencyProperty.RegisterAttached("ColumnSpan", typeof(int), typeof(Grid), new FrameworkPropertyMetadata(1));

        public static int GetColumnSpan(DependencyObject obj)
        {
            return (int)obj.GetValue(ColumnSpanProperty);
        }

        public static void SetColumnSpan(DependencyObject obj, int value)
        {
            obj.SetValue(ColumnSpanProperty, value);
        }

        public FreezableCollection<RowDefinition> RowDefinitions { get; private set; }
        public FreezableCollection<ColumnDefinition> ColumnDefinitions { get; private set; }

        private IDefinitionBase[] defaultRowDefinitions;
        private IDefinitionBase[] defaultColumnDefinitions;

        public Grid()
        {
            RowDefinitions = new FreezableCollection<RowDefinition>();
            RowDefinitions.SetInheritanceParent(this);

            ColumnDefinitions = new FreezableCollection<ColumnDefinition>();
            ColumnDefinitions.SetInheritanceParent(this);

            defaultRowDefinitions = new [] { new RowDefinition() };
            defaultColumnDefinitions = new [] { new ColumnDefinition() };
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            IDefinitionBase[] currentRowDefinitions = RowDefinitions.Count == 0 ? defaultRowDefinitions : RowDefinitions.ToArray();
            IDefinitionBase[] currentColumnDefinitions = ColumnDefinitions.Count == 0 ? defaultColumnDefinitions : ColumnDefinitions.ToArray();

            double[] rowsLength = currentRowDefinitions.Select(definitionBase => definitionBase.Length.IsAbsolute ? definitionBase.Length.Value : 0).ToArray();
            double[] columnsLength = currentColumnDefinitions.Select(definitionBase => definitionBase.Length.IsAbsolute ? definitionBase.Length.Value : 0).ToArray();

            foreach (FrameworkElement child in Children)
            {
                child.Measure(new Size(
                    GetMeasureLength(currentColumnDefinitions, availableSize.Width, GetColumn(child), GetColumnSpan(child)),
                    GetMeasureLength(currentRowDefinitions, availableSize.Height, GetRow(child), GetRowSpan(child))));

                int row = GetRow(child);
                int column = GetColumn(child);

                if (GetRowSpan(child) == 1 && (currentRowDefinitions[row].Length.IsAuto || currentRowDefinitions[row].Length.IsStar))
                {
                    rowsLength[row] = Math.Max(rowsLength[row], child.DesiredSize.Height);
                }

                if (GetColumnSpan(child) == 1 && (currentColumnDefinitions[column].Length.IsAuto || currentColumnDefinitions[column].Length.IsStar))
                {
                    columnsLength[column] = Math.Max(columnsLength[column], child.DesiredSize.Width);
                }
            }

            SetBoundedValues(currentRowDefinitions, ref rowsLength);
            SetBoundedValues(currentColumnDefinitions, ref columnsLength);

            return new Size(columnsLength.Sum(), rowsLength.Sum());
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            IDefinitionBase[] currentRowDefinitions = RowDefinitions.Count == 0 ? defaultRowDefinitions : RowDefinitions.ToArray();
            IDefinitionBase[] currentColumnDefinitions = ColumnDefinitions.Count == 0 ? defaultColumnDefinitions : ColumnDefinitions.ToArray();

            double[] rowsLength = currentRowDefinitions.Select(definitionBase => definitionBase.Length.IsAbsolute ? definitionBase.Length.Value : 0).ToArray();
            double[] columnsLength = currentColumnDefinitions.Select(definitionBase => definitionBase.Length.IsAbsolute ? definitionBase.Length.Value : 0).ToArray();

            foreach (FrameworkElement child in Children)
            {
                int row = GetRow(child);
                int column = GetColumn(child);

                if (GetRowSpan(child) == 1 && currentRowDefinitions[row].Length.IsAuto)
                {
                    rowsLength[row] = Math.Max(rowsLength[row], child.DesiredSize.Height);
                }

                if (GetColumnSpan(child) == 1 && currentColumnDefinitions[column].Length.IsAuto)
                {
                    columnsLength[column] = Math.Max(columnsLength[column], child.DesiredSize.Width);
                }
            }

            double rowStarLength = GetStarLength(currentRowDefinitions, finalSize.Height - rowsLength.Sum());
            double columnStarLength = GetStarLength(currentColumnDefinitions, finalSize.Width - columnsLength.Sum());

            SetStarLengths(currentRowDefinitions, rowStarLength, ref rowsLength);
            SetStarLengths(currentColumnDefinitions, columnStarLength, ref columnsLength);

            SetBoundedValues(currentRowDefinitions, ref rowsLength);
            SetBoundedValues(currentColumnDefinitions, ref columnsLength);

            SetActualLength(currentRowDefinitions, rowsLength);
            SetActualLength(currentColumnDefinitions, columnsLength);

            foreach (FrameworkElement child in Children)
            {
                child.Arrange(new Rect(
                    columnsLength.Take(GetColumn(child)).Sum(),
                    rowsLength.Take(GetRow(child)).Sum(),
                    columnsLength.Skip(GetColumn(child)).Take(GetColumnSpan(child)).Sum(),
                    rowsLength.Skip(GetRow(child)).Take(GetRowSpan(child)).Sum()));
            }

            return finalSize;
        }

        private static double GetMeasureLength(IDefinitionBase[] definitionBases, double availableLength, double start, double span)
        {
            double remainingLength = availableLength;
            double absoluteLength = 0;
            bool allAbsolute = true;

            for (int i = 0; i < definitionBases.Length; i++)
            {
                if (i >= start && i < start + span)
                {
                    if (definitionBases[i].Length.IsAbsolute)
                    {
                        absoluteLength += (double)definitionBases[i].Length.Value;
                    }
                    else
                    {
                        allAbsolute = false;
                    }
                }
                else if (definitionBases[i].Length.IsAbsolute)
                {
                    remainingLength -= (double)definitionBases[i].Length.Value;
                }
            }

            return allAbsolute ? absoluteLength : Math.Max(0, remainingLength);
        }

        [System.Runtime.CompilerServices.Reflectable(false)]
        private static void SetBoundedValues(IDefinitionBase[] definitionBases, ref double[] lengths)
        {
            for (int i = 0; i < lengths.Length; i++)
            {
                lengths[i] = lengths[i].Bounds(definitionBases[i].MinLength, definitionBases[i].MaxLength);
            }
        }

        [System.Runtime.CompilerServices.Reflectable(false)]
        private static void SetStarLengths(IDefinitionBase[] definitionBases, double starLength, ref double[] lengths)
        {
            for (int i = 0; i < definitionBases.Length; i++)
            {
                if (definitionBases[i].Length.IsStar)
                {
                    lengths[i] = definitionBases[i].Length.Value * starLength;
                }
            }
        }

        private static void SetActualLength(IDefinitionBase[] definitionBases, double[] actualLengths)
        {
            for (int i = 0; i < definitionBases.Length; i++)
            {
                definitionBases[i].ActualLength = actualLengths[i];
            }
        }

        private static double GetStarLength(IEnumerable<IDefinitionBase> definitionBases, double totalStarsLength)
        {
            // each axis starred length is a bounded function:
            // axis.GetStarredLength(starLength) => (axis.Length.Value * starLength).Bounds(axis.MinLength, axis.MaxLength)

            // find a starLength where:
            // definitionBases.Sum(axis => axis.GetStarredLength(starLength)) == totalStarsLength

            IEnumerable<IDefinitionBase> starredAxis = definitionBases.Where(axis => axis.Length.IsStar);

            if (starredAxis.Count() == 0 || totalStarsLength <= 0)
            {
                return 0;
            }

            double[] bounds = starredAxis.Select(axis => axis.MinLength / axis.Length.Value).Union(starredAxis.Select(axis => axis.MaxLength / axis.Length.Value)).ToArray();

            double smallerBound = bounds.Where(vertex => starredAxis.Sum(axis => GetStarredAxisLength(axis, vertex)) <= totalStarsLength).DefaultIfEmpty(Double.NaN).Max();
            double largerBound = bounds.Where(vertex => starredAxis.Sum(axis => GetStarredAxisLength(axis, vertex)) >= totalStarsLength).DefaultIfEmpty(Double.NaN).Min();

            if (!Double.IsNaN(smallerBound) && !Double.IsNaN(largerBound))
            {
                if (smallerBound == largerBound)
                {
                    return smallerBound;
                }

                if (Granular.Compatibility.Double.IsInfinity(largerBound))
                {
                    double totalSmallerStarsLength = starredAxis.Where(axis => axis.MaxLength <= axis.Length.Value * smallerBound).Sum(axis => GetStarredAxisLength(axis, smallerBound));
                    double totalLargerStars = starredAxis.Where(axis => axis.MaxLength > axis.Length.Value * smallerBound).Sum(axis => axis.Length.Value);

                    return (totalStarsLength - totalSmallerStarsLength) / totalLargerStars;
                }

                double smallerBoundTotalLength = starredAxis.Sum(axis => GetStarredAxisLength(axis, smallerBound));
                double largerBoundTotalLength = starredAxis.Sum(axis => GetStarredAxisLength(axis, largerBound));

                return smallerBound + (largerBound - smallerBound) * (totalStarsLength - smallerBoundTotalLength) / (largerBoundTotalLength - smallerBoundTotalLength);
            }

            return smallerBound.DefaultIfNaN(largerBound).DefaultIfNaN(0);
        }

        private static double GetStarredAxisLength(IDefinitionBase starredAxis, double starLength)
        {
            return (starredAxis.Length.Value * starLength).Bounds(starredAxis.MinLength, starredAxis.MaxLength);
        }
    }
}