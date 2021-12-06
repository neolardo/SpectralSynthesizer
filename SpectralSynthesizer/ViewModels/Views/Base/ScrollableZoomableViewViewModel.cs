using System;
using System.Windows;
using System.Windows.Controls;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Viewmodel for views that have a main scrollviewer, therefore they can be scrolled and zoomed at.
    /// While zooming or changing size the content center stays in the scrollviwer's center.
    /// </summary>
    public abstract class ScrollableZoomableViewViewModel : ViewViewModel
    {
        #region Delegate and Events

        /// <summary>
        /// Delegate for fireing scrolling events.
        /// </summary>
        /// <param name="horizontalOffset">The new horizontal offset after scrolling.</param>
        /// <param name="verticalOffset">The new vertical offset after scrolling.</param>
        public delegate void ScrolledDelegate(double horizontalOffset, double verticalOffset);

        /// <summary>
        /// Delegate for fireing zooming events.
        /// </summary>
        /// <param name="pixelPerData">The pixel per data value after scrolling.</param>
        public delegate void ZoomedDelegate(double pixelPerData);

        /// <summary>
        /// Fires off after scrolling.
        /// </summary>
        public event ScrolledDelegate Scrolled;

        /// <summary>
        /// Fires off when scrolling from code behind. This view's scrollviewer should be subscribed to it.
        /// </summary>
        public event ScrolledDelegate ScrollToOffset;

        /// <summary>
        /// Event that fires off after zooming in or out.
        /// </summary>
        public event ZoomedDelegate Zoomed;

        /// <summary>
        /// Event that fires off after alternative zooming in or out.
        /// </summary>
        public event ZoomedDelegate AltZoomed;

        #endregion

        #region Properties

        /// <summary>
        /// The horizontal scroll offset of the view.
        /// </summary>
        public double HorizontalOffset { get; set; }

        /// <summary>
        /// The vertical scroll offset of the view.
        /// </summary>
        public double VerticalOffset { get; set; }

        /// <summary>
        /// The scrollviewer's width.
        /// </summary>
        protected double ScrollWidth { get; set; }

        /// <summary>
        /// The scrollviewer's height.
        /// </summary>
        protected double ScrollHeight { get; set; }

        #region Zooming Parameters

        /// <summary>
        /// The maximum value of the <see cref="PixelPerData"/>.
        /// </summary>
        protected double PPDMax { get; set; } = 1000;

        /// <summary>
        /// The minimum value of the <see cref="PixelPerData"/>.
        /// </summary>
        protected double PPDMin { get; set; } = 20;

        /// <summary>
        /// The ratio of the zooming.
        /// </summary>
        protected double ZoomRatio { get; set; } = 1.2;

        private double _pixelPerData = 200;

        /// <summary>
        /// The number of pixels that represent one data part.
        /// </summary>
        public double PixelPerData
        {
            get
            {
                return _pixelPerData;
            }
            protected set
            {
                if (value >= PPDMin && value <= PPDMax)
                {
                    if (Math.Abs(value - _pixelPerData) > Computer.CompareDelta)
                    {
                        _pixelPerData = value;
                        RefreshOnZoom();
                        Zoomed?.Invoke(_pixelPerData);
                    }
                }
            }
        }

        #endregion

        #region Alt Zooming Parameters

        /// <summary>
        /// The maximum value of the <see cref="AltPixelPerData"/>.
        /// </summary>
        protected double AltPPDMax { get; set; } = 8;

        /// <summary>
        /// The minimum value of the <see cref="AltPixelPerData"/>.
        /// </summary>
        protected double AltPPDMin { get; set; } = 1;

        /// <summary>
        /// The ratio of the alternative zooming.
        /// </summary>
        protected double AltZoomRatio { get; set; } = 2.0;

        private double _altPixelPerData = 2;

        /// <summary>
        /// The number of pixels that represent one data part.
        /// </summary>
        public double AltPixelPerData
        {
            get
            {
                return _altPixelPerData;
            }
            protected set
            {
                if (value >= AltPPDMin && value <= AltPPDMax)
                {
                    if (Math.Abs(value - _altPixelPerData) > Computer.CompareDelta)
                    {
                        _altPixelPerData = value;
                        RefreshOnAltZoom();
                        AltZoomed?.Invoke(_altPixelPerData);
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Gets the current ratio of the center of the scrollviewer.
        /// Should be called before a new <see cref="ViewViewModel.Length"/> or <see cref="ViewViewModel.Height"/> is set.
        /// </summary>
        /// <returns>The center ratio.</returns>
        protected virtual (double horizontal, double vertical) GetCenterRatio()
        {
            double horizontal = Length < Computer.CompareDelta ? 0 : (HorizontalOffset + (ScrollWidth / 2.0)) / Length;
            double vertical = Height < Computer.CompareDelta ? 0 : (VerticalOffset + (ScrollHeight / 2.0)) / Height;
            return (horizontal, vertical);
        }

        /// <summary>
        /// Gets the ratio of the center of the scrollviewer with the given length and height.
        /// </summary>
        /// <param name="length">The length of the view.</param>
        /// <param name="height">The height of the view.</param>
        /// <returns>The center ratio.</returns>
        protected virtual (double horizontal, double vertical) GetCenterRatio(double length, double height)
        {
            double horizontal = length < Computer.CompareDelta ? 0 : (HorizontalOffset + (ScrollWidth / 2.0)) / length;
            double vertical = height < Computer.CompareDelta ? 0 : (VerticalOffset + (ScrollHeight / 2.0)) / height;
            return (horizontal, vertical);
        }

        /// <summary>
        /// Refreshes the scrollviewer's horizontal offset.
        /// Should be called after a new <see cref="ViewViewModel.Length"/> or <see cref="ViewViewModel.Height"/>is set.
        /// </summary>
        /// <param name="horizontalCenterRatio">The horizontal center ratio before the scrollviewer's parameters have changed.</param>
        /// <param name="verticalCenterRatio">The vertical center ratio before the scrollviewer's parameters have changed.</param>
        protected virtual void RefreshScrollViewerOffset(double horizontalCenterRatio, double verticalCenterRatio)
        {
            double newHorizontalOffset = horizontalCenterRatio * Length - ScrollWidth / 2;
            if (newHorizontalOffset < 0)
                newHorizontalOffset = 0;
            if (newHorizontalOffset > Length - ScrollWidth / 2)
                newHorizontalOffset = Length - ScrollWidth / 2;
            double newVerticalOffset = verticalCenterRatio * Height - ScrollHeight / 2;
            if (newVerticalOffset < 0)
                newVerticalOffset = 0;
            if (newVerticalOffset > Height - ScrollHeight / 2)
                newVerticalOffset = Height - ScrollHeight / 2;
            InvokeScrollToOffset(newHorizontalOffset, newVerticalOffset);
        }

        /// <inheritdoc/>
        protected override void OnLengthChanged(double oldValue, double newValue)
        {
            (double horizontal, double vertical) ratio = GetCenterRatio(oldValue, Height);
            RefreshScrollViewerOffset(ratio.horizontal, ratio.vertical);
        }

        /// <inheritdoc/>
        protected override void OnHeightChanged(double oldValue, double newValue)
        {
            (double horizontal, double vertical) ratio = GetCenterRatio(Length, oldValue);
            RefreshScrollViewerOffset(ratio.horizontal, ratio.vertical);
        }

        #region Scroll

        /// <summary>
        /// Invokes the <see cref="ScrollToOffset"/> event.
        /// </summary>
        protected void InvokeScrollToOffset(double horizontalOffset, double verticalOffset) => ScrollToOffset?.Invoke(horizontalOffset, verticalOffset);

        /// <summary>
        /// Invokes the <see cref="Scrolled"/> event.
        /// </summary>
        protected void InvokeScrolled(double horizontalOffset, double verticalOffset) => Scrolled?.Invoke(horizontalOffset, verticalOffset);

        /// <summary>
        /// Occurs when the scrollviewer changes its position.
        /// </summary>
        /// <param name="sender">The scrollviewer.</param>
        /// <param name="e">The event args.</param>
        public virtual void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scroll = sender as ScrollViewer;
            VerticalOffset = scroll.VerticalOffset;
            HorizontalOffset = scroll.HorizontalOffset;
            RefreshOnScroll();
            Scrolled?.Invoke(HorizontalOffset, VerticalOffset);
        }

        /// <summary>
        /// Called when the scrollviewer changes its size.
        /// Manages to stay at the center of the scrollviewer when changing size.
        /// </summary>
        /// <param name="sender">The scroll viewer.</param>
        /// <param name="e">The event args.</param>
        public virtual void OnScrollViewerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var oldWidth = ScrollWidth;
            var oldHeight = ScrollHeight;
            (double horizontal, double vertical) ratio = GetCenterRatio();
            var scrollViewer = (sender as ScrollViewer);
            ScrollWidth = scrollViewer.ActualWidth;
            ScrollHeight = scrollViewer.ActualHeight;
            if (Math.Abs(oldHeight - ScrollHeight) > Computer.CompareDelta || Math.Abs(oldWidth - ScrollWidth) > Computer.CompareDelta)
            {
                RefreshScrollViewerOffset(ratio.horizontal, ratio.vertical);
                RefreshOnScrollViewerSizeChanged();
            }
        }

        /// <summary>
        /// Refreshes this view on scrolling.
        /// </summary>
        protected virtual void RefreshOnScroll() { }

        /// <summary>
        /// Refreshes this view when the scrollviewer's size has changed.
        /// </summary>
        protected virtual void RefreshOnScrollViewerSizeChanged() { }

        #endregion

        #region Zoom

        /// <summary>
        /// Zooms in on the view.
        /// </summary>
        public void ZoomIn() => PixelPerData *= ZoomRatio;

        /// <summary>
        /// Zooms out of the view.
        /// </summary>
        public void ZoomOut() => PixelPerData /= ZoomRatio;

        /// <summary>
        /// Zooms in on the view in an alternative way.
        /// </summary>
        public void AltZoomIn() => AltPixelPerData *= AltZoomRatio;

        /// <summary>
        /// Zooms out of the view in an alternative way.
        /// </summary>
        public void AltZoomOut() => AltPixelPerData /= AltZoomRatio;

        /// <summary>
        /// Refreshes this view on zooming.
        /// </summary>
        protected abstract void RefreshOnZoom();

        /// <summary>
        /// Refreshes this view on alternative zooming.
        /// </summary>
        protected virtual void RefreshOnAltZoom() { }

        /// <summary>
        /// Sets up the zooming parameters. Should be called right after initalization.
        /// </summary>
        /// <param name="pixelPerData">The new <see cref="PixelPerData"/> value.</param>
        public void SetUpZoomingParameters(double pixelPerData)
        {
            PixelPerData = pixelPerData;
        }

        /// <summary>
        /// Sets up the zooming parameters. Should be called right after initalization.
        /// </summary>
        /// <param name="pixelPerData">The new <see cref="PixelPerData"/> value.</param>
        /// <param name="ppdMax">The new <see cref="PPDMax"/> value.</param>
        /// <param name="ppdMin">The new <see cref="PPDMin"/> value.</param>
        /// <param name="zoomRatio">The new <see cref="ZoomRatio"/> value.</param>
        public void SetUpZoomingParameters(double pixelPerData, double ppdMax, double ppdMin, double zoomRatio)
        {
            PPDMax = ppdMax;
            PPDMin = ppdMin;
            ZoomRatio = zoomRatio;
            PixelPerData = pixelPerData;
        }

        /// <summary>
        /// Sets up the zooming parameters. Should be called right after initalization.
        /// </summary>
        /// <param name="altPixelPerData">The new <see cref="PixelPerData"/> value.</param>
        public void SetUpAltZoomingParameters(double altPixelPerData)
        {
            AltPixelPerData = altPixelPerData;
        }

        /// <summary>
        /// Sets up the alternative zooming parameters. Should be called right after initalization.
        /// </summary>
        /// <param name="altPixelPerData">The new <see cref="AltPixelPerData"/> value.</param>
        /// <param name="altppdMax">The new <see cref="AltPPDMax"/> value.</param>
        /// <param name="altppdMin">The new <see cref="AltPPDMin"/> value.</param>
        /// <param name="altZoomRatio">The new <see cref="AltZoomRatio"/> value.</param>
        public void SetUpAltZoomingParameters(double altPixelPerData, double altppdMax, double altppdMin, double altZoomRatio)
        {
            AltPPDMax = altppdMax;
            AltPPDMin = altppdMin;
            AltZoomRatio = altZoomRatio;
            AltPixelPerData = altPixelPerData;
        }

        #endregion

        #endregion
    }
}
