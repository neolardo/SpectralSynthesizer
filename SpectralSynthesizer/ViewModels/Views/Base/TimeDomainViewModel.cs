using System;
using System.Collections.Generic;
using System.Windows;

namespace SpectralSynthesizer
{
    /// <summary>
    /// The abstract viewmodel class for views whom data is in the time domain.
    /// Time domain views can follow each other and a part of their data can be selected.
    /// </summary>
    public abstract class TimeDomainViewModel : ScrollableZoomableViewViewModel
    {
        #region Delegates and Events

        /// <summary>
        /// Delegate for fireing events when the selection has changed.
        /// </summary>
        /// <param name="start">The new start of the selection.</param>
        /// <param name="end">The new end of the selection.</param>
        public delegate void SelectionChangedDelegate(double start, double end);

        /// <summary>
        /// Fires off when the <see cref="Selection"/> has changed.
        /// </summary>
        public SelectionChangedDelegate SelectionChanged;

        #endregion

        #region Properties

        /// <summary>
        /// The number of pixels that represents one second.
        /// </summary>
        public double PixelPerSecond
        {
            get { return PixelPerData; }
            set { PixelPerData = value; }
        }

        private (double Start, double End) _selection;

        /// <summary>
        /// The selected part of the data relative to the full length of the data. Ranges from 0.0 to 1.0.
        /// </summary>
        public (double Start, double End) Selection
        {
            get { return _selection; }
            set
            {
                value = (Computer.ClampMin(value.Start, 0), Computer.ClampMax(value.End, 1));
                if (Math.Abs(value.Start - _selection.Start) > Computer.CompareDelta || Math.Abs(value.End - _selection.End) > Computer.CompareDelta)
                {
                    if (IsSelectionValid(value.Start, value.End))
                    {
                        _selection = value;
                        OnSelectionChanged();
                    }
                }
            }
        }

        /// <summary>
        /// The width of the selection box.
        /// </summary>
        public double SelectionBoxWidth { get; set; }

        /// <summary>
        /// The opacity of the selection box.
        /// </summary>
        public double SelectionBoxOpacity { get; set; } = 0.5;

        /// <summary>
        /// The margin of the selection box.
        /// </summary>
        public Thickness SelectionBoxMargin { get; set; } = new Thickness(0, 0, 0, 0);

        /// <summary>
        /// The minimum number of selectable wave float data.
        /// </summary>
        private static double SelectionBoxMinimumWidth => 5;

        #endregion

        #region Methods

        #region Selection

        /// <summary>
        /// Selects the whole data range.
        /// </summary>
        protected void SelectAll() => Selection = (0, 1);

        /// <summary>
        /// Sets the start of the selection from the given horizontal point of the content.
        /// </summary>
        /// <param name="x">The horizontal point of the new start of the selection.</param>
        protected void SelectFrom(double x) => Selection = (x / Length, Selection.End);

        /// <summary>
        /// Sets the end of the selection to the given horizontal point of the content.
        /// </summary>
        /// <param name="x">The horizontal point of the new end of the selection.</param>
        protected void SelectTo(double x) => Selection = (Selection.Start, x / Length);

        /// <summary>
        /// Returns true if the given selection values are valid.
        /// </summary>
        /// <param name="start">The new start of the selection.</param>
        /// <param name="end">The new end of the selection.</param>
        /// <returns>True if the selection values are valid, otherwise false.</returns>
        private bool IsSelectionValid(double start, double end) => !(start > end || start < 0 || end > 1 || (Length >= SelectionBoxMinimumWidth && (end - start) * Length < SelectionBoxMinimumWidth));

        /// <summary>
        /// Called after the <see cref="Selection"/> has changed its value.
        /// </summary>
        private void OnSelectionChanged()
        {
            RefreshSelectionBox();
            RefreshOnSelectionChanged();
            SelectionChanged?.Invoke(Selection.Start, Selection.End);
        }

        /// <inheritdoc/>
        protected override void OnLengthChanged(double oldValue, double newValue)
        {
            base.OnLengthChanged(oldValue, newValue);
            RefreshSelectionBox();
            RefreshOnSelectionChanged();
        }

