using System.Windows;
using System.Windows.Controls;

namespace SpectralSynthesizer
{
    /// <summary>
    /// Interaction logic for NoteView.xaml.
    /// </summary>
    public partial class NoteView : UserControl
    {
        public NoteView()
        {
            InitializeComponent();
        }

        public bool IsContentLoaded
        {
            get { return (bool)GetValue(IsContentLoadedProperty); }
            set { SetValue(IsContentLoadedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsContentLoaded.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsContentLoadedProperty =
            DependencyProperty.Register(nameof(IsContentLoaded), typeof(bool), typeof(NoteView), new PropertyMetadata(null));


        #region Views

        public RulerViewViewModel SinusoidRulerViewDataContext
        {
            get { return (RulerViewViewModel)GetValue(SinusoidRulerViewDataContextProperty); }
            set { SetValue(SinusoidRulerViewDataContextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SinusoidRulerViewDataContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SinusoidRulerViewDataContextProperty =
            DependencyProperty.Register(nameof(SinusoidRulerViewDataContext), typeof(RulerViewViewModel), typeof(NoteView), new PropertyMetadata(null));

        public SpectogramViewViewModel SinusoidViewDataContext
        {
            get { return (SpectogramViewViewModel)GetValue(SinusoidViewDataContextProperty); }
            set { SetValue(SinusoidViewDataContextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SinusoidViewDataContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SinusoidViewDataContextProperty =
            DependencyProperty.Register(nameof(SinusoidViewDataContext), typeof(SpectogramViewViewModel), typeof(NoteView), new PropertyMetadata(null));

        public RulerItemViewModel TransientRulerViewDataContext
        {
            get { return (RulerItemViewModel)GetValue(TransientRulerViewDataContextProperty); }
            set { SetValue(TransientRulerViewDataContextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TransientRulerViewDataContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TransientRulerViewDataContextProperty =
            DependencyProperty.Register(nameof(TransientRulerViewDataContext), typeof(RulerViewViewModel), typeof(NoteView), new PropertyMetadata(null));

        public WaveViewViewModel TransientViewDataContext
        {
            get { return (WaveViewViewModel)GetValue(TransientViewDataContextProperty); }
            set { SetValue(TransientViewDataContextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TransientViewDataContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TransientViewDataContextProperty =
            DependencyProperty.Register(nameof(TransientViewDataContext), typeof(WaveViewViewModel), typeof(NoteView), new PropertyMetadata(null));

        public RulerViewViewModel NoiseRulerViewDataContext
        {
            get { return (RulerViewViewModel)GetValue(NoiseRulerViewDataContextProperty); }
            set { SetValue(NoiseRulerViewDataContextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NoiseRulerViewDataContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoiseRulerViewDataContextProperty =
            DependencyProperty.Register(nameof(NoiseRulerViewDataContext), typeof(RulerViewViewModel), typeof(NoteView), new PropertyMetadata(null));

        public WaveViewViewModel NoiseViewDataContext
        {
            get { return (WaveViewViewModel)GetValue(NoiseViewDataContextProperty); }
            set { SetValue(NoiseViewDataContextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NoiseViewDataContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoiseViewDataContextProperty =
            DependencyProperty.Register(nameof(NoiseViewDataContext), typeof(WaveViewViewModel), typeof(NoteView), new PropertyMetadata(null));


        #endregion

        #region Audio Buffers

        public AudioBufferBorderViewModel NoteAudioBufferDataContext
        {
            get { return (AudioBufferBorderViewModel)GetValue(NoteAudioBufferDataContextProperty); }
            set { SetValue(NoteAudioBufferDataContextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NoteAudioBufferDataContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoteAudioBufferDataContextProperty =
            DependencyProperty.Register(nameof(NoteAudioBufferDataContext), typeof(AudioBufferBorderViewModel), typeof(NoteView), new PropertyMetadata(null));


        public AudioBufferBorderViewModel SinusoidAudioBufferDataContext
        {
            get { return (AudioBufferBorderViewModel)GetValue(SinusoidAudioBufferDataContextProperty); }
            set { SetValue(SinusoidAudioBufferDataContextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SinusoidAudioBufferDataContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SinusoidAudioBufferDataContextProperty =
            DependencyProperty.Register(nameof(SinusoidAudioBufferDataContext), typeof(AudioBufferBorderViewModel), typeof(NoteView), new PropertyMetadata(null));

        public AudioBufferBorderViewModel TransientAudioBufferDataContext
        {
            get { return (AudioBufferBorderViewModel)GetValue(TransientAudioBufferDataContextProperty); }
            set { SetValue(TransientAudioBufferDataContextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TransientAudioBufferDataContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TransientAudioBufferDataContextProperty =
            DependencyProperty.Register(nameof(TransientAudioBufferDataContext), typeof(AudioBufferBorderViewModel), typeof(NoteView), new PropertyMetadata(null));

        public AudioBufferBorderViewModel NoiseAudioBufferDataContext
        {
            get { return (AudioBufferBorderViewModel)GetValue(NoiseAudioBufferDataContextProperty); }
            set { SetValue(NoiseAudioBufferDataContextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NoiseAudioBufferDataContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoiseAudioBufferDataContextProperty =
            DependencyProperty.Register(nameof(NoiseAudioBufferDataContext), typeof(AudioBufferBorderViewModel), typeof(NoteView), new PropertyMetadata(null));

        #endregion
    }
}
