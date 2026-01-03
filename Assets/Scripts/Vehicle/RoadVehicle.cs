using System;
using System.Collections.Generic;
using Core;
using Level;
using Physics;
using TriInspector;
using UnityEngine;
using World;

namespace Vehicle
{
    /// <summary>
    /// Represents a vehicle that travels the road at a constant speed.
    /// </summary>
    [Serializable]
    [DeclareTabGroup("State")]
    [DeclareTabGroup("Properties")]
    [DeclareTabGroup("Collider")]
    public class RoadVehicle : IVehicle
    {
        /// <summary>
        /// The state of the vehicle on the road.
        /// </summary>
        [Group("State"), Tab("State")]
        [SerializeField] private VehicleState vehicleState;
        
        /// <summary>
        /// Is this object enabled to appear on the world?
        /// </summary>
        [Group("State"), Tab("State")]
        [SerializeField] private bool isEnabled;
        
        /// <summary>
        /// Is this object paused?
        /// </summary>
        [Group("State"), Tab("State")]
        [SerializeField] private bool paused;
        
        /// <summary>
        /// Static properties of the vehicle.
        /// </summary>
        [HideLabel]
        [Group("Properties"), Tab("Properties")]
        [SerializeField] private VehicleProperties vehicleProperties;
        
        /// <summary>
        /// The vehicle collider properties.
        /// </summary>
        [HideLabel]
        [Group("Collider"), Tab("Collider")]
        [SerializeField] private Pseudo3DCollider pseudo3DCollider;
        
        /// <summary>
        /// Is the collision for this vehicle enabled?
        /// </summary>
        [HideLabel]
        [Group("Collider"), Tab("Collider")]
        [SerializeField] protected bool collisionIsEnabled;
        
        /// <summary>
        /// The visible vehicle properties.
        /// </summary>
        [HideLabel]
        [Group("Collider"), Tab("Collider")]
        [SerializeField] private float customSpriteScale = 1f;

        // Properties
        public Dictionary<int, Rect> ColliderBuffer { get; set; }
        public Pseudo3DCollider Pseudo3DCollider => pseudo3DCollider;
        public VehicleState VehicleState => vehicleState;
        public VehicleProperties VehicleProperties => vehicleProperties;
        public ColliderType ColliderType => pseudo3DCollider.ColliderType;
        private LoadedWorld LoadedWorld => GameController.I.LoadedWorld;
        public List<Sprite> CurrentSprites { get; }
        public float CustomSpriteScale => customSpriteScale;
        public bool Paused
        {
            get => paused;
            set => paused = value;
        }
        public bool Enabled
        {
            get => isEnabled;
            set => isEnabled = value;
        }
        public bool CollisionIsEnabled
        {
            get => collisionIsEnabled;
            set => collisionIsEnabled = value;
        }

        /// <summary>
        /// Instantiate a new road vehicle.
        /// </summary>
        /// <param name="vehicleState">The initial vehicle state.</param>
        /// <param name="levelVehicle">Data that describes the properties of this vehicle.</param>
        public RoadVehicle(VehicleState vehicleState, LevelVehicle levelVehicle)
        {
            this.vehicleState = vehicleState;
            pseudo3DCollider = levelVehicle.pseudo3DCollider;
            ColliderBuffer = new Dictionary<int, Rect>();
            CollisionIsEnabled = true;
            CurrentSprites = new List<Sprite> {levelVehicle.sprite};
        }

        public void HandleUpdate()
        {
            if (Paused) 
                return;
            
            vehicleState.trackPosition += vehicleState.trackSpeed * Time.fixedDeltaTime;
            
            // Loop track.
            int trackTrip = LoadedWorld.Length * LoadedWorld.SegmentLength;
            while (vehicleState.trackPosition >= trackTrip) 
                vehicleState.trackPosition -= trackTrip;
            
            while (vehicleState.trackPosition < 0)
                vehicleState.trackPosition += trackTrip;
        }
    }
}