using System.Collections.Generic;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Animation
{
    /// <summary>
    /// Animator by frames for a image.
    /// </summary>
    public class ImageAnimator : MonoBehaviour
    {
        [Title("References")] 
        [SerializeField] [Required] private Image image;
        
        [Title("Parameters")]
        [SerializeField] bool autostart;
        [SerializeField] bool loop;
        [SerializeField] bool disableImageWhenStopped = true;
        [SerializeField] [PreviewObject] List<Sprite> frames;
        [SerializeField] float frameRate = 0.16f;
        
        // Properties
        public Sprite CurrentSprite => image.sprite;
        public bool Enabled => image.enabled;
        
        // Private members.
        private bool _isPlaying;
        private ImageAnimationHandler _animator;
        
        #region Monobehavior entry points

        private void Awake()
        {
            _animator = new ImageAnimationHandler(frames, image, frameRate);
            if (autostart)
                Play();
            else
                Stop();
        }
        
        private void FixedUpdate()
        {
            if (_isPlaying && _animator.HandleUpdate() && !loop)
                _isPlaying = false;
        }

        #endregion
        
        #region Public API

        /// <summary>
        /// Play the animation.
        /// </summary>
        public void Play()
        {
            if (!_isPlaying)
            {
                _isPlaying = true;
                image.enabled = true;
                _animator.StartAnimation();
            }
        }

        /// <summary>
        /// Stop the animation.
        /// </summary>
        public void Stop()
        {
            _isPlaying = false;
            if (disableImageWhenStopped)
                image.enabled = false;
        }
        
        #endregion

    }
}