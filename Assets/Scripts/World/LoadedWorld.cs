using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Extensions;
using Physics;
using UnityEngine;
using Vehicle;
using Level;
using Random = UnityEngine.Random;

namespace World
{
    /// <summary>
    /// In-memory representation of a loaded level.
    /// </summary>
    [Serializable]
    public class LoadedWorld
    {
        // Backing fields visible on the inspector.
        [SerializeField] PlayerVehicle[] playerVehicles;
        [SerializeField] RoadVehicle[] roadVehicles;
        [SerializeField] TrackSegment[] segments;
        [SerializeField] float roadWidth;
        [SerializeField] int segmentLength;

        // Properties
        public TrackSegment[] Segments => segments;
        public int Length => Segments.Length;
        public float RoadWidth => roadWidth;
        public int SegmentLength => segmentLength;
        public IVehicle[] Vehicles => playerVehicles.Cast<IVehicle>().Concat(roadVehicles).ToArray();
        public RoadVehicle[] RoadVehicles => roadVehicles;
        public PlayerVehicle[] PlayerVehicles => playerVehicles;
        
        /// <summary>
        /// Instantiate a track from the level configuration.
        /// </summary>
        /// <param name="levelData">The level configuration.</param>
        /// <param name="playerVehicles"></param>
        public LoadedWorld(LevelData levelData, PlayerVehicle[] playerVehicles)
        {
            GameLogger.Info("Loading world from level.", GetType());
            
            // Load world.
            roadWidth = levelData.RoadWidth;
            segmentLength = levelData.SegmentLength;
            segments = new TrackSegment[levelData.Segments];
            this.playerVehicles = playerVehicles;

            // Iterate through all the track segments.
            for (int i = 0; i < levelData.Segments; i++)
            {
                ref TrackSegment trackSegment = ref Segments[i];
                trackSegment.WorldLine.Z = i * levelData.SegmentLength;
                trackSegment.WorldLine.W = levelData.RoadWidth;
                trackSegment.segmentObjects = new List<StaticWorldObject>();

                // Apply level modifiers that are in the segment range.
                foreach (LevelModifier m in levelData.LevelModifiers)
                {
                    if (m.segments.InRange(i) && i % m.frequency == 0)
                    {
                        // Modify geometry.
                        trackSegment.Curve += m.curve;
                        trackSegment.WorldLine.Y += Mathf.Sin(((m.oscillationNodes * Mathf.PI) / (m.segments.y - m.segments.x)) * (i - m.segments.x)) * levelData.TrackHeight;

                        // Add objects to segment.
                        if (m.worldObjects.Count > 0)
                        {
                            List<StaticWorldObject> modifierObjects = m.worldObjects.Select(obj => new StaticWorldObject(obj)).ToList();
                            trackSegment.segmentObjects.AddRange(modifierObjects);
                        }
                    }
                }
            }
            
            // Load road vehicles.
            int currentVehicle = 0;
            int totalRoadVehicles = levelData.LevelVehicles.Sum(v => v.amount);
            roadVehicles = new RoadVehicle[totalRoadVehicles];
            foreach (LevelVehicle levelVehicle in levelData.LevelVehicles)
            {
                for (int i = 0; i < levelVehicle.amount; i++)
                    roadVehicles[currentVehicle++] = new RoadVehicle(CreateRandomState(levelVehicle), levelVehicle);
            }
        }

        public void HandleUpdate()
        {
            foreach (TrackSegment trackSegment in segments) 
                trackSegment.HandleUpdate();
        }

        /// <summary>
        /// Set all collectibles as enabled.
        /// </summary>
        public void ResetCollectibles()
        {
            GameLogger.Info("Resetting collectibles", GetType());
            
            foreach (TrackSegment trackSegment in segments)
            {
                foreach (StaticWorldObject segmentObject in trackSegment.segmentObjects)
                {
                    if (segmentObject.Pseudo3DCollider.ColliderType == ColliderType.Collectible)
                        segmentObject.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Initialize a random road vehicle state.
        /// </summary>
        /// <param name="vehicleSpecs">The vehicle properties.</param>
        /// <returns>A random vehicle state.</returns>
        private VehicleState CreateRandomState(LevelVehicle vehicleSpecs)
        {
            VehicleState randomVehicleState;
            randomVehicleState.horizontalPosition = Random.Range(-0.25f, 0.25f);
            randomVehicleState.horizontalSpeed = 0f;
            randomVehicleState.trackSpeed = vehicleSpecs.speed;
            randomVehicleState.trackPosition = Random.Range(0f, Length * SegmentLength);
            return randomVehicleState;
        }
    }
}