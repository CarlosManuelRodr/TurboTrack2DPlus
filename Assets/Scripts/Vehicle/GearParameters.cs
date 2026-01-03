using System;
using TriInspector;

namespace Vehicle
{
    /// <summary>
    /// Defines the vehicle physics properties of a gear level.
    /// </summary>
    [Serializable]
    public class GearParameters
    {
        [Title("Parameters")]
        public float acceleration = 7000;
        public float friction = 0.4f;
        public float driftingFriction = 1f;
        public float outOfRoadFriction = 1f;

        [Title("Debug")]
        [ShowInInspector] public float MaxSpeed => CalculateMaxSpeed();

        /// <summary>
        /// Calculate the maximum achievable speed.
        /// </summary>
        public float CalculateMaxSpeed()
        {
            return acceleration / friction;
        }
    }
}