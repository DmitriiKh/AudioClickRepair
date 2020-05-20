// <copyright file="IRegenerator.cs" company="Dmitrii Khrustalev">
// Copyright (c) Dmitrii Khrustalev. All rights reserved.
// </copyright>

namespace CarefulAudioRepair.Processing
{
    using CarefulAudioRepair.Data;

    /// <summary>
    /// Regenerates sequence of audio samples.
    /// </summary>
    internal interface IRegenerator
    {
        /// <summary>
        /// Gets number of input samples needed in order to make regeneration.
        /// </summary>
        int InputDataSize { get; }

        /// <summary>
        /// Restores the sequence of audio samples inside patch.
        /// </summary>
        /// <param name="patch">Patch that needs restoration.</param>
        void RestorePatch(AbstractPatch patch);
    }
}
