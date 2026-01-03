using System.Collections.Generic;
using UnityEngine;

namespace Animation
{
    /// <summary>
    /// Handles animations composed by a sequence of sprites. Used as simpler replacement for
    /// Unity's complex built-in animation system.
    /// </summary>
    public abstract class AnimationHandler
    {
        private readonly List<Sprite> _frames;
        private readonly float _frameRate;

        private int _currentFrame;
        private float _timer;

        /// <summary>
        /// Instantiate a new animator handler.
        /// </summary>
        /// <param name="frames">The frame sprites.</param>
        /// <param name="frameRate">The frame rate in seconds.</param>
        protected AnimationHandler(List<Sprite> frames, float frameRate = 0.16f)
        {
            _frames = frames;
            _frameRate = frameRate;
        }

        /// <summary>
        /// Assign the current frame sprite.
        /// </summary>
        /// <param name="sprite">The current frame sprite.</param>
        protected abstract void SetCurrentSprite(Sprite sprite);
        
        /// <summary>
        /// Reset animation state and start it.
        /// </summary>
        public void StartAnimation()
        {
            _currentFrame = 0;
            _timer = 0f;
            
            if (_frames.Count > 0)
                SetCurrentSprite(_frames[0]);
        }

        /// <summary>
        /// Handle frame update.
        /// </summary>
        /// <returns>True if the animation has reached the last frame. False otherwise.</returns>
        public bool HandleUpdate()
        {
            if (_frames.Count <= 1)
                return false;
            
            _timer += Time.deltaTime;
            if (_timer > _frameRate)
            {
                _currentFrame = (_currentFrame + 1) % _frames.Count;
                SetCurrentSprite( _frames[_currentFrame]);
                _timer -= _frameRate;
                return (_currentFrame + 1 == _frames.Count);
            }

            return false;
        }

    }
}