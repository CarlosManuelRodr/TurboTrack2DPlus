using System.Collections.Generic;
using UnityEngine;

namespace Physics
{
    /// <summary>
    /// Interface for a world object that contains a collider.
    /// </summary>
    public interface ICollidable
    {
        /// <summary>
        /// Properties of a Pseudo3D collider.
        /// </summary>
        Pseudo3DCollider Pseudo3DCollider { get; }
        
        /// <summary>
        /// Temp field to store the projected collider in world coordinates.
        /// </summary>
        Dictionary<int, Rect> ColliderBuffer { get; set; }
    }
}