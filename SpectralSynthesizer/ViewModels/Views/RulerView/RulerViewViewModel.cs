using SpectralSynthesizer.Models;
using System;
using System.Collections.ObjectModel;

namespace SpectralSynthesizer
{
    /// <summary>
    /// The viewmodel of the <see cref="RulerView"/>
    /// </summary>
    public class RulerViewViewModel : OptimizedTimeDomainViewModel
    {
        #region Properties

        #region Model

        private TimeDomainModel _model;

        /// <summary>
        /// The time domain model
        /// </summary>
        public TimeDomainModel Model
        {
            get { return _model; }
            set
            {
                if (_model != null)
                    _model.PropertyChanged -= OnPropertyChanged;
                _model = value;
                if (_model != null)
                    _model.PropertyChanged += OnPropertyChanged;
            }
        }

        #endregion

        /// <summary>
        /// The list of ruler items
        /// </summary>
        public ObservableCollection<RulerItemViewModel> Items { get; set; } = new ObservableCollection<RulerItemViewModel>();

        /// <summary>
        /// The height of one ruler
        /// </summary>
        public double Height => 15;

        #endregion

        #region Methods

        /// <summary>
        /// Loads the data to the ruler
        /// </summary>
        public void LoadData(TimeDomainModel model)
        {
            Model = model;
            if (Model != null)
            {
                Length = Model.Length / 1000f * PixelPerSecond;
                CreateRuler();
                IsContentLoaded = true;
            }
            else
            {
                IsContentLoaded = false;
            }
        }

        /// <summary>
        /// Method to create the ruler
        /// </summary>
        public void CreateRuler()
        {
            if (Model == null)
                return;

            Items.Clear();
            int tiny = 0;
            int small = 1;
            int big = 8;
            int unitpersec = 64;
            double unit = PixelPerSecond / unitpersec;
            if (PixelPerSecond > 2400)
            {
                tiny = 1;
                small = 8;
            }
            else if (PixelPerSecond > 1700)
            {
                tiny = 2;
                small = 8;
            }
            else if (PixelPerSecond > 1300)
            {
                tiny = 2;
                small = 16;
            }
            else if (PixelPerSecond > 800)
            {
                tiny = 4;
                small = 16;
            }
            else if (PixelPerSecond > 550)
            {
                tiny = 4;
                small = 32;
            }
            else if (PixelPerSecond > 420)
            {
                tiny = 8;
                small = 32;
            }
            else if (PixelPerSecond > 300)
            {
                small = 8;
                big = 64;
            }
            else if (PixelPerSecond > 200)
            {
                small = 16;
                big = 64;
            }
            else if (PixelPerSecond > 130)
            {
                small = 16;
                big = 128;
            }
            else if (PixelPerSecond > 80)
            {
                small = 32;
                big = 128;
            }
            else if (PixelPerSecond > 50)
            {
                small = 32;
                big = 256;
            }
            else if (PixelPerSecond > 18)
            {
                small = 64;
                big = 256;
            }
            else
            {
                small = 64;
                big = 512;
            }
            int start = (int)(ContentMargin.Left / unit);
            if (tiny == 0)
            {
                start = start / small;
                if (Math.Abs((ContentMargin.Left / unit) / small - start) < 0.01)
                    start = start * small;
                else
                    start = (start + 1) * small;
                for (int i = start; i * unit < ContentMargin.Left + ContentWidth; i++)
                {
                    if (i % big == 0)
                    {
                        int sec = i / unitpersec;
                        int min = sec / 60;
                        sec -= min * 60;
                        string secstr = sec < 10 ? "0" + sec.ToString() : sec.ToString();
                        Items.Add(new RulerItemViewModel(i * unit - ContentMargin.Left, min + ":" + secstr, 10.0));
                    }
                    else if (i % small == 0)
                        Items.Add(new RulerItemViewModel(i * unit - ContentMargin.Left, "", 5.0));
                }
            }
            else
            {
                start = start / tiny;
                if (Math.Abs((ContentMargin.Left / unit) / tiny - start) < 0.01)
                    start = start * tiny;
                else
                    start = (start + 1) * tiny;
                for (int i = start; i * unit < ContentMargin.Left + ContentWidth; i++)
                {
                    if (i % small == 0)
                    {
                        int hundredmillisec = (int)(((i % unitpersec) / (double)unitpersec) * 100);
                        int sec = i / unitpersec;
                        int min = sec / 60;
                        sec -= min * 60;
                        string secstr = sec < 10 ? "0" + sec.ToString() : sec.ToString();
                        string hundredmillisecstr = hundredmillisec < 10 ? "0" + hundredmillisec.ToString() : hundredmillisec.ToString();
                        Items.Add(new RulerItemViewModel(i * unit - ContentMargin.Left, min + ":" + secstr + ":" + hundredmillisecstr, 10.0));
                    }
                    else if (i % tiny == 0)
                        Items.Add(new RulerItemViewModel(i * unit - ContentMargin.Left, "", 5.0));
                }
            }

        }

        /// <inheritdoc/>
        protected override void RefreshOnScroll() => CreateRuler();

        /// <inheritdoc/>
        protected override void RefreshOnScrollViewerSizeChanged() => CreateRuler();

        /// <inheritdoc/>
        protected override void RefreshOnZoom()
        {
            if (Model == null)
                return;
            Length = Model.Length / 1000f * PixelPerSecond;
            CreateRuler();
        }

        #endregion

        #region Contructor

        /// <summary>
        /// Creates a new instance of a <see cref="RulerView"/>'s viewmodel
        /// </summary>
        public RulerViewViewModel()
        {
        }

        #endregion

    }
}
