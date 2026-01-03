using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Animation
{
    /// <summary>
    /// Handles image-sequence animations on image-based UI game objects.
    /// </summary>
    public class ImageAnimationHandler : AnimationHandler
    {
        /// <summary>
        /// The target renderer.
        /// </summary>
        private readonly Image _image;
        
        /// <summary>
        /// Instantiate a new image animation handler.
        /// </summary>
        /// <param name="frames"></param>
        /// <param name="image"></param>
        /// <param name="frameRate"></param>
        public ImageAnimationHandler(List<Sprite> frames, Image image, float frameRate = 0.16f) 
            : base(frames, frameRate)
        {
            _image = image;
        }

        /// <summary>
        /// Assign the current frame sprite to the image object.
        /// </summary>
        /// <param name="sprite">The current frame sprite.</param>
        protected override void SetCurrentSprite(Sprite sprite)
        {
            _image.sprite = sprite;
        }
    }
}