using System;

namespace AudioClickRepair.Data
{
    public abstract class AbstractFragment
    {
        protected double[] internalArray;

        /// <summary>
        /// Gets or sets relative position of the first sample of fragment.
        /// </summary>
        public int StartPosition { get; protected set; }

        /// <summary>
        /// Gets relative position of the last sample of fragment.
        /// </summary>
        public int EndPosition => this.StartPosition + this.Length - 1;

        /// <summary>
        /// Gets length of fragment.
        /// </summary>
        public int Length => this.internalArray.Length;

        /// <summary>
        /// Sets value of internal array using relative position.
        /// </summary>
        /// <param name="position">Relative position.</param>
        /// <param name="value">Value.</param>
        public void SetValue(int position, double value) =>
            this.internalArray[position - this.StartPosition] = value;

        /// <summary>
        /// Gets value from internal array using relative position.
        /// </summary>
        /// <param name="position">Relative position.</param>
        /// <returns>Value.</returns>
        public double GetValue(int position) =>
            this.internalArray[position - this.StartPosition];

        /// <summary>
        /// Gets internal array of samples.
        /// </summary>
        /// <returns>Array of samples.</returns>
        public double[] GetInternalArray() => this.internalArray;

        /// <summary>
        /// Replaces internal array.
        /// </summary>
        /// <param name="replacementArray">New internal array.</param>
        internal void SetInternalArray(double[] replacementArray)
        {
            if (replacementArray is null 
                || replacementArray.Length != this.internalArray.Length)
            {
                throw new ArgumentException(
                    nameof(replacementArray) + " is null, longer or shorter.");
            }

            this.internalArray = replacementArray;
        }
    }
}
