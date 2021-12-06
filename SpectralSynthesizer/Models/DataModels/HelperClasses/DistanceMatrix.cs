using System;
using System.Collections.Generic;
using System.Linq;

namespace SpectralSynthesizer
{
    /// <summary>
    /// An abstract class representing a matrix which allows to calculate the closest distanced pairs of the given values.
    /// </summary>
    /// <typeparam name="T">The type of the matrix's values.</typeparam>
    public abstract class DistanceMatrix<T> where T : IComparable, IComparable<T>
    {
        #region Properties

        /// <summary>
        /// The raw matrix containing the distances of all the values, regardless of the maximum distance.
        /// </summary>
        public T[,] Matrix { get; set; }

        /// <summary>
        /// The maximum distance used in comparisons.
        /// </summary>
        public T MaxDistance { get; private set; }

        /// <summary>
        /// The vector containing the <see cref="Matrix"/>'s global indexes in ascending form.
        /// </summary>
        private List<int> OrderedGlobalIndexVector { get; } = new List<int>();

        #endregion

        #region Methods

        /// <summary>
        /// Creates the <see cref="Matrix"/> and the<see cref="OrderedGlobalIndexVector"/>.
        /// </summary>
        /// <param name="rows">The rows of the matrix.</param>
        /// <param name="columns">The columns of the matrix.</param>
        /// <param name="maxDistance">The maximum distance of two <see cref="T"/> values.</param>
        private void CreateMatrix(IList<T> rows, IList<T> columns, T maxDistance)
        {
            MaxDistance = maxDistance;
            Matrix = new T[rows.Count, columns.Count];
            var tempList = new List<T>();
            for (int rowInd = 0; rowInd < rows.Count; rowInd++)
            {
                for (int columnInd = 0; columnInd < columns.Count; columnInd++)
                {
                    T value = DistanceOf(rows[rowInd], columns[columnInd]);
                    Matrix[rowInd, columnInd] = value;
                    int index = tempList.AddInOrder(value);
                    OrderedGlobalIndexVector.Insert(index, ToGlobalIndex(rowInd, columnInd));
                }
            }
        }

        /// <summary>
        /// Converts a global index to matrix index.
        /// </summary>
        /// <param name="globalIndex">The global index.</param>
        /// <returns>The matrix index touple.</returns>
        private (int rowIndex, int columnIndex) ToMatrixIndex(int globalIndex) => (ToRowIndex(globalIndex), ToColumnIndex(globalIndex));

        /// <summary>
        /// Converts a global index to row index.
        /// </summary>
        /// <param name="globalIndex">The global index.</param>
        /// <returns>The row index.</returns>
        private int ToRowIndex(int globalIndex) => globalIndex / Matrix.GetLength(1);

        /// <summary>
        /// Converts a global index to column index.
        /// </summary>
        /// <param name="globalIndex">The global index.</param>
        /// <returns>The column index.</returns>
        private int ToColumnIndex(int globalIndex) => globalIndex % Matrix.GetLength(1);

        /// <summary>
        /// Converts a matrix index to global index.
        /// </summary>
        /// <param name="rowIndex">The row index.<param>
        /// <param name="columnIndex">The column index.<param>
        /// <returns>The matrix index touple.</returns>
        private int ToGlobalIndex(int rowIndex, int columnIndex) => rowIndex * Matrix.GetLength(1) + columnIndex;

        /// <summary>
        /// Calculates the closest ( <see cref="SpectralUnit"/> , <see cref="SpectralPoint"/> ) pairs from the given distance matrix.
        /// If a pair is not found in the given range, -1 is used instead of the item's index.
        /// </summary>
        /// <returns>The list of the closest pairs containing the indexes of the items.</returns>
        public List<(int rowIndex, int columnIndex)> CalculateClosestPairs()
        {
            var pairList = new List<(int, int)>();
            var globalIndexes = new List<int>(OrderedGlobalIndexVector);
            List<int> unpairedRowIndexes = new List<int>();
            for (int i = 0; i < Matrix.GetLength(0); i++)
            {
                unpairedRowIndexes.Add(i);
            }
            List<int> unpairedColumnIndexes = new List<int>();
            for (int i = 0; i < Matrix.GetLength(1); i++)
            {
                unpairedColumnIndexes.Add(i);
            }
            while (globalIndexes.Count > 0 && Matrix[ToRowIndex(globalIndexes[0]), ToColumnIndex(globalIndexes[0])].CompareTo(MaxDistance) <= 0)
            {
                var matrixIndex = ToMatrixIndex(globalIndexes[0]);
                pairList.Add((matrixIndex.rowIndex, matrixIndex.columnIndex));
                unpairedRowIndexes.Remove(matrixIndex.rowIndex);
                unpairedColumnIndexes.Remove(matrixIndex.columnIndex);
                for (int row = 0; row < Matrix.GetLength(0); row++)
                {
                    globalIndexes.Remove(ToGlobalIndex(row, matrixIndex.columnIndex));
                }
                for (int column = 0; column < Matrix.GetLength(1); column++)
                {
                    globalIndexes.Remove(ToGlobalIndex(matrixIndex.rowIndex, column));
                }
            }
            // add not found pairs
            foreach (var rowIndex in unpairedRowIndexes)
            {
                pairList.Add((rowIndex, -1));
            }
            foreach (var columnIndex in unpairedColumnIndexes)
            {
                pairList.Add((-1, columnIndex));
            }
            return pairList;
        }

        #region Abstract Operators

        /// <summary>
        /// Return the <see cref="T"/> distance between two <see cref="T"/> values.
        /// </summary>
        /// <param name="a">The first <see cref="T"/> value.</param>
        /// <param name="b">The second <see cref="T"/> value.</param>
        /// <returns>The <see cref="T"/> distance between two <see cref="T"/> values.</returns>
        protected abstract T DistanceOf(T a, T b);

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DistanceMatrix{T}"/> class.
        /// </summary>
        /// <param name="rows">The rows of the matrix.</param>
        /// <param name="columns">The columns of the matrix.</param>
        /// <param name="maxDistance">The maximum distance of two <see cref="T"/> values.</param>
        public DistanceMatrix(IList<T> rows, IList<T> columns, T maxDistance)
        {
            CreateMatrix(rows, columns, maxDistance);
        }

        #endregion
    }

    /// <summary>
    /// Represents a <see cref="DistanceMatrix{T}"/> with <see cref="int"/> values.
    /// </summary>
    public class IntegerDistanceMatrix : DistanceMatrix<int>
    {
        #region Operators Methods

        /// <inheritdoc/>
        protected override int DistanceOf(int a, int b) => Math.Abs(a - b);
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegerDistanceMatrix"/> class.
        /// </summary>
        /// <param name="rows">The rows of the matrix.</param>
        /// <param name="columns">The columns of the matrix.</param>
        /// <param name="maxDistance">The maximum distance of two integer values.</param>
        public IntegerDistanceMatrix(IList<int> rows, IList<int> columns, int maxDistance) : base(rows, columns, maxDistance) { }

        #endregion
    }

    /// <summary>
    /// Represents a <see cref="DistanceMatrix{T}"/> with <see cref="float"/> values.
    /// </summary>
    public class FloatDistanceMatrix : DistanceMatrix<float>
    {
        #region Operators Methods

        /// <inheritdoc/>
        protected override float DistanceOf(float a, float b) => MathF.Abs(a - b);
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="FloatDistanceMatrix"/> class.
        /// </summary>
        /// <param name="rows">The rows of the matrix.</param>
        /// <param name="columns">The columns of the matrix.</param>
        /// <param name="maxDistance">The maximum distance of two float values.</param>
        public FloatDistanceMatrix(IList<float> rows, IList<float> columns, float maxDistance) : base(rows, columns, maxDistance) { }

        #endregion
    }
}