        /// <summary>
        /// Refreshes the selection box's properties.
        /// </summary>
        private void RefreshSelectionBox()
        {
            SelectionBoxMargin = new Thickness(Selection.Start * Length, 0, 0, 0);
            SelectionBoxWidth = (Selection.End - Selection.Start) * Length;
        }

        /// <summary>
        /// Called after the <see cref="Selection"/> has changed.
        /// </summary>
        protected virtual void RefreshOnSelectionChanged() { }

        #endregion

        #region Following

        #region Static

        /// <summary>
        /// Makes the given <see cref="TimeDomainViewModel"/>s follow each other.
        /// </summary>
        /// <param name="a">The first <see cref="TimeDomainViewModel"/>.</param>
        /// <param name="b">The second <see cref="TimeDomainViewModel"/>.</param>
        /// <param name="followScroll">True if this views should follow each other's scrolling, otherwise false.</param>
        /// <param name="followZoom">True if this views should follow each other's zooming, otherwise false.</param>
        /// <param name="followAltZoom">True if this views should follow each other's alt zooming, otherwise false.</param>
        /// <param name="followSelectionChange">True if this views should follow each other's selection changing, otherwise false.</param>
        public static void SetUpFollowers(TimeDomainViewModel a, TimeDomainViewModel b, bool followScroll = true, bool followZoom = true, bool followAltZoom = true, bool followSelectionChange = true)
        {
            a.Follow(b, followScroll, followZoom, followAltZoom, followSelectionChange);
            b.Follow(a, followScroll, followZoom, followAltZoom, followSelectionChange);
        }

        /// <summary>
        /// Makes the given <see cref="TimeDomainViewModel"/>s follow each other.
        /// </summary>
        /// <param name="a">The first <see cref="TimeDomainViewModel"/>.</param>
        /// <param name="b">The second <see cref="TimeDomainViewModel"/>.</param>
        /// <param name="c">The third <see cref="TimeDomainViewModel"/>.</param>
        /// <param name="followScroll">True if this views should follow each other's scrolling, otherwise false.</param>
        /// <param name="followZoom">True if this views should follow each other's zooming, otherwise false.</param>
        /// <param name="followAltZoom">True if this views should follow each other's alt zooming, otherwise false.</param>
        /// <param name="followSelectionChange">True if this views should follow each other's selection changing, otherwise false.</param>
        public static void SetUpFollowers(TimeDomainViewModel a, TimeDomainViewModel b, TimeDomainViewModel c, bool followScroll = true, bool followZoom = true, bool followAltZoom = true, bool followSelectionChange = true)
        {
            List<TimeDomainViewModel> tdvms = new List<TimeDomainViewModel>();
            tdvms.Add(a);
            tdvms.Add(b);
            tdvms.Add(c);
            FollowAll(tdvms, followScroll, followZoom, followAltZoom, followSelectionChange);
        }

        /// <summary>
        /// Makes the given <see cref="TimeDomainViewModel"/>s follow each other.
        /// </summary>
        /// <param name="a">The first <see cref="TimeDomainViewModel"/>.</param>
        /// <param name="b">The second <see cref="TimeDomainViewModel"/>.</param>
        /// <param name="c">The third <see cref="TimeDomainViewModel"/>.</param>
        /// <param name="d">The forth <see cref="TimeDomainViewModel"/>.</param>
        /// <param name="followScroll">True if this views should follow each other's scrolling, otherwise false.</param>
        /// <param name="followZoom">True if this views should follow each other's zooming, otherwise false.</param>
        /// <param name="followAltZoom">True if this views should follow each other's alt zooming, otherwise false.</param>
        /// <param name="followSelectionChange">True if this views should follow each other's selection changing, otherwise false.</param>
        public static void SetUpFollowers(TimeDomainViewModel a, TimeDomainViewModel b, TimeDomainViewModel c, TimeDomainViewModel d, bool followScroll = true, bool followZoom = true, bool followAltZoom = true, bool followSelectionChange = true)
        {
            List<TimeDomainViewModel> tdvms = new List<TimeDomainViewModel>();
            tdvms.Add(a);
            tdvms.Add(b);
            tdvms.Add(c);
            tdvms.Add(d);
            FollowAll(tdvms, followScroll, followZoom, followAltZoom, followSelectionChange);
        }

        /// <summary>
        /// Makes the given <see cref="TimeDomainViewModel"/>s follow each other.
        /// </summary>
        /// <param name="tdvms">The collection of <see cref="TimeDomainViewModel"/>s.</param>
        /// <param name="followScroll">True if this views should follow each other's scrolling, otherwise false.</param>
        /// <param name="followZoom">True if this views should follow each other's zooming, otherwise false.</param>
        /// <param name="followAltZoom">True if this views should follow each other's alt zooming, otherwise false.</param>
        /// <param name="followSelectionChange">True if this views should follow each other's selection changing, otherwise false.</param>
        public static void FollowAll(IEnumerable<TimeDomainViewModel> tdvms, bool followScroll = true, bool followZoom = true, bool followAltZoom = true, bool followSelectionChange = true)
        {
            foreach (var follower in tdvms)
            {
                foreach (var followable in tdvms)
                {
                    if (follower != followable)
                    {
                        follower.Follow(followable, followScroll, followZoom, followAltZoom, followSelectionChange);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Method to follow another time domain view's position and scale.
        /// Should only be called once.
        /// </summary>
        /// <param name="view">The other <see cref="TimeDomainViewModel"/>.</param>
        /// <param name="followScroll">True if this view should follow the other view's scrolling, otherwise false.</param>
        /// <param name="followZoom">True if this view should follow the other view's zooming, otherwise false.</param>
        /// <param name="followAltZoom">True if this view should follow the other view's alt zooming, otherwise false.</param>
        /// <param name="followSelectionChange">True if this view should follow the other view's selection changing, otherwise false.</param>
        public void Follow(TimeDomainViewModel view, bool followScroll = true, bool followZoom = true, bool followAltZoom = true, bool followSelectionChange = true)
        {
            if (followScroll)
            {
                OnFollowScrolled(view.HorizontalOffset);
                view.Scrolled += (horizontal, vertical) => OnFollowScrolled(horizontal);
            }
            if (followZoom)
            {
                OnFollowZoomed(view.PixelPerSecond);
                view.Zoomed += OnFollowZoomed;
            }
            if (followAltZoom)
            {
                OnFollowAltZoomed(view.AltPixelPerData);
                view.AltZoomed += OnFollowAltZoomed;
            }
            if (followSelectionChange)
            {
                OnFollowSelectionChanged(view.Selection.Start, view.Selection.End);
                view.SelectionChanged += OnFollowSelectionChanged;
            }
            OnFollow();
        }

        /// <summary>
        /// Called on following another view's <see cref="ScrollableZoomableViewViewModel.Zoomed"/> event.
        /// </summary>
        /// <param name="pixelPerSecond">The new value of the <see cref="PixelPerSecond"/> property.</param>
        private void OnFollowZoomed(double pixelPerSecond) => PixelPerSecond = pixelPerSecond;

        /// <summary>
        /// Called on following another view's <see cref="ScrollableZoomableViewViewModel.AltZoomed"/> event.
        /// </summary>
        /// <param name="altPixelPerData">The new value of the <see cref="ScrollableZoomableViewViewModel.AltPixelPerData"/> property.</param>
        private void OnFollowAltZoomed(double altPixelPerData) => AltPixelPerData = altPixelPerData;

        /// <summary>
        /// Called on following another view's <see cref="ScrollableZoomableViewViewModel.Scrolled"/> event.
        /// </summary>
        /// <param name="horizontalOffset">The new value of the horizontal scrolling offset.</param>
        private void OnFollowScrolled(double horizontalOffset)
        {
            var old = HorizontalOffset;
            HorizontalOffset = horizontalOffset;
            if (Math.Abs(old - HorizontalOffset) > Computer.CompareDelta)
            {
                // ScrollToOffset automatically calls the RefreshOnScroll() method
                InvokeScrollToOffset(HorizontalOffset, VerticalOffset);
            }
        }

        /// <summary>
        /// Called on following another view's <see cref="SelectionChanged"/> event.
        /// </summary>
        /// <param name="start">The new value of the <see cref="Selection"/>'s start.</param>
        /// <param name="end">The new value of the <see cref="Selection"/>'s end.</param>
        private void OnFollowSelectionChanged(double start, double end) => Selection = (start, end);

        /// <summary>
        /// Called after a follower is set.
        /// </summary>
        protected virtual void OnFollow() { }

        #endregion

        #endregion

    }
}
