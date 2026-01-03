// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Linq;
using TriInspector;
using UnityEngine;

namespace World
{
    /// <summary>
    /// Tracks are composed by a collection of segments.
    /// This structure defines the properties of a single segment.
    /// </summary>
    [Serializable]
    [DeclareTabGroup("Line")]
    public struct TrackSegment
    {
        /// <summary>
        /// World position of the segment line.
        /// </summary>
        [HideLabel]
        [Group("Line"), Tab("Line")]
        public WorldLine WorldLine;
        
        /// <summary>
        /// The curve factor in the range [-1, 1].
        /// </summary>
        public float Curve;
        
        /// <summary>
        /// Preview property.
        /// </summary>
        [ShowInInspector] [PreviewObject] [ListDrawerSettings(Draggable = false, AlwaysExpanded = true, HideAddButton = true, HideRemoveButton = true)]
        public List<Sprite> ObjectPreviews => segmentObjects.Select(o => o.Preview).ToList();
        
        /// <summary>
        /// The segment objects. Contains definition and collider buffer.
        /// </summary>
        public List<StaticWorldObject> segmentObjects;

        public void HandleUpdate()
        {
            if (segmentObjects == null)
                return;
            
            foreach (StaticWorldObject segmentObject in segmentObjects) 
                segmentObject.HandleUpdate();
        }
    }
}