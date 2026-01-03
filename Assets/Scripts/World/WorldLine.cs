using System;
using System.Diagnostics.CodeAnalysis;

namespace World
{
    /// <summary>
    /// The world position of a line.
    /// </summary>
    [Serializable]
    [SuppressMessage("ReSharper", "InconsistentNaming")] // Because caps are used to distinguish world coordinates.
    public struct WorldLine
    {
        /// <summary>
        /// x, y and z positions of the center of the segment.
        /// </summary>
        public float X, Y, Z;

        /// <summary>
        /// Half the width of the road (from camera to road edge)
        /// </summary>
        public float W;
    }
}