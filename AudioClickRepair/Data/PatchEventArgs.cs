// <copyright file="PatchEventArgs.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace AudioClickRepair.Data
{
    /// <summary>
    /// Arguments for calling event handler.
    /// </summary>
    public class PatchEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PatchEventArgs"/> class.
        /// </summary>
        /// <param name="newStartPosition">Start position of fragment.</param>
        /// <param name="newLength">Length of fragment.</param>
        public PatchEventArgs(int newStartPosition, int newLength) =>
            this.Patched = new ArrayFragment(
                new double[newLength],
                newStartPosition);

        /// <summary>
        /// Gets fragment. Needs to be updated by event handler.
        /// </summary>
        public AbstractFragment Patched { get; }

        /// <summary>
        /// Gets error level. Needs to be updated by event handler.
        /// </summary>
        public double NewErrorLevelAtStart { get; internal set; }
    }
}