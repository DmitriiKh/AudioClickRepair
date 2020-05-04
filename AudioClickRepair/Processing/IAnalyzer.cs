// <copyright file="IAnalyzer.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace AudioClickRepair.Processing
{
    /// <summary>
    /// IAnalyzer interface.
    /// </summary>
    public interface IAnalyzer
    {
        /// <summary>
        /// Gets size of input data array needed to perform calculations.
        /// </summary>
        /// <returns>Size.</returns>
        int InputDataSize { get; }

        /// <summary>
        /// Gets default result. Can be used if calculation can not be done.
        /// </summary>
        /// <returns>Default result.</returns>
        double DefaultResult { get; }

        /// <summary>
        /// Returns result of calculation.
        /// </summary>
        /// <param name="inputData">Input data.</param>
        /// <returns>Result.</returns>
        double GetResult(double[] inputData);
    }
}
