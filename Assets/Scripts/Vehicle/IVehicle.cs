using UnityEngine;
using World;

namespace Vehicle
{
    /// <summary>
    /// Interface that defines a vehicle.
    /// </summary>
    public interface IVehicle : ILayeredWorldObject
    {
        /// <summary>
        /// Represents the state (position and speed) of a vehicle on the road.
        /// </summary>
        VehicleState VehicleState { get; }
        
        VehicleProperties VehicleProperties { get; }
        
        Vector2 SpritePosition => new(VehicleState.horizontalPosition + VehicleProperties.offsetX, VehicleProperties.offsetY);
        Vector2 VehiclePosition => new(VehicleState.horizontalPosition, 0f);
        
        public bool Paused { get; set; }
        public bool CollisionIsEnabled { get; set; }
        
        /// <summary>
        /// Handle a frame.
        /// </summary>
        public void HandleUpdate();
    }
}