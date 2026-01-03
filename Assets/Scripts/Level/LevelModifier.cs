using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;
using World;

namespace Level
{
    /// <summary>
    /// Modifiable track properties. 
    /// </summary>
    [Serializable]
    public class LevelModifier
    {
        /// <summary>
        /// A label for visual ease on the inspector.
        /// </summary>
        public string label;

        /// <summary>
        /// The type of level modifier.
        /// </summary>
        public LevelModifierType levelModifierType = LevelModifierType.Any;

        /// <summary>
        /// The segment interval where this modifier will act.
        /// </summary>
        [FormerlySerializedAs("Segments")] public Vector2Int segments;
        
        /// <summary>
        /// The curve distortion applied to the segments.
        /// </summary>
        [ShowIf("IsRoadProperty")]
        public float curve;
        
        /// <summary>
        /// The height distortion applied to the segments.
        /// </summary>
        [ShowIf("IsRoadProperty")]
        public float h;
        
        [ShowIf("IsRoadProperty")]
        public float oscillationNodes;
        
        /// <summary>
        /// The frequency in which this modifier is applied. 1 to modify every segment,
        /// 2 for every two segments and so on...
        /// </summary>
        public int frequency = 1;
        
        /// <summary>
        /// Preview property.
        /// </summary>
        [ShowIf("IsWorldObject")] [ShowInInspector] [PreviewObject] [ListDrawerSettings(Draggable = false, AlwaysExpanded = true, HideAddButton = true, HideRemoveButton = true)]
        public List<Sprite> ObjectPreviews => worldObjects?.Select(o => o.Preview).ToList();

        // Properties
        public bool IsRoadProperty => levelModifierType is LevelModifierType.RoadProperty or LevelModifierType.Any;
        public bool IsWorldObject => levelModifierType is LevelModifierType.WorldObject or LevelModifierType.Any;
        
        /// <summary>
        /// A list with the world objects to insert in the segments.
        /// </summary>
        [ShowIf("IsWorldObject")]
        public List<StaticWorldObject> worldObjects;
    }
}