using Physics;
using TriInspector;
using UnityEngine;
using World;

namespace Vehicle
{
    [DeclareTabGroup("NPC Options")]
    public class NpcVehicle : PlayerVehicle
    {
        [Group("NPC Options"), Tab("NPC Options")]
        [SerializeField] private int boostDistanceFromCollectible = 30000;
        
        [Group("NPC Options"), Tab("NPC Options")]
        [SerializeField] private float aiHorizontalSpeed = 1f;
        
        public override void HandleInput() {}

        public override void HandleUpdate()
        {
            HandleCollisions();

            // Handle horizontal movement.
            (float trackPosition, float horizontalPosition)? nextCollectiblePosition = GetNextCollectiblePosition();
            if (nextCollectiblePosition != null)
            {
                vehicleState.horizontalPosition = Mathf.MoveTowards(vehicleState.horizontalPosition, 
                                                                     nextCollectiblePosition.Value.horizontalPosition, 
                                                                     aiHorizontalSpeed * Time.deltaTime);
                
                if (Mathf.Abs(nextCollectiblePosition.Value.trackPosition - VehicleState.trackPosition) <
                    boostDistanceFromCollectible)
                    SetGearHigh();
                else
                    SetGearLow();

            }
            
            CurrentAcceleration = CurrentGearParameters.acceleration;

            HandleVehicleMovement();
        }

        private (float trackPosition, float horizontalPosition)? GetNextCollectiblePosition()
        {
            TrackSegment? searchStartSegment = null;
            foreach (TrackSegment segment in LoadedWorld.Segments)
            {
                if (segment.segmentObjects == null)
                    continue;

                if (positionBetweenFrames.Contains(segment.WorldLine.Z))
                    searchStartSegment = segment;

                if (searchStartSegment != null && segment.segmentObjects.Count > 0)
                {
                    foreach (StaticWorldObject segmentObject in segment.segmentObjects)
                    {
                        if (segmentObject.Pseudo3DCollider.ColliderType == ColliderType.Collectible)
                        {
                            (float trackPosition, float horizontalPosition) result = (segment.WorldLine.Z, segmentObject.X);
                            return result;
                            //return segmentObject.X;
                        }
                    }
                }
            }

            return null;
        }
    }
}