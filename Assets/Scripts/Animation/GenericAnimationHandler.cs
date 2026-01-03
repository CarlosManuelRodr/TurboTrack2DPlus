using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Animation
{
    /// <summary>
    /// A generic animation handler that updates the frame into the public property CurrentFrame.
    /// </summary>
    public class GenericAnimationHandler : AnimationHandler
    {
        /// <summary>
        /// The current frame of the animated item.
        /// </summary>
        public Sprite CurrentFrame { get; private set; }
        
        /// <summary>
        /// Instantiate a new GenericAnimationHandler.
        /// </summary>
        /// <param name="frames">The animation frames.</param>
        /// <param name="frameRate">The animation frame rate.</param>
        public GenericAnimationHandler(List<Sprite> frames, float frameRate) 
            : base(frames, frameRate)
        {
            CurrentFrame = (frames != null && frames.Count != 0) ? frames.First() : null;
        }

        /// <summary>
        /// Assign the value of CurrentFrame.
        /// </summary>
        protected override void SetCurrentSprite(Sprite sprite)
        {
            CurrentFrame = sprite;
        }
    }
}