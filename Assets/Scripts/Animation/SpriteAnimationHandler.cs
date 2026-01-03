using System.Collections.Generic;
using UnityEngine;

namespace Animation
{
    /// <summary>
    /// Handles image-sequence animations on sprite-based game objects.
    /// </summary>
    public class SpriteAnimationHandler : AnimationHandler
    {
        /// <summary>
        /// The target renderer.
        /// </summary>
        private readonly SpriteRenderer _spriteRenderer;
        
        // Private members.
        private Sprite _currentSprite;

        /// <summary>
        /// Instantiate a new sprite animator.
        /// </summary>
        /// <param name="frames">The frame sprites.</param>
        /// <param name="spriteRenderer">The target renderer.</param>
        /// <param name="frameRate">The frame rate in seconds.</param>
        public SpriteAnimationHandler(List<Sprite> frames, SpriteRenderer spriteRenderer, float frameRate = 0.16f) :
            base(frames, frameRate)
        {
            _spriteRenderer = spriteRenderer;
        }

        /// <summary>
        /// Assign the current frame sprite to the sprite renderer.
        /// </summary>
        /// <param name="sprite">The current frame sprite.</param>
        protected override void SetCurrentSprite(Sprite sprite)
        {
            _currentSprite = sprite;
            _spriteRenderer.sprite = _currentSprite;
        }

        /// <summary>
        /// Get the current sprite of the animation.
        /// </summary>
        public Sprite GetCurrentSprite()
        {
            return _currentSprite;
        }
    }
}
