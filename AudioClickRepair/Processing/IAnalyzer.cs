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
        /// Returns size of input data array needed to perform calculations.
        /// </summary>
        /// <returns>Size.</returns>
        int GetInputDataSize();

        /// <summary>
        /// Returns default result. Can be used if calculation can not be done.
        /// </summary>
        /// <returns>Default result.</returns>
        double GetDefaultResult();

        /// <summary>
        /// Returns result of calculation.
        /// </summary>
        /// <param name="inputData">Input data.</param>
        /// <returns>Result.</returns>
        double GetResult(double[] inputData);
    }
}
