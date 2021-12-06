using System;
using System.Collections.Generic;

namespace SpectralSynthesizer
{
    /// <summary>
    /// A comparer class used by the <see cref="Computer"/>.
    /// </summary>
    public class ComputerComparer : IComparer<float>
    {
        public int Compare(float x, float y)
        {
            if (MathF.Abs(x - y) < Computer.CompareDelta)
                return 0;
            else if (x < y)
                return -1;
            else
                return 1;
        }
    }
}
