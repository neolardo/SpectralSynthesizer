using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SpectralSynthesizer.Models.Audio.Data
{
    /// <summary>
    /// Represents the stochastic noise model of any <see cref="Note"/>.
    /// </summary>
    public class Noise : BaseModel, IRenderable
    {
        #region Properties

        [JsonProperty]
        /// <summary>
        /// The <see cref="SpectralEnvelope"/> <see cref="RatioPoint{T}"/> list.
        /// </summary>
        public List<RatioPoint<SpectralEnvelope>> Envelopes { get; private set; } = new List<RatioPoint<SpectralEnvelope>>();

        [JsonProperty]
        /// <summary>
        /// The <see cref="Noise"/> model in it's <see cref="Data.Wave"/> form.
        /// </summary>
        public Wave Wave { get; private set; }

        #endregion

        #region Methods

        #region Creation

        /// <summary>
        /// Adds the list of <see cref="SpectralEnvelope"/>s to the <see cref="Envelopes"/> list.
        /// </summary>
        /// <param name="envelopes">The list of <see cref="SpectralEnvelope"/>s.</param>
        /// <param name="offset">The ratio offset between each envelope.</param>
        private void GenerateRatioPointsFromEnvelopes(List<SpectralEnvelope> envelopes, double offset)
        {
            double position = 0;
            foreach (var e in envelopes)
            {
                Envelopes.Add(new RatioPoint<SpectralEnvelope>(e, position));
                position += offset;
            }
        }

        #endregion

        #region Rendering

        /// <summary>
        /// Renders this <see cref="Noise"/>. This function can be cancelled.
        /// </summary>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="length">The length of the rendered audio in floats.</param>
        /// <param name="token">The cancellation token to cancel this function.</param>
        /// <returns>The float array containing the rendered audio data.</returns>
        public float[] Render(int sampleRate, int length, CancellationToken token)
        {
            float[] buffer = new float[length];
            if (Envelopes.Count < 2)
            {
                return buffer;
            }
            double offsetRatio = Envelopes[1].Position - Envelopes[0].Position;
            int hopScale = 1;
            int offset = (int)(offsetRatio * length);
            int samplingLength = (int)(offset / (1.0 - (Computer.HannWindowCache.EffectiveOffsetRatio * 2.0)) * hopScale);
            int currentOffset = 0;
            foreach (var p in Envelopes)
            {
                token.ThrowIfCancellationRequested();
                var complexArray = p.Value.ToComplexArray(samplingLength, sampleRate);
                var transformedArray = Computer.FourierTransform(complexArray, token, true);
                var floatArray = Array.ConvertAll(transformedArray, _ => (float)_.Real / transformedArray.Length);
                floatArray = Computer.HannWindowCache.Apply(floatArray.Take(samplingLength).ToArray(), WindowingType.Full);
                buffer.AddSample(floatArray, currentOffset, samplingLength);
                currentOffset += offset;
            }
            return buffer;
        }

        #endregion

        /// <summary>
        /// Gets the a list of <see cref="SpectralEnvelope"/>s from this <see cref="Noise"/> inside a given selection range in an inclusive way.
        /// </summary>
        /// <param name="selection">The selection ratio range.</param>
        /// <returns>A <see cref="RatioPoint{T}"/> <see cref="SpectralEnvelope"/> list.</returns>
        public List<RatioPoint<SpectralEnvelope>> GetSelectedEnvelopes((double start, double end) selection)
        {
            var envelopes = new List<RatioPoint<SpectralEnvelope>>();
            if (Envelopes.Count <= 3)
            {
                envelopes.AddRange(Envelopes);
            }
            else
            {
                if ((Envelopes[0].Position > selection.start && Envelopes[0].Position < selection.end) || (Envelopes[1].Position > selection.start && Envelopes[1].Position < selection.end))
                {
                    envelopes.Add(Envelopes[0]);
                }
                for (int i = 1; i < Envelopes.Count - 1; i++)
                {
                    if ((Envelopes[i].Position > selection.start && Envelopes[i].Position < selection.end) ||
                        (Envelopes[i + 1].Position > selection.start && Envelopes[i + 1].Position < selection.end) ||
                        (Envelopes[i - 1].Position > selection.start && Envelopes[i - 1].Position < selection.end))
                    {
                        envelopes.Add(Envelopes[i]);
                    }
                }
                if ((Envelopes[Envelopes.Count - 1].Position > selection.start && Envelopes[Envelopes.Count - 1].Position < selection.end) || (Envelopes[Envelopes.Count - 2].Position > selection.start && Envelopes[Envelopes.Count - 2].Position < selection.end))
                {
                    envelopes.Add(Envelopes[Envelopes.Count - 1]);
                }
            }
            return envelopes;
        }

        /// <inheritdoc/>
        public override BaseModel GetDeepCopy()
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new blank instance of the <see cref="Noise"/> class.
        /// </summary>
        [JsonConstructor]
        public Noise() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Noise"/> class by interpolating between two existing <see cref="Noise"/>s.
        /// This function can be cancelled.
        /// </summary>
        /// <param name="a">The first <see cref="Noise"/>.</param>
        /// <param name="b">The second <see cref="Noise"/>.</param>
        /// <param name="lerpValue">The lerp value between the two <see cref="Noise"/>s.</param>
        /// <param name="aNoteScale">The number of notes to change in the first <see cref="Noise"/> to match the pitch of the new <see cref="Note"/>.</param>
        /// <param name="bNoteScale">The number of notes to change in the second <see cref="Noise"/> to match the pitch of the new <see cref="Note"/>.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to cancel this function.</param>
        public Noise(Noise a, Noise b, double lerpValue, int aNoteScale, int bNoteScale, CancellationToken token)
        {
            var modifiedA = new Noise(a);
            var modifiedB = new Noise(a);
            if (aNoteScale > 0)
            {
                foreach (var e in modifiedA.Envelopes)
                {
                    var amplitudesCopy = (float[])e.Value.Amplitudes.Clone();
                    e.Value.Amplitudes = new float[amplitudesCopy.Length];
                    Array.Copy(amplitudesCopy, 0, e.Value.Amplitudes, aNoteScale, amplitudesCopy.Length - aNoteScale);
                }
            }
            else
            {
                foreach (var e in modifiedA.Envelopes)
                {
                    var amplitudesCopy = (float[])e.Value.Amplitudes.Clone();
                    e.Value.Amplitudes = new float[amplitudesCopy.Length];
                    Array.Copy(amplitudesCopy, -aNoteScale, e.Value.Amplitudes, 0, amplitudesCopy.Length + aNoteScale);
                }
            }
            if (bNoteScale > 0)
            {
                foreach (var e in modifiedB.Envelopes)
                {
                    var amplitudesCopy = (float[])e.Value.Amplitudes.Clone();
                    e.Value.Amplitudes = new float[amplitudesCopy.Length];
                    Array.Copy(amplitudesCopy, 0, e.Value.Amplitudes, bNoteScale, amplitudesCopy.Length - bNoteScale);
                }
            }
            else
            {
                foreach (var e in modifiedB.Envelopes)
                {
                    var amplitudesCopy = (float[])e.Value.Amplitudes.Clone();
                    e.Value.Amplitudes = new float[amplitudesCopy.Length];
                    Array.Copy(amplitudesCopy, -bNoteScale, e.Value.Amplitudes, 0, amplitudesCopy.Length + bNoteScale);
                }
            }
            token.ThrowIfCancellationRequested();
            var primaryNoise = modifiedA.Envelopes.Count > modifiedB.Envelopes.Count ? modifiedA : modifiedB;
            var secondaryNoise = primaryNoise == modifiedA ? modifiedB : modifiedA;
            lerpValue = primaryNoise == modifiedA ? 1 - lerpValue : lerpValue;
            int startEnvelopeIndex = 0;
            foreach (var primaryEnvelope in primaryNoise.Envelopes)
            {
                SpectralEnvelope newEnvelope = null;
                while (startEnvelopeIndex + 1 < secondaryNoise.Envelopes.Count && secondaryNoise.Envelopes[startEnvelopeIndex + 1].Position < primaryEnvelope.Position)
                {
                    startEnvelopeIndex++;
                }
                if (startEnvelopeIndex + 1 < secondaryNoise.Envelopes.Count)
                {
                    double secondaryLerpRange = secondaryNoise.Envelopes[startEnvelopeIndex + 1].Position - secondaryNoise.Envelopes[startEnvelopeIndex].Position;
                    double secondaryLerpValue = primaryEnvelope.Position - secondaryNoise.Envelopes[startEnvelopeIndex].Position;
                    newEnvelope = new SpectralEnvelope(secondaryNoise.Envelopes[startEnvelopeIndex].Value.Amplitudes
                        .Zip(secondaryNoise.Envelopes[startEnvelopeIndex + 1].Value.Amplitudes, (aAmp, bAmp) => Computer.Lerp(aAmp, bAmp, secondaryLerpValue))
                        .Zip(primaryEnvelope.Value.Amplitudes, (secondaryAmp, primaryAmp) => Computer.Lerp(secondaryAmp, primaryAmp, lerpValue)).ToArray());
                }
                else
                {
                    newEnvelope = new SpectralEnvelope(secondaryNoise.Envelopes[startEnvelopeIndex].Value.Amplitudes
                        .Zip(primaryEnvelope.Value.Amplitudes, (secondaryAmp, primaryAmp) => Computer.Lerp(secondaryAmp, primaryAmp, lerpValue)).ToArray());
                }
                Envelopes.Add(new RatioPoint<SpectralEnvelope>(newEnvelope, primaryEnvelope.Position));
                token.ThrowIfCancellationRequested();
            }
            Wave = new Wave(Render(a.Wave.WaveFormat.SampleRate * a.Wave.WaveFormat.Channels, Computer.Lerp(secondaryNoise.Wave.Data.Length, primaryNoise.Wave.Data.Length, lerpValue), token), a.Wave.WaveFormat);
        }

        /// <summary>
        /// Initilaizes a new instance of the <see cref="Noise"/> class by deep copying the given <see cref="Noise"/>.
        /// </summary>
        /// <param name="noise">The <see cref="Noise"/> to deep copy.</param>
        public Noise(Noise noise)
        {
            Wave = new Wave((float[])noise.Wave.Data.Clone(), noise.Wave.WaveFormat);
            foreach (var e in noise.Envelopes)
            {
                Envelopes.Add(new RatioPoint<SpectralEnvelope>(new SpectralEnvelope((float[])e.Value.Amplitudes.Clone()), e.Position));
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Noise"/> class from a given <see cref="Data.Wave"/>. This function can be cancelled.
        /// </summary>
        /// <param name="wave">The <see cref="Data.Wave"/>.</param>
        /// <param name="samplingFrequency">The sampling frequency.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to cancel this function.</param>
        public Noise(Wave wave, float samplingFrequency, CancellationToken token)
        {
            var envelopes = new List<SpectralEnvelope>();
            int sampleRate = wave.WaveFormat.SampleRate * wave.WaveFormat.Channels;
            int samplingLength = (int)(sampleRate / samplingFrequency);
            int hopScale = 1;
            int hopSize = (int)(samplingLength * (1.0 - (Computer.HannWindowCache.EffectiveOffsetRatio * 2.0)) / hopScale);
            double loadingPercent = 1.0 / (int)Math.Ceiling(wave.Data.Length / (double)hopSize);
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio *= loadingPercent;
            for (int i = 0; i < wave.Data.Length; i += hopSize)
            {
                token.ThrowIfCancellationRequested();
                float[] sample = new float[samplingLength];
                int length = Computer.ClampMax(samplingLength, wave.Data.Length - i);
                Array.Copy(wave.Data, i, sample, 0, length);
                envelopes.Add(new SpectralEnvelope(sample, sampleRate, token));
                Instrument.InstrumentGenerationPercentManager.LoadStep();
            }
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio /= loadingPercent;
            GenerateRatioPointsFromEnvelopes(envelopes, hopSize / (double)wave.Data.Length);
            Wave = new Wave(Render(ProjectModel.StandardSampleRate, (int)(wave.Data.Length * ((ProjectModel.StandardWaveFormat.SampleRate * ProjectModel.StandardWaveFormat.Channels) / (double)sampleRate)), token), ProjectModel.StandardWaveFormat);
        }

        #endregion
    }
}
