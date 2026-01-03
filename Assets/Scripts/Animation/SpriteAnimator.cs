using System.Collections.Generic;
using TriInspector;

using UnityEngine;
using World;

namespace Animation
{
    /// <summary>
    /// Animator by frames for a sprite renderer.
    /// </summary>
    public class SpriteAnimator : MonoBehaviour, IVisible
    {
        [Title("References")]
        [SerializeField] [Required] SpriteRenderer spriteRenderer;
        
        [InfoBox("Optional reference to play/stop an audio source alongside the animation.")] 
        [SerializeField] AudioSource audioSource;
        
        [Title("Parameters")]
        [SerializeField] bool autostart;
        [SerializeField] bool loop;
        [SerializeField] bool disableOnFinish;
        [SerializeField] List<Sprite> frames;
        [SerializeField] float frameRate = 0.16f;
        
        // Properties
        public Sprite CurrentSprite => spriteRenderer.sprite;
        public bool Enabled => spriteRenderer.enabled;
        
        // Private members.
        private bool _isPlaying;
        private SpriteAnimationHandler _animator;
        
        #region Monobehavior entry points
        
        private void Awake()
        {
            _animator = new SpriteAnimationHandler(frames, spriteRenderer, frameRate);
            if (autostart)
                Play();
            else
                Stop();
        }

        private void FixedUpdate()
        {
            if (_isPlaying && _animator.HandleUpdate() && !loop)
            {
                _isPlaying = false;
                if (disableOnFinish)
                    spriteRenderer.enabled = false;
            }
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
                spriteRenderer.enabled = true;
                _animator.StartAnimation();

                if (audioSource)
                    audioSource.Play();
            }
        }

        /// <summary>
        /// Stop the animation.
        /// </summary>
        public void Stop()
        {
            _isPlaying = false;
            spriteRenderer.enabled = false;
            
            if (audioSource)
                audioSource.Stop();
        }
        
        #endregion
    }
}