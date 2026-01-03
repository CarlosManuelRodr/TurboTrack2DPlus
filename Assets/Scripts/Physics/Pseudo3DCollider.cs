using System;
using System.Diagnostics.CodeAnalysis;
using TriInspector;
using UnityEngine;

namespace Physics
{
    /// <summary>
    /// Properties of a Pseudo3D collider.
    /// </summary>
    [Serializable]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class Pseudo3DCollider
    {
        /// <summary>
        /// The type of collider. Intended to define its physic properties.
        /// </summary>
        [SerializeField] private ColliderType colliderType = ColliderType.Unmovable;
        
        /// <summary>
        /// The vehicle collider length in world coordinates.
        /// </summary>
        [HideIf("NoCollider")] [SerializeField] private float colliderLength = 200f;
        
        /// <summary>
        /// The x scale of the collider.
        /// </summary>
        [HideIf("NoCollider")] [SerializeField] private Vector2 colliderScale = Vector2.one;

        /// <summary>
        /// An offset for the collider x coordinate.
        /// </summary>
        [HideIf("NoCollider")] [SerializeField] private Vector2 colliderOffset = Vector2.zero;

        // Properties
        private bool NoCollider => (colliderType == ColliderType.None);
        public ColliderType ColliderType => colliderType;
        public float ColliderLength => colliderLength;
        public Vector2 ColliderScale => colliderScale;
        public Vector2 ColliderOffset => colliderOffset;
    }
}