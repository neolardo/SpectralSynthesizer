using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SpectralSynthesizer
{
    /// <summary>
    /// An optimized version of the <see cref="TimeDomainViewModel"/> that only shows a small part of the content,
    /// and refreshes it only when the view needs that data.
    /// </summary>
    public abstract class OptimizedTimeDomainViewModel : TimeDomainViewModel
    {
        #region Properties

        /// <summary>
        /// The visible part of the view
        /// </summary>
        protected double VisibleWidth { get; set; }

        /// <summary>
        /// That part of the view that has some data in it
        /// </summary>
        public double ContentWidth { get; set; }

        /// <summary>
        /// The margin of the content inside the scrollviewer
        /// </summary>
        public Thickness ContentMargin { get; set; } = new Thickness(0, 0, 0, 0);

        /// <summary>
        /// Indicates how big is the content part compared to the visible part.
        /// Should be at least 1, numbers too big, or too small might result in performance issues.
        /// </summary>
        protected double ContentScaleMultiplier { get; set; } = 2;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            VerticalOffset = e.VerticalOffset;
            HorizontalOffset = e.HorizontalOffset;
            RefreshOnScrollIfOutOfContent();
            InvokeScrolled(HorizontalOffset, VerticalOffset);
        }

        /// <summary>
        /// Refreshes the view on scrolling when the content's end has been reached.
        /// </summary>
        private void RefreshOnScrollIfOutOfContent()
        {
            if (HorizontalOffset < ContentMargin.Left) // out of left side
            {
                double center = HorizontalOffset + VisibleWidth / 2.0;
                double marginleft = center - ContentWidth / 2.0;
                if (marginleft < 0)
                    marginleft = 0;
                ContentMargin = new Thickness(marginleft, 0, Length - ContentWidth - marginleft, 0);
                RefreshOnScroll();
            }
            else if (HorizontalOffset + VisibleWidth > ContentMargin.Left + ContentWidth) // out of right side
            {
                double center = HorizontalOffset + VisibleWidth / 2.0;
                double marginleft = center - ContentWidth / 2.0;
                if (Length < marginleft + ContentWidth)
                    marginleft = Length - ContentWidth;
                ContentMargin = new Thickness(marginleft, 0, Length - ContentWidth - marginleft, 0);
                RefreshOnScroll();
            }
        }

        /// <summary>
        /// Refreshes the view when the content's end has reached while scrolling.
        /// </summary>
        protected abstract new void RefreshOnScroll();

        /// <inheritdoc/>
        public override void OnScrollViewerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var oldWidth = ScrollWidth;
            var oldHeight = ScrollHeight;
            var centerRatio = GetCenterRatio();
            var scroll = sender as ScrollViewer;
            ScrollWidth = scroll.ActualWidth;
            ScrollHeight = scroll.ActualHeight;
            VisibleWidth = scroll.ActualWidth;
            if (Math.Abs(oldHeight - ScrollHeight) > Computer.CompareDelta || Math.Abs(oldWidth - ScrollWidth) > Computer.CompareDelta)
            {
                RefreshScrollViewerOffset(centerRatio.horizontal, centerRatio.vertical);
                RefreshOnScrollViewerSizeChanged();
            }
        }

        /// <summary>
        /// Refreshes the view when the content's end has reached while size changing.
        /// </summary>
        protected abstract new void RefreshOnScrollViewerSizeChanged();

        /// <inheritdoc/>
        protected override void RefreshScrollViewerOffset(double horizontalCenterRatio, double verticalCenterRatio)
        {
            double newVerticalOffset = verticalCenterRatio * Height - ScrollHeight / 2;
            if (newVerticalOffset < 0)
                newVerticalOffset = 0;
            if (newVerticalOffset > Height - ScrollHeight / 2)
                newVerticalOffset = Height - ScrollHeight / 2;

            VisibleWidth = ScrollWidth;
            ContentWidth = VisibleWidth * ContentScaleMultiplier;
            if (Length < Computer.CompareDelta)
                return;
            if (VisibleWidth > Length)
            {
                VisibleWidth = Length;
                ContentWidth = Length;
                ContentMargin = new Thickness(0, 0, 0, 0);
                InvokeScrollToOffset(0, newVerticalOffset);
            }
            else if (ContentWidth > Length)
            {
                ContentWidth = Length;
                ContentMargin = new Thickness(0, 0, 0, 0);
                double newOffset = (horizontalCenterRatio * Length) - (VisibleWidth / 2.0);
                if (newOffset < 0)
                    newOffset = 0;
                else if (newOffset + VisibleWidth > Length)
                    newOffset = Length - VisibleWidth;
                InvokeScrollToOffset(newOffset, newVerticalOffset);
            }
            else
            {
                double newOffset = (horizontalCenterRatio * Length) - (VisibleWidth / 2.0);
                if (newOffset < 0)
                    newOffset = 0;
                else if (newOffset + VisibleWidth > Length)
                    newOffset = Length - VisibleWidth;

                double newMarginLeft = newOffset + (VisibleWidth / 2.0) - (ContentWidth / 2.0);
                if (newMarginLeft < 0)
                    newMarginLeft = 0;
                else if (newMarginLeft + ContentWidth > Length)
                    newMarginLeft = Length - ContentWidth;
                ContentMargin = new Thickness(newMarginLeft, 0, Length - newMarginLeft - ContentWidth, 0);
                InvokeScrollToOffset(newOffset, newVerticalOffset);
            }
        }

        /// <inheritdoc/>
        protected override (double horizontal, double vertical) GetCenterRatio()
        {
            double horizontal = Length < Computer.CompareDelta ? 0 : (HorizontalOffset + (VisibleWidth / 2.0)) / Length;
            double vertical = Height < Computer.CompareDelta ? 0 : (VerticalOffset + (ScrollHeight / 2.0)) / Height;
            return (horizontal, vertical);
        }

        /// <inheritdoc/>
        protected override (double horizontal, double vertical) GetCenterRatio(double length, double height)
        {
            double horizontal = length < Computer.CompareDelta ? 0 : (HorizontalOffset + (VisibleWidth / 2.0)) / length;
            double vertical = height < Computer.CompareDelta ? 0 : (VerticalOffset + (ScrollHeight / 2.0)) / height;
            return (horizontal, vertical);
        }

        #endregion

    }
}
