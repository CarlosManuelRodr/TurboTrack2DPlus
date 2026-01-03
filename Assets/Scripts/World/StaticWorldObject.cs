using System;
using System.Collections.Generic;
using Physics;
using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace World
{
    /// <summary>
    /// An object placed in the world that has a fixed position.
    /// </summary>
    [Serializable]
    [DeclareTabGroup("State")]
    [DeclareTabGroup("Visible")]
    [DeclareTabGroup("Collider")]
    public class StaticWorldObject : IWorldObject
    {
        public const float CollectibleReappearSeconds = 10f;
        
        /// <summary>
        /// The sprite x position.
        /// </summary>
        [Group("State"), Tab("State")]
        [FormerlySerializedAs("spriteX")] [SerializeField] private float x;

        /// <summary>
        /// Random variation amplitude for the x coordinate of the sprite.
        /// </summary>
        [Group("State"), Tab("State")]
        [FormerlySerializedAs("spriteXRandomVariation")] [SerializeField] private float xRandomVariation;
        
        /// <summary>
        /// Flag to reflect the object horizontally.
        /// </summary>
        [Group("State"), Tab("State")]
        [SerializeField] private bool flipX;
        
        /// <summary>
        /// Is this object enabled to appear on the world?
        /// </summary>
        [Group("State"), Tab("State")]
        [SerializeField] private bool enabled = true;
        
        /// <summary>
        /// Custom scale for this object.
        /// </summary>
        [Group("State"), Tab("State")]
        [SerializeField] private float customSpriteScale = 1f;
        
        /// <summary>
        /// The object sprite.
        /// </summary>
        [Group("Visible"), Tab("Visible")]
        [HideLabel]
        [SerializeField] private VisibleElement visibleElement;
        
        /// <summary>
        /// The collider properties of the object.
        /// </summary>
        [Group("Collider"), Tab("Collider")]
        [HideLabel]
        [SerializeField] private Pseudo3DCollider pseudo3DCollider;
        
        // Properties
        public Sprite Preview => visibleElement.Preview;
        public float X => x + _randomVariationX;
        public bool FlipX => flipX;
        public Pseudo3DCollider Pseudo3DCollider => pseudo3DCollider;
        public bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }

        public Sprite CurrentSprite => visibleElement.CurrentSprite;

        public float CustomSpriteScale
        {
            get => customSpriteScale;
            set => customSpriteScale = value;
        }

        public Dictionary<int, Rect> ColliderBuffer
        {
            get => _colliderBuffer;
            set => _colliderBuffer = value;
        }
        
        // Private members
        private Dictionary<int, Rect> _colliderBuffer;
        private float _randomVariationX;
        private float _collectedTime;

        public StaticWorldObject() {}

        /// <summary>
        /// Copy constructor for a world object.
        /// </summary>
        /// <param name="other">The reference world object.</param>
        public StaticWorldObject(StaticWorldObject other)
        {
            x = other.x;
            xRandomVariation = other.xRandomVariation;
            flipX = other.flipX;
            visibleElement = other.visibleElement;
            pseudo3DCollider = other.pseudo3DCollider;
            customSpriteScale = other.customSpriteScale;
            enabled = other.enabled;
            
            _colliderBuffer = new Dictionary<int, Rect>();
            _randomVariationX = Random.Range(-xRandomVariation, xRandomVariation);
            
            visibleElement.Initialize();
        }

        public void HandleUpdate()
        {
            if (enabled)
                visibleElement.HandleUpdate();
            else if (Time.time - _collectedTime > CollectibleReappearSeconds)
                Enabled = true;
        }

        public void Collect()
        {
            if (pseudo3DCollider.ColliderType != ColliderType.Collectible)
                throw new Exception("Cannot call Collect() on a non-collective World object.");

            Enabled = false;
            _collectedTime = Time.time;
        }
    }
}