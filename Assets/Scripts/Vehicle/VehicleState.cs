using System;

namespace Vehicle
{
    /// <summary>
    /// Represents the state of a vehicle on the road.
    /// </summary>
    [Serializable]
    public struct VehicleState
    {
        /// <summary>
        /// The vehicle current horizontal (turning) position in world coordinates.
        /// </summary>
        public float horizontalPosition;

        /// <summary>
        /// The vehicle's current horizontal (turning) speed.
        /// </summary>
        public float horizontalSpeed;

        /// <summary>
        /// The vehicle's current frontal speed in track coordinates.
        /// </summary>
        public float trackSpeed;

        /// <summary>
        /// The position in track coordinates.
        /// </summary>
        public float trackPosition;
    }
}