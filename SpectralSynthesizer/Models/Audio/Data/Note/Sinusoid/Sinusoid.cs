using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace SpectralSynthesizer.Models.Audio.Data
{
    /// <summary>
    /// Represents the sinousoid part of any <see cref="Note"/>.
    /// </summary>
    public class Sinusoid : BaseModel, IRenderable
    {
        #region Properties

        [JsonProperty]
        /// <summary>
        /// The <see cref="SpectralTrajectory"/> list.
        /// </summary>
        public List<SpectralTrajectory> Trajectories { get; private set; } = new List<SpectralTrajectory>();

        [JsonProperty]
        /// <summary>
        /// The number of spectrums of the sinusoid was created from.
        /// </summary>
        private int SpectrumCount { get; }

        #endregion

        #region Methods

        #region Creation

        #region Spectral Trajectory Generation

        /// <summary>
        /// Generates the <see cref="Trajectories"/> list from a given <see cref="Spectrum"/> list.
        /// </summary>
        /// <param name="spectrums">The <see cref="Spectrum"/> list.</param>
        /// <param name="deltaTime">The delta time between <see cref="Spectrum"/>s in milliseconds.</param>
        /// <param name="continuationRange">The maximum range in log 12 base logarithmic values where two <see cref="SpectralUnit"/>s are connected.</param>
        /// <param name="maxSleepingTime">The maximum time a trajectory can sleep in milliseconds.</param>
        /// <param name="maxSleepingTime">The minimum length of any trajectory in milliseconds.</param>
        private void GenerateSpectralTrajectories(List<Spectrum> spectrums, double deltaTime, float continuationRange, int maxSleepingTime, int minimumLength, CancellationToken token)
        {
            if (spectrums == null || spectrums.Count == 0)
                return;
            // create the first trajcetories
            foreach (var spectralUnit in spectrums[spectrums.Count - 1].Spectrals)
                Trajectories.Add(new SpectralTrajectory(new RatioPoint<SpectralUnit>(spectralUnit, 1)));

            // convert to index count
            maxSleepingTime = (int)Math.Max(maxSleepingTime / deltaTime, 1);
            minimumLength = (int)Math.Max(minimumLength / deltaTime, 1);
            var currentTrajectories = new List<SpectralTrajectory>();
            currentTrajectories.AddRange(Trajectories);
            var deletableTrajectories = new List<SpectralTrajectory>();
            var addableTrajectories = new List<SpectralTrajectory>();
            var lastPoints = new List<RatioPoint<SpectralUnit>>();
            double loadingPercent = 1.0 / (double)(spectrums.Count - 1);
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio *= loadingPercent;
            // search for the remaining trajectories
            for (int specInd = spectrums.Count - 2; specInd >= 0; specInd--)
            {
                token.ThrowIfCancellationRequested();
                lastPoints.Clear();
                addableTrajectories.Clear();
                deletableTrajectories.Clear();
                double position = (double)specInd / (spectrums.Count - 1);
                foreach (var traj in currentTrajectories)
                {
                    // the last point is the 0. indexed one, because the algorithm is going backwards
                    lastPoints.Add(traj.SpectralPoints[0]);
                }
                var distanceMatrix = new FloatDistanceMatrix(lastPoints.Select(p => Computer.FrequencyToLogarithmicFrequency(p.Value.Frequency)).ToList(), spectrums[specInd].Spectrals.Select(s => Computer.FrequencyToLogarithmicFrequency(s.Frequency)).ToList(), continuationRange);
                var pairs = distanceMatrix.CalculateClosestPairs(); // rowInd = trajInd, columnInd = spectralInd
                foreach (var pair in pairs)
                {
                    // sleeping trajectories
                    if (pair.columnIndex == -1)
                    {
                        int pointInd = 0;
                        while (pointInd < currentTrajectories[pair.rowIndex].SpectralPoints.Count && currentTrajectories[pair.rowIndex].SpectralPoints[pointInd].Value.Amplitude < Computer.CompareDelta)
                        {
                            pointInd++;
                        }
                        if (pointInd >= maxSleepingTime)
                        {
                            currentTrajectories[pair.rowIndex].StartHopIndex = specInd;
                            deletableTrajectories.Add(currentTrajectories[pair.rowIndex]);
                        }
                        else
                        {
                            var silenceSpectral = new SpectralUnit(0, currentTrajectories[pair.rowIndex].SpectralPoints[0].Value.Frequency);
                            currentTrajectories[pair.rowIndex].AddPoint(new RatioPoint<SpectralUnit>(silenceSpectral, position));
                        }
                    }
                    // new trajectories  
                    else if (pair.rowIndex == -1)
                    {
                        bool isPointTooClose = false;
                        int pointInd = 0;
                        while (isPointTooClose == false && pointInd < lastPoints.Count)
                        {
                            if (distanceMatrix.Matrix[pointInd, pair.columnIndex] <= continuationRange)
                            {
                                isPointTooClose = true;
                            }
                            pointInd++;
                        }
                        if (isPointTooClose == false)
                        {
                            double lastPosition = (double)(specInd + 1) / (spectrums.Count - 1);
                            if (lastPosition > spectrums.Count - 1)
                            {
                                lastPosition = spectrums.Count - 1;
                            }
                            var lastPoint = new RatioPoint<SpectralUnit>(new SpectralUnit(0, spectrums[specInd].Spectrals[pair.columnIndex].Frequency), lastPosition);
                            var currentPoint = new RatioPoint<SpectralUnit>(spectrums[specInd].Spectrals[pair.columnIndex], position);
                            var newTrajectory = new SpectralTrajectory(lastPoint);
                            newTrajectory.AddPoint(currentPoint);
                            addableTrajectories.Add(newTrajectory);
                        }
                    }
                    // continuous trajectories
                    else
                    {
                        currentTrajectories[pair.rowIndex].AddPoint(new RatioPoint<SpectralUnit>(spectrums[specInd].Spectrals[pair.columnIndex], position));
                    }
                }
                // delete long-sleeping trajectories
                foreach (var traj in deletableTrajectories)
                {
                    currentTrajectories.Remove(traj);
                }
                // add new trajectories
                foreach (var traj in addableTrajectories)
                {
                    currentTrajectories.Add(traj);
                    Trajectories.Add(traj);
                }
                Instrument.InstrumentGenerationPercentManager.LoadStep();
            }
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio /= loadingPercent;
            foreach (var t in currentTrajectories)
            {
                t.StartHopIndex = 0;
            }
            OptimizeTrajectories(minimumLength);
        }

        /// <summary>
        /// Deletes any unnecessary silent <see cref="RatioPoint{T}"/>s or any short <see cref="SpectralTrajectory"/> from a given <see cref="Trajectories"/> list.
        /// </summary>
        /// <param name="minimumSpectralPointCount">The minimum required number of points of any trajectory.</param>
        private void OptimizeTrajectories(int minimumSpectralPointCount)
        {
            float minimumPCMAmplitude = Computer.DecibelToPCMAmplitude(ProjectModel.GlobalMinimumDecibel);
            var deletableTrajectories = new List<SpectralTrajectory>();
            foreach (var traj in Trajectories)
            {
                while (2 < traj.SpectralPoints.Count && traj.SpectralPoints[0].Value.Amplitude < minimumPCMAmplitude && traj.SpectralPoints[1].Value.Amplitude < minimumPCMAmplitude)
                {
                    traj.SpectralPoints.RemoveAt(0);
                    traj.StartHopIndex++;
                }
                while (2 < traj.SpectralPoints.Count && traj.SpectralPoints[traj.SpectralPoints.Count - 1].Value.Amplitude < minimumPCMAmplitude && traj.SpectralPoints[traj.SpectralPoints.Count - 2].Value.Amplitude < minimumPCMAmplitude)
                {
                    traj.SpectralPoints.RemoveAt(traj.SpectralPoints.Count - 1);
                }
                if (traj.SpectralPoints.Count < minimumSpectralPointCount)
                {
                    deletableTrajectories.Add(traj);
                }
            }
            foreach (var traj in deletableTrajectories)
            {
                Trajectories.Remove(traj);
            }
        }

        #endregion

        #region Spectrum List Creation

        /// <summary>
        /// Gets a list of <see cref="Spectrum"/>s from the given <see cref="Wave"/>. This function can be cancelled.
        /// </summary>
        /// <param name="wave">The <see cref="Wave"/>.</param>
        /// <param name="samplingLength">The sampling length.</param>
        /// <param name="hopSize">The hop size.</param>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="minimumDecibel">The minimum deibel amplitude of any <see cref="SpectralUnit"/>.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to cancel this function.</param>
        /// <returns>The list of <see cref="Spectrum"/>s.</returns>
        private static List<Spectrum> GetSpectrumsFromWave(Wave wave, int samplingLength, int hopSize, int sampleRate, float minimumDecibel, CancellationToken token)
        {
            double loadingPercent = 1.0 / (int)Math.Ceiling(wave.Data.Length / (double)hopSize);
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio *= loadingPercent;
            var spectrums = new List<Spectrum>();
            for (int i = 0; i < wave.Data.Length; i += hopSize)
            {
                token.ThrowIfCancellationRequested();
                float[] sample = wave.Data.GetSample(i, samplingLength);
                spectrums.Add(new Spectrum(sample, sampleRate, minimumDecibel, WindowingType.Full, token));
                Instrument.InstrumentGenerationPercentManager.LoadStep();
            }
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio /= loadingPercent;
            return spectrums;
        }

        /// <summary>
        /// Gets a list of <see cref="Spectrum"/>s from the given <see cref="Wave"/> with fixed frequency <see cref="SpectralUnit"/>. This function can be cancelled.
        /// </summary>
        /// <param name="wave">The <see cref="Wave"/>.</param>
        /// <param name="samplingLength">The sampling length.</param>
        /// <param name="hopSize">The hop size.</param>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="minimumDecibel">The minimum deibel amplitude of any <see cref="SpectralUnit"/>.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to cancel this function.</param>
        /// <returns>The list of <see cref="Spectrum"/>s.</returns>
        private static List<Spectrum> GetFixedFrequencySpectrumsFromWave(Wave wave, int samplingLength, int hopSize, int sampleRate, float minimumDecibel, CancellationToken token)
        {
            double loadingPercent = 1.0 / (int)Math.Ceiling(wave.Data.Length / (double)hopSize);
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio *= loadingPercent;
            var spectrums = new List<Spectrum>();
            float[] frequencies = new Spectrum(wave.Data, sampleRate, WindowingType.HalfDescending, token).Spectrals.Select(_ => _.Frequency).ToArray();
            for (int i = 0; i < wave.Data.Length; i += hopSize)
            {
                token.ThrowIfCancellationRequested();
                float[] sample = wave.Data.GetSample(i, samplingLength);
                spectrums.Add(new Spectrum(sample, frequencies, sampleRate, minimumDecibel, WindowingType.Full, token));
                Instrument.InstrumentGenerationPercentManager.LoadStep();
            }
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio /= loadingPercent;
            return spectrums;
        }

        #endregion

        #endregion

        #region Rendering

        /// <summary>
        /// Renders this <see cref="Sinusoid"/>. This function can be cancelled.
        /// </summary>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="length">The length of the rendered audio in floats.</param>
        /// <param name="token">The cancellation token to cancel this function.</param>
        /// <returns>The float array containing the rendered audio.</returns>
        public float[] Render(int sampleRate, int length, CancellationToken token)
        {
            float[] buffer = new float[length];
            foreach (var trajectory in Trajectories)
            {
                var trajWave = trajectory.Render(sampleRate, length, token);
                buffer = buffer.Zip(trajWave, (a, b) => a + b).ToArray();
            }
            return buffer;
        }

        /// <summary>
        /// Reconstructs this <see cref="Sinusoid"/>. This function can be cancelled.
        /// </summary>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="hopSize">The hop size.</param>
        /// <param name="length">The length of the rendered audio in floats.</param>
        /// <param name="token">The cancellation token to cancel this function.</param>
        /// <returns>The float array containing the rendered audio.</returns>
        private float[] Reconstruct(int sampleRate, int hopSize, int length, CancellationToken token)
        {
            float[] buffer = new float[length];
            foreach (var trajectory in Trajectories)
            {
                var trajWave = trajectory.Reconstruct(sampleRate, hopSize, length, token);
                buffer = buffer.Zip(trajWave, (a, b) => a + b).ToArray();
            }
            return buffer;
        }

        /// <summary>
        /// Calculates the difference of a the original wave and the generated sinusoid wave.
        /// </summary>
        /// <param name="originalWave">The original <see cref="Wave"/></param>
        /// <param name="hopSize">The original hop size.</param>
        /// <param name="token">The token to cancel this function.</param>
        /// <returns>The residual <see cref="float[]"/> wave.</returns>
        public void CalculateResidual(Wave originalWave, int hopSize, CancellationToken token)
        {
            int sampleRate = originalWave.WaveFormat.SampleRate * originalWave.WaveFormat.Channels;
            float[] sinusoidWave = Reconstruct(sampleRate, hopSize, originalWave.Data.Length, token);
            float[] residualWave = new float[originalWave.Data.Length];
            int samplingLength = hopSize * 2;
            hopSize = samplingLength / 3;
            float ampScale = 1.125f;
            for (int i = 0; i < originalWave.Data.Length; i += hopSize)
            {
                token.ThrowIfCancellationRequested();
                float[] originalSample = Computer.HannWindowCache.Apply(originalWave.Data.GetSample(i, samplingLength), WindowingType.Full);
                float[] sinusoidSample = Computer.HannWindowCache.Apply(sinusoidWave.GetSample(i, samplingLength), WindowingType.Full);
                var originalComplexArray = Computer.FourierTransform(originalSample.ToComplexArray().ZeroPad(), token, false);
                var sinusoidComplexArray = Computer.FourierTransform(sinusoidSample.ToComplexArray().ZeroPad(), token, false);
                var residualComplexArray = originalComplexArray.Zip(sinusoidComplexArray, (original, sinusoid) => Complex.FromPolarCoordinates(Math.Max(original.Magnitude - sinusoid.Magnitude, 0), original.Phase)).ToArray();
                residualComplexArray = Computer.FourierTransform(residualComplexArray, token, true);
                var residualSample = (residualComplexArray.InverseZeroPad(samplingLength)).Select(_ => (float)(_.Real / ((originalComplexArray.Length) * ampScale))).ToArray();
                residualSample = Computer.HannWindowCache.Apply(residualSample, WindowingType.Full);
                residualWave.AddSample(residualSample, i, samplingLength);
            }
            originalWave.Data = residualWave;
        }

        #endregion

        #region Spectrum Generation

        /// <summary>
        /// Generates a <see cref="Spectrum"/> of this <see cref="Sinusoid"/> at the given ratio in time.
        /// </summary>
        /// <param name="ratio">The ratio in time where the <see cref="Spectrum"/> should be generated at.</param>
        /// <returns>The generated <see cref="Spectrum"/>.</returns>
        public Spectrum GenerateSpectrumAt(double ratio)
        {
            var spectrum = new Spectrum();
            var relevantTrajectories = GetSelectedTrajectories((ratio, ratio));
            foreach (var t in relevantTrajectories)
            {
                int pointIndex = 0;
                while (pointIndex + 1 < t.SpectralPoints.Count && t.SpectralPoints[pointIndex + 1].Position < ratio)
                {
                    pointIndex++;
                }
                if (pointIndex < t.SpectralPoints.Count && t.SpectralPoints[pointIndex].Position <= ratio)
                {
                    double lerpRange = t.SpectralPoints[pointIndex + 1].Position - t.SpectralPoints[pointIndex].Position;
                    double lerpValue = (ratio - t.SpectralPoints[pointIndex].Position) / lerpRange;
                    float amp = Computer.Lerp(t.SpectralPoints[pointIndex].Value.Amplitude, t.SpectralPoints[pointIndex + 1].Value.Amplitude, lerpValue);
                    float freq = Computer.LogarithmicFrequencyToFrequency(
                                     Computer.Lerp(
                                         Computer.FrequencyToLogarithmicFrequency(t.SpectralPoints[pointIndex].Value.Frequency),
                                         Computer.FrequencyToLogarithmicFrequency(t.SpectralPoints[pointIndex + 1].Value.Amplitude), lerpValue));
                    spectrum.Spectrals.Add(new SpectralUnit(amp, freq));
                }
            }
            return spectrum;
        }

        #endregion

        #region Hop Dictionary Generation

        /// <summary>
        /// Generates a nested <see cref="Dictionary{int, Dictionary{(int, int)}}"/> of <see cref="SpectralUnit"/> points from this <see cref="Sinusoid"/>. The outer key is the hop index, while the inner key is the trajectory index.
        /// </summary>
        /// <returns>The nested <see cref="Dictionary{int, Dictionary{(int, int)}}"/> of <see cref="SpectralUnit"/> points containing the <see cref="SpectralTrajectory"/> and <see cref="RatioPoint{SpectralUnit}"/> indexes.</returns>
        public Dictionary<int, Dictionary<int, int>> GenerateHopIndexDictionary()
        {
            var hopDictionary = new Dictionary<int, Dictionary<int, int>>();
            for (int trajInd = 0; trajInd < Trajectories.Count; trajInd++)
            {
                for (int pointInd = 0; pointInd < Trajectories[trajInd].SpectralPoints.Count; pointInd++)
                {
                    int globalHopIndex = Trajectories[trajInd].StartHopIndex + pointInd;
                    if (!hopDictionary.ContainsKey(globalHopIndex))
                    {
                        hopDictionary.Add(globalHopIndex, new Dictionary<int, int>());
                    }
                    hopDictionary[globalHopIndex].Add(trajInd, pointInd);
                }
            }
            return hopDictionary;
        }

        #endregion

        /// <summary>
        /// Gets the a list of <see cref="SpectralTrajectory"/>s from this <see cref="Sinusoid"/> inside a given selection range.
        /// </summary>
        /// <param name="selection">The selection ratio range.</param>
        /// <returns>A <see cref="SpectralTrajectory"/> list.</returns>
        public List<SpectralTrajectory> GetSelectedTrajectories((double start, double end) selection)
        {
            return (from t in Trajectories
                    where t.SpectralPoints[0].Position < selection.end && t.SpectralPoints[t.SpectralPoints.Count - 1].Position > selection.start
                    select t).ToList();
        }

        /// <inheritdoc/>
        public override BaseModel GetDeepCopy()
        {
            return new Sinusoid(this);
        }

        #endregion

        #region Constructors

        [JsonConstructor]
        public Sinusoid(int spectrumCount) { SpectrumCount = spectrumCount; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sinusoid"/> class by creating a deep copy of the given <see cref="Sinusoid"/>.
        /// </summary>
        /// <param name="sinusoid">The <see cref="Sinusoid"/> to copy.</param>
        public Sinusoid(Sinusoid sinusoid)
        {
            SpectrumCount = sinusoid.SpectrumCount;
            foreach (var t in sinusoid.Trajectories)
            {
                Trajectories.Add(new SpectralTrajectory(t));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sinusoid"/> class by interpolating between two existing <see cref="Sinusoid"/>s.
        /// This function can be cancelled.
        /// </summary>
        /// <param name="a">The first <see cref="Sinusoid"/>.</param>
        /// <param name="b">The second <see cref="Sinusoid"/>.</param>
        /// <param name="lerpValue">The lerp value.</param>
        /// <param name="aFreqScale">The freq scale of the first <see cref="Sinusoid"/>.</param>
        /// <param name="bFreqScale">The freq scale of the second <see cref="Sinusoid"/>.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to cancel this function.</param>
        public Sinusoid(Sinusoid a, Sinusoid b, double lerpValue, float aFreqScale, float bFreqScale, CancellationToken token)
        {
            if (a == b)
            {
                var aSinusoid = new Sinusoid(a);
                foreach (var t in aSinusoid.Trajectories)
                {
                    for (int i = 0; i < t.SpectralPoints.Count; i++)
                    {
                        t.SpectralPoints[i] = new RatioPoint<SpectralUnit>(new SpectralUnit(t.SpectralPoints[i].Value.Amplitude, t.SpectralPoints[i].Value.Frequency * aFreqScale), t.SpectralPoints[i].Position);
                    }
                }
                SpectrumCount = aSinusoid.SpectrumCount;
                Trajectories.AddRange(aSinusoid.Trajectories);
            }
            else
            {
                var aSinusoid = new Sinusoid(a);
                foreach (var t in aSinusoid.Trajectories)
                {
                    for (int i = 0; i < t.SpectralPoints.Count; i++)
                    {
                        t.SpectralPoints[i] = new RatioPoint<SpectralUnit>(new SpectralUnit(t.SpectralPoints[i].Value.Amplitude, t.SpectralPoints[i].Value.Frequency * aFreqScale), t.SpectralPoints[i].Position);
                    }
                }
                var bSinusoid = new Sinusoid(b);
                foreach (var t in bSinusoid.Trajectories)
                {
                    for (int i = 0; i < t.SpectralPoints.Count; i++)
                    {
                        t.SpectralPoints[i] = new RatioPoint<SpectralUnit>(new SpectralUnit(t.SpectralPoints[i].Value.Amplitude, t.SpectralPoints[i].Value.Frequency * bFreqScale), t.SpectralPoints[i].Position);
                    }
                }
                token.ThrowIfCancellationRequested();
                var primarySinusoid = aSinusoid.SpectrumCount > bSinusoid.SpectrumCount ? aSinusoid : bSinusoid;
                SpectrumCount = primarySinusoid.SpectrumCount;
                var secondarySinusoid = primarySinusoid == aSinusoid ? new Sinusoid(bSinusoid, SpectrumCount, token) : new Sinusoid(aSinusoid, SpectrumCount, token);
                lerpValue = primarySinusoid == aSinusoid ? 1.0 - lerpValue : lerpValue;
                var primaryHopDictionary = primarySinusoid.GenerateHopIndexDictionary();
                var secondaryHopDictionary = secondarySinusoid.GenerateHopIndexDictionary();
                float logaritmicMergeDistance = 0.25f;
                for (int i = 0; i < SpectrumCount; i++)
                {
                    if (primaryHopDictionary.ContainsKey(i) && secondaryHopDictionary.ContainsKey(i))
                    {
                        var primaryFrequencies = primaryHopDictionary[i].Select(_ => (_.Key, _.Value, Computer.FrequencyToLogarithmicFrequency(primarySinusoid.Trajectories[_.Key].SpectralPoints[_.Value].Value.Frequency))).ToList();
                        var secondaryFrequencies = secondaryHopDictionary[i].Select(_ => (_.Key, _.Value, Computer.FrequencyToLogarithmicFrequency(secondarySinusoid.Trajectories[_.Key].SpectralPoints[_.Value].Value.Frequency))).ToList();
                        var distanceMatrix = new FloatDistanceMatrix(primaryFrequencies.Select(_ => _.Item3).ToList(), secondaryFrequencies.Select(_ => _.Item3).ToList(), logaritmicMergeDistance);
                        var pairs = distanceMatrix.CalculateClosestPairs();
                        foreach (var p in pairs)
                        {
                            if (p.rowIndex != -1 && p.columnIndex != -1)
                            {
                                var primarySpectral = primarySinusoid.Trajectories[primaryFrequencies[p.rowIndex].Key].SpectralPoints[primaryFrequencies[p.rowIndex].Value].Value;
                                var secondarySpectral = secondarySinusoid.Trajectories[secondaryFrequencies[p.columnIndex].Key].SpectralPoints[secondaryFrequencies[p.columnIndex].Value].Value;
                                var lerpSpectral = new SpectralUnit(Computer.Lerp(secondarySpectral.Amplitude, primarySpectral.Amplitude, lerpValue),
                                                                    Computer.LogarithmicFrequencyToFrequency(Computer.Lerp(Computer.FrequencyToLogarithmicFrequency(secondarySpectral.Frequency), Computer.FrequencyToLogarithmicFrequency(primarySpectral.Frequency), lerpValue)));
                                primarySinusoid.Trajectories[primaryFrequencies[p.rowIndex].Key].SpectralPoints[primaryFrequencies[p.rowIndex].Value] = new RatioPoint<SpectralUnit>(lerpSpectral, primarySinusoid.Trajectories[primaryFrequencies[p.rowIndex].Key].SpectralPoints[primaryFrequencies[p.rowIndex].Value].Position);
                                secondarySinusoid.Trajectories[secondaryFrequencies[p.columnIndex].Key].SpectralPoints[secondaryFrequencies[p.columnIndex].Value] = new RatioPoint<SpectralUnit>(new SpectralUnit(0, lerpSpectral.Frequency), secondarySinusoid.Trajectories[secondaryFrequencies[p.columnIndex].Key].SpectralPoints[secondaryFrequencies[p.columnIndex].Value].Position);
                            }
                        }
                    }
                    token.ThrowIfCancellationRequested();
                }
                Trajectories.AddRange(primarySinusoid.Trajectories);
                Trajectories.AddRange(secondarySinusoid.Trajectories);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sinusoid"/> class by creation a deep copy of the given <see cref="Sinusoid"/> with the given spectrum count resolution.
        /// Note that the resolution should be equal or greater than the given <see cref="Sinusoid.SpectrumCount"/>.
        /// This function can be cancelled.
        /// </summary>
        /// <param name="sinusoid">The <see cref="Sinusoid"/> to deep copy.</param>
        /// <param name="spectrumCount">The new resolution as the <see cref="Spectrum"/> count number.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to cancel this function.</param>
        private Sinusoid(Sinusoid sinusoid, int spectrumCount, CancellationToken token)
        {
            SpectrumCount = spectrumCount;
            var hopDictionary = sinusoid.GenerateHopIndexDictionary();
            int startHopIndex = 0;
            double lerpRange = 1.0 / sinusoid.SpectrumCount;
            var trajectoryDictionary = new Dictionary<int, SpectralTrajectory>();
            var spectralList = new List<(int trajInd, SpectralUnit spectral)>();
            for (int newHopIndex = 0; newHopIndex < spectrumCount; newHopIndex++)
            {
                token.ThrowIfCancellationRequested();
                spectralList.Clear();
                double primaryPosition = newHopIndex / (double)spectrumCount;
                // serach closest spectral points
                while (startHopIndex + 1 < sinusoid.SpectrumCount && startHopIndex / (double)sinusoid.SpectrumCount < primaryPosition)
                {
                    startHopIndex++;
                }
                // interpolate spectral points to match the new resolution
                if (startHopIndex + 1 < sinusoid.SpectrumCount)
                {
                    if (hopDictionary.ContainsKey(startHopIndex) && hopDictionary.ContainsKey(startHopIndex + 1))
                    {
                        var startTrajIndexes = hopDictionary[startHopIndex].Select(t => t.Key).ToArray();
                        var endTrajIndexes = hopDictionary[startHopIndex + 1].Select(t => t.Key).ToArray();
                        var intersectIndexes = startTrajIndexes.Intersect(endTrajIndexes).ToArray();
                        foreach (var index in intersectIndexes)
                        {
                            var startSpectral = sinusoid.Trajectories[index].SpectralPoints[hopDictionary[startHopIndex][index]].Value;
                            var endSpectral = sinusoid.Trajectories[index].SpectralPoints[hopDictionary[startHopIndex + 1][index]].Value;
                            double lerpValue = (primaryPosition - (startHopIndex / (double)sinusoid.SpectrumCount)) / lerpRange; ;
                            spectralList.Add((index, new SpectralUnit(Computer.Lerp(startSpectral.Amplitude, endSpectral.Amplitude, lerpValue),
                                                                      Computer.LogarithmicFrequencyToFrequency(Computer.Lerp(Computer.FrequencyToLogarithmicFrequency(startSpectral.Frequency), Computer.FrequencyToLogarithmicFrequency(endSpectral.Frequency), lerpValue)))));
                        }
                        var remainingStartIndexes = startTrajIndexes.Except(intersectIndexes);
                        foreach (var index in remainingStartIndexes)
                        {
                            spectralList.Add((index, new SpectralUnit(0, sinusoid.Trajectories[index].SpectralPoints[hopDictionary[startHopIndex][index]].Value.Frequency)));
                        }
                        var remainingEndIndexes = endTrajIndexes.Except(intersectIndexes);
                        foreach (var index in remainingEndIndexes)
                        {
                            spectralList.Add((index, new SpectralUnit(0, sinusoid.Trajectories[index].SpectralPoints[hopDictionary[startHopIndex + 1][index]].Value.Frequency)));
                        }
                    }
                    else if (hopDictionary.ContainsKey(startHopIndex))
                    {
                        foreach (var tuple in hopDictionary[startHopIndex])
                        {
                            spectralList.Add((tuple.Key, new SpectralUnit(0, sinusoid.Trajectories[tuple.Key].SpectralPoints[tuple.Value].Value.Frequency)));
                        }
                    }
                    else if (hopDictionary.ContainsKey(startHopIndex + 1))
                    {
                        foreach (var tuple in hopDictionary[startHopIndex + 1])
                        {
                            spectralList.Add((tuple.Key, new SpectralUnit(0, sinusoid.Trajectories[tuple.Key].SpectralPoints[tuple.Value].Value.Frequency)));
                        }
                    }
                }
                else
                {
                    if (hopDictionary.ContainsKey(startHopIndex))
                    {
                        foreach (var tuple in hopDictionary[startHopIndex])
                        {
                            spectralList.Add((tuple.Key, new SpectralUnit(0, sinusoid.Trajectories[tuple.Key].SpectralPoints[tuple.Value].Value.Frequency)));
                        }
                    }
                }
                // continue trajectories
                foreach (var s in spectralList)
                {
                    if (!trajectoryDictionary.ContainsKey(s.trajInd))
                    {
                        var newTrajectory = new SpectralTrajectory(new RatioPoint<SpectralUnit>(s.spectral, primaryPosition))
                        {
                            StartHopIndex = newHopIndex
                        };
                        trajectoryDictionary.Add(s.trajInd, newTrajectory);
                        Trajectories.Add(newTrajectory);
                    }
                    else
                    {
                        trajectoryDictionary[s.trajInd].SpectralPoints.Add(new RatioPoint<SpectralUnit>(s.spectral, primaryPosition));
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sinusoid"/> class from the given <see cref="Wave"/>. The <see cref="Wave"/> is updated as the <see cref="Sinusoid"/> is removed from it. This function can be cancelled.
        /// </summary>
        /// <param name="wave">The <see cref="Wave"/>.</param>
        /// <param name="samplingFrequency">The sampling frequency.</param>
        /// <param name="areFrequenciesFixed">Indicates whether the frequencies are fixed or not.</param>
        /// <param name="minimumDecibel">The minimum deibel amplitude of any <see cref="SpectralUnit"/>.</param>
        /// <param name="continuationRange">The maximum range in log 12 base logarithmic values where two <see cref="SpectralUnit"/>s are connected.</param>
        /// <param name="maxSleepingTime">The maximum time a trajectory can sleep in milliseconds.</param>
        /// <param name="maxSleepingTime">The minimum length of any trajectory in milliseconds.</param>
        /// <param name="token">The <see cref="CancellationToken"/> to cancel this function.</param>
        public Sinusoid(Wave wave, float samplingFrequency, bool areFrequenciesFixed, float minimumDecibel, float continuationRange, int maxSleepingTime, int minimumLength, CancellationToken token)
        {
            int sampleRate = wave.WaveFormat.SampleRate * wave.WaveFormat.Channels;
            int samplingLength = Computer.ClampMin((int)(sampleRate / samplingFrequency), (int)(sampleRate / ProjectModel.MaximumSamplingFrequency));
            int hopScale = 1;
            int hopSize = (int)(samplingLength * (1.0 - (Computer.HannWindowCache.EffectiveOffsetRatio * 2.0)) / hopScale);
            double loadingPercent = 0.1;
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio *= loadingPercent;
            var spectrums = areFrequenciesFixed ? GetFixedFrequencySpectrumsFromWave(wave, samplingLength, hopSize, sampleRate, minimumDecibel, token) : GetSpectrumsFromWave(wave, samplingLength, hopSize, sampleRate, minimumDecibel, token);
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio /= loadingPercent;
            SpectrumCount = spectrums.Count;
            loadingPercent = 0.9;
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio *= loadingPercent;
            GenerateSpectralTrajectories(spectrums, (int)((double)hopSize / sampleRate * 1000.0), continuationRange, maxSleepingTime, minimumLength, token);
            Instrument.InstrumentGenerationPercentManager.PercentStepRatio /= loadingPercent;
            CalculateResidual(wave, hopSize, token);
        }


        #endregion
    }
}
