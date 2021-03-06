﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Controls
{
    using System;

    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    /// <summary>
    /// Extensions methods for ItemsControl control.
    /// </summary>
    public static class ItemsControlExtensions
    {
        /// <summary>
        /// Scroll to horizontal offset <paramref name="horizontalOffset"/>.
        /// </summary>
        /// <param name="frameworkElement">
        /// The list view base control.
        /// </param>
        /// <param name="horizontalOffset">
        /// The horizontal offset.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="frameworkElement"/> is null.
        /// </exception>
        public static void ScrollToHorizontalOffset(this FrameworkElement frameworkElement, double horizontalOffset)
        {
            if (frameworkElement == null)
            {
                throw new ArgumentNullException("frameworkElement");
            }

            var scrollViewer = VisualTreeHelperEx.GetVisualChild<ScrollViewer>(frameworkElement);
            if (scrollViewer != null)
            {
                if (scrollViewer.HorizontalScrollMode != ScrollMode.Disabled)
                {
                    scrollViewer.ChangeView(horizontalOffset, null, null);
                }
            }
        }

        /// <summary>
        /// The scroll to vertical offset <paramref name="verticalOffset"/>.
        /// </summary>
        /// <param name="frameworkElement">
        /// The list view base.
        /// </param>
        /// <param name="verticalOffset">
        /// The vertical offset.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="frameworkElement"/> is null.
        /// </exception>
        public static void ScrollToVerticalOffset(this FrameworkElement frameworkElement, double verticalOffset)
        {
            if (frameworkElement == null)
            {
                throw new ArgumentNullException("frameworkElement");
            }

            var scrollViewer = VisualTreeHelperEx.GetVisualChild<ScrollViewer>(frameworkElement);
            if (scrollViewer != null)
            {
                if (scrollViewer.VerticalScrollMode != ScrollMode.Disabled)
                {
                    scrollViewer.ChangeView(null, verticalOffset, null);
                }
            }
        }

        /// <summary>
        /// Scroll to horizontal zero.
        /// </summary>
        /// <param name="frameworkElement">
        /// The list view base control.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="frameworkElement"/> is null.
        /// </exception>
        public static void ScrollToHorizontalZero(this FrameworkElement frameworkElement)
        {
            ScrollToHorizontalOffset(frameworkElement, 0.0d);
        }

        /// <summary>
        /// Scroll to vertical zero.
        /// </summary>
        /// <param name="frameworkElement">
        /// The list view base control.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="frameworkElement"/> is null.
        /// </exception>
        public static void ScrollToVerticalZero(this FrameworkElement frameworkElement)
        {
            ScrollToVerticalOffset(frameworkElement, 0.0d);
        }

        /// <summary>
        /// Get scroll viewer horizontal offset.
        /// </summary>
        /// <param name="frameworkElement">
        /// The list view base control.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="frameworkElement"/> is null.
        /// </exception>
        public static double GetScrollViewerHorizontalOffset(this FrameworkElement frameworkElement)
        {
            if (frameworkElement == null)
            {
                throw new ArgumentNullException("frameworkElement");
            }

            var scrollViewer = VisualTreeHelperEx.GetVisualChild<ScrollViewer>(frameworkElement);
            if (scrollViewer != null)
            {
                return scrollViewer.HorizontalOffset;
            }

            return 0.0d;
        }

        /// <summary>
        /// Get scroll viewer vertical offset.
        /// </summary>
        /// <param name="frameworkElement">
        /// The list view base control.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="frameworkElement"/> is null.
        /// </exception>
        public static double GetScrollViewerVerticalOffset(this FrameworkElement frameworkElement)
        {
            if (frameworkElement == null)
            {
                throw new ArgumentNullException("frameworkElement");
            }

            var scrollViewer = VisualTreeHelperEx.GetVisualChild<ScrollViewer>(frameworkElement);
            if (scrollViewer != null)
            {
                return scrollViewer.VerticalOffset;
            }

            return 0.0d;
        }
    }
}