using System;
using UnityEngine;

namespace Rendering
{
    /// <summary>
    /// Screen coordinates produced by projecting a segment.
    /// </summary>
    [Serializable]
    public struct ProjectedLine
    {
        /// <summary>
        /// The screen coordinates of a projection. 
        /// </summary>
        public Vector2 coord;

        /// <summary>
        /// The projected width of the segment.
        /// </summary>
        public float w;
        
        /// <summary>
        /// The scaling factor for scaling between the normalized projection plane and
        /// the screen coordinates.
        /// </summary>
        public float scalingFactor;
        
        /// <summary>
        /// The highest height seen so far on the scene. If the coord y has a lower value
        /// than the clip value, the projected line won't be rendered.
        /// </summary>
        public float clip;
        
        /// <summary>
        /// Lineal segment interpolation.
        /// </summary>
        /// <param name="a">The first segment.</param>
        /// <param name="b">The last segment.</param>
        /// <param name="t">The interpolation parameter</param>
        /// <returns></returns>
        public static ProjectedLine Lerp(ProjectedLine a, ProjectedLine b, float t)
        {
            ProjectedLine result;
            result.coord = Vector2.Lerp(a.coord, b.coord, t);
            result.w = Mathf.Lerp(a.w, b.w, t);
            result.scalingFactor = Mathf.Lerp(a.scalingFactor, b.scalingFactor, t);
            result.clip = Mathf.Lerp(a.clip, b.clip, t);
            return result;
        }
    }
}