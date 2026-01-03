using UnityEngine;

namespace Core.Extensions
{
    /// <summary>
    /// Utilities for extending the vector functionality.
    /// </summary>
    internal static class VectorExtensions
    {
        /// <summary>
        /// Is i in this vector range?
        /// </summary>
        /// <param name="range">A Vector2 representing a range.</param>
        /// <param name="i">The test value.</param>
        /// <returns>True if in range, false otherwise.</returns>
        public static bool InRange(this Vector2Int range, int i)
        {
            return i > range.x && i < range.y;
        }
    }
}
