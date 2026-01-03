using System;
using Physics;
using TriInspector;
using UnityEngine;

namespace Level
{
    /// <summary>
    /// A traffic vehicle that circles the level.
    /// </summary>
    [Serializable]
    [DeclareTabGroup("Collider")]
    public class LevelVehicle
    {
        /// <summary>
        /// The amount of vehicles of this type.
        /// </summary>
        public int amount;
        
        /// <summary>
        /// The constant road speed of the vehicles.
        /// </summary>
        public float speed;
        
        /// <summary>
        /// The vehicles sprite.
        /// </summary>
        [PreviewObject] public Sprite sprite;

        /// <summary>
        /// The vehicle collider properties.
        /// </summary>
        [HideLabel]
        [Group("Collider"), Tab("Collider")]
        public Pseudo3DCollider pseudo3DCollider; 
    }
}