using System.Collections.Generic;
using System.Linq;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Level
{
    /// <summary>
    /// Defines a racing track level.
    /// </summary>
    [CreateAssetMenu(fileName = "New Level", menuName = "Racer/Level")]
    [DeclareTabGroup("Road properties")]
    [DeclareTabGroup("World objects")]
    [DeclareTabGroup("Vehicles")]
    [DeclareTabGroup("Tools")]
    public class LevelData : ScriptableObject
    {
        [FormerlySerializedAs("length")]
        [Title("Level properties")]
        [SerializeField] [Tooltip("Amount of segments.")] private int segments = 1600;
        [SerializeField] [Tooltip("Road width in world coordinates.")] private float roadWidth;
        [SerializeField] [Tooltip("Segment length in world coordinates.")] private int segmentLength;
        [SerializeField] [Tooltip("Maximum track height in world coordinates.")] private float trackHeight;
        
        [FormerlySerializedAs("levelModifiers")]
        [Title("Level configuration")]
        [FormerlySerializedAs("modifiers")]
        [FormerlySerializedAs("Modifier")]
        
        [HideLabel]
        [Group("Road properties"), Tab("Road properties")]
        [SerializeField] private List<LevelModifier> roadPropertiesModifiers;
        
        [HideLabel]
        [Group("World objects"), Tab("World objects")]
        [SerializeField] private List<LevelModifier> worldObjectsModifiers;
        
        [HideLabel]
        [Group("Vehicles"), Tab("Vehicles")]
        [SerializeField] private List<LevelVehicle> levelVehicles;

        [Group("Tools"), Tab("Tools")]
        [SerializeField] private int displacement;

        [Button("Apply displacement")]
        private void ApplyDisplacement()
        {
            foreach (LevelModifier m in RoadPropertiesModifiers)
                m.segments += new Vector2Int(displacement, displacement);
            
            foreach (LevelModifier m in WorldObjectsModifiers)
                m.segments += new Vector2Int(displacement, displacement);
        }
        
        /// <summary>
        /// The amount of segments that the track contains.
        /// </summary>
        public int Segments => segments;

        /// <summary>
        /// Modifier for road segments.
        /// </summary>
        public List<LevelModifier> LevelModifiers => RoadPropertiesModifiers.Concat(WorldObjectsModifiers).ToList();

        /// <summary>
        /// Traffic vehicles that populate the road.
        /// </summary>
        public List<LevelVehicle> LevelVehicles => levelVehicles;
        
        /// <summary>
        /// Road width in world coordinates.
        /// </summary>
        public float RoadWidth => roadWidth;
        
        /// <summary>
        /// Maximum track height in world coordinates.
        /// </summary>
        public float TrackHeight => trackHeight;
        
        /// <summary>
        /// Segment length in world coordinates.
        /// </summary>
        public int SegmentLength => segmentLength;

        public List<LevelModifier> RoadPropertiesModifiers
        {
            get => roadPropertiesModifiers;
            set => roadPropertiesModifiers = value;
        }

        public List<LevelModifier> WorldObjectsModifiers
        {
            get => worldObjectsModifiers;
            set => worldObjectsModifiers = value;
        }
    }
}