// ReSharper disable MemberInitializerValueIgnored
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Animation;
using TriInspector;
using UnityEngine;

namespace World
{
    /// <summary>
    /// A visible element can be a static sprite or an animation composed by
    /// a sequence of sprites.
    /// </summary>
    [Serializable]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class VisibleElement : IVisible
    {
        /// <summary>
        /// The type of the visible element.
        /// </summary>
        [SerializeField] VisibleElementType visibleElementType;
        
        /// <summary>
        /// The sprite used if type is static.
        /// </summary>
        [ShowIf("IsStatic")] [PreviewObject] [SerializeField] Sprite sprite;
        
        /// <summary>
        /// The sprite sequence used if the type is an animation.
        /// </summary>
        [ShowIf("IsAnimated")] [PreviewObject] [SerializeField] List<Sprite> spriteSequence;
        
        /// <summary>
        /// The frame rate of the animation if applicable.
        /// </summary>
        [ShowIf("IsAnimated")] [SerializeField] float frameRate = 0.16f;
        
        // Properties
        public Sprite CurrentSprite => visibleElementType switch
        {
            VisibleElementType.Static => sprite,
            VisibleElementType.Animated => _animationHandler.CurrentFrame,
            _ => throw new ArgumentOutOfRangeException()
        };
        public Sprite Preview => visibleElementType switch
        {
            VisibleElementType.Static => sprite,
            VisibleElementType.Animated => spriteSequence is { Count: > 0 } ? spriteSequence.First() : sprite,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        private bool IsStatic => (visibleElementType == VisibleElementType.Static);
        private bool IsAnimated => (visibleElementType == VisibleElementType.Animated);
        
        // Private fields.
        private GenericAnimationHandler _animationHandler;

        public VisibleElement(Sprite sprite, float customSpriteScale)
        {
            visibleElementType = VisibleElementType.Static;
            frameRate = 0f;
            
            this.sprite = sprite;

            Initialize();
        }

        public VisibleElement(List<Sprite> spriteSequence, float customSpriteScale, float frameRate)
        {
            visibleElementType = VisibleElementType.Animated;
            this.frameRate = frameRate;
            this.spriteSequence = spriteSequence;
            
            Initialize();
        }

        public void Initialize()
        {
            _animationHandler = visibleElementType == VisibleElementType.Animated ? 
                new GenericAnimationHandler(spriteSequence, frameRate) : 
                null;
        }

        public void HandleUpdate()
        {
            _animationHandler?.HandleUpdate();
        }
    }
}