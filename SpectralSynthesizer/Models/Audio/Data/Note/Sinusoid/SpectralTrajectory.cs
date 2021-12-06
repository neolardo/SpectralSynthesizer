using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SpectralSynthesizer.Models.Audio.Data
{
    /// <summary>
    /// Represents the trajectory of a <see cref="SpectralUnit"/>s.
    /// </summary>
    public class SpectralTrajectory : BaseModel
    {
        #region Properties

        /// <summary>
        /// The list of <see cref="RatioPoint"/>s this trajectory consists of.
        /// </summary>
        public List<RatioPoint<SpectralUnit>> SpectralPoints { get; set; } = new List<RatioPoint<SpectralUnit>>();

        [JsonProperty]
        /// <summary>
        /// The randomly generated starting phase of this trajectory. Ranges from 0.0 to 1.0.
        /// </summary>
        public double StartPhaseRatio { get; }

        /// <summary>
        /// The starting hop index of this <see cref="SpectralTrajectory"/>.
        /// </summary>
        public int StartHopIndex { get; set; }

        #endregion

        #region Methods

        #region Rendering

        /// <summary>
        /// Renders this <see cref="SpectralTrajectory"/>. This function can be cancelled.
        /// </summary>
        /// <param name="sampleRate">The sample rate.</param
        /// <param name="length">The full length of the <see cref="TimeDomainModel"/> containing this <see cref="SpectralTrajectory"/>, measured in float audio data units.</param>
        /// <param name="token">The cancellation token to cancel this function.</param>
        /// <returns>The float array containing the rendered wave.</returns>
        public float[] Render(int sampleRate, int length, CancellationToken token)
        {
            float[] buffer = new float[length];
            if (SpectralPoints.Count > 1)
            {
                double phase = StartPhaseRatio * Computer.SineWaveCache.Length;
                int offset = (int)(SpectralPoints[0].Position * length);
                for (int pointInd = 0; pointInd < SpectralPoints.Count - 1 && offset < length; pointInd++)
                {
                    token.ThrowIfCancellationRequested();
                    int sineLength = Computer.ClampMax((int)((SpectralPoints[pointInd + 1].Position - SpectralPoints[pointInd].Position) * length), length - offset);
                    var sine = SpectralUnit.RenderSineWave(sampleRate, SpectralPoints[pointInd].Value, SpectralPoints[pointInd + 1].Value, sineLength, ref phase);
                    Array.Copy(sine, 0, buffer, offset, Computer.ClampMax(sineLength, length - offset));
                    offset += sineLength;
                }
            }
            return buffer;
        }

        /// <summary>
        /// Reconstructs this <see cref="SpectralTrajectory"/>. This function can be cancelled.
        /// </summary>
        /// <param name="sampleRate">The sample rate.</param
        /// <param name="hopSize">The hop size.</param
        /// <param name="length">The full length of the <see cref="TimeDomainModel"/> containing this <see cref="SpectralTrajectory"/>, measured in float audio data units.</param>
        /// <param name="token">The cancellation token to cancel this function.</param>
        /// <returns>The float array containing the rendered audio.</returns>
        public float[] Reconstruct(int sampleRate, int hopSize, int length, CancellationToken token)
        {
            float[] buffer = new float[length];
            if (SpectralPoints.Count > 1)
            {
                double phase = StartPhaseRatio * Computer.SineWaveCache.Length;
                int offset = StartHopIndex * hopSize;
                for (int pointInd = 0; pointInd < SpectralPoints.Count - 1 && offset < length; pointInd++)
                {
                    token.ThrowIfCancellationRequested();
                    int sineLength = Computer.ClampMax(hopSize, length - offset);
                    var sine = SpectralUnit.RenderSineWave(sampleRate, SpectralPoints[pointInd].Value, SpectralPoints[pointInd + 1].Value, sineLength, ref phase);
                    Array.Copy(sine, 0, buffer, offset, sineLength);
                    offset += sineLength;
                }
            }
            return buffer;
        }

        #endregion

        /// <summary>
        /// Adds a given point to the <see cref="SpectralPoints"/> list in the correct position.
        /// </summary>
        /// <param name="point">The given <see cref="RatioPoint"/>.</param>
        public void AddPoint(RatioPoint<SpectralUnit> point)
        {
            SpectralPoints.Sort(new RatioPointPositionComparer<SpectralUnit>());
            int index = 0;
            while (index < SpectralPoints.Count && point.Position > SpectralPoints[index].Position)
            {
                index++;
            }
            SpectralPoints.Insert(index, point);
        }

        /// <inheritdoc/>
        public override BaseModel GetDeepCopy()
        {
            return new SpectralTrajectory(this);
        }

        #endregion

        #region Constructors

        [JsonConstructor]
        public SpectralTrajectory(double startPhaseRatio) { StartPhaseRatio = startPhaseRatio; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectralTrajectory"/> class by creating a deep copy of the given <see cref="SpectralTrajectory"/>.
        /// </summary>
        public SpectralTrajectory(SpectralTrajectory trajectory)
        {
            StartPhaseRatio = trajectory.StartPhaseRatio;
            StartHopIndex = trajectory.StartHopIndex;
            foreach (var p in trajectory.SpectralPoints)
            {
                SpectralPoints.Add(new RatioPoint<SpectralUnit>(new SpectralUnit(p.Value.Amplitude, p.Value.Frequency), p.Position));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectralTrajectory"/> class. 
        /// </summary>
        /// <param name="firstPoint">The first point of the trajectory.</param>
        public SpectralTrajectory(RatioPoint<SpectralUnit> firstPoint)
        {
            AddPoint(firstPoint);
            StartPhaseRatio = Computer.R.NextDouble();
        }

        #endregion
    }
}