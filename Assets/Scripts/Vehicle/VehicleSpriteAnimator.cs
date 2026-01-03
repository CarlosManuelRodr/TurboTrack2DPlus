using System;
using System.Collections.Generic;
using Animation;
using TriInspector;
using UnityEngine;
using World;

namespace Vehicle
{
    /// <summary>
    /// Animator for the directional movement of a car.
    /// </summary>
    public class VehicleSpriteAnimator : MonoBehaviour, IVisible
    {
        [Title("Parameters")]
        [SerializeField] [Required] private SpriteRenderer spriteRenderer;
        
        [Title("Parameters")]
        [SerializeField] private float frameRate = 0.16f;
        
        [Title("Animation data")] 
        [PreviewObject] [SerializeField] private Sprite idleSprite;
        [PreviewObject] [SerializeField] private List<Sprite> moveLeftSprites;
        [PreviewObject] [SerializeField] private List<Sprite> moveRightSprites;
        
        // Properties
        public Sprite CurrentSprite => spriteRenderer.sprite;
        public bool Enabled => spriteRenderer.enabled;
        
        // States
        private VehicleDirection _currentVehicleDirection = VehicleDirection.None;
        private bool _animationComplete;
        private SpriteAnimationHandler _currentAnimationHandler;
        private SpriteAnimationHandler _idleAnimationHandler;
        private SpriteAnimationHandler _moveLeftAnimationHandler;
        private SpriteAnimationHandler _moveRightAnimationHandler;
        
        private void Start()
        {
            _idleAnimationHandler = new SpriteAnimationHandler(new List<Sprite> { idleSprite }, spriteRenderer, frameRate);
            _moveLeftAnimationHandler = new SpriteAnimationHandler(moveLeftSprites, spriteRenderer, frameRate);
            _moveRightAnimationHandler = new SpriteAnimationHandler(moveRightSprites, spriteRenderer, frameRate);
            SetVehicleDirection(VehicleDirection.Idle);
        }

        private void FixedUpdate()
        {
            if (!_animationComplete && _currentAnimationHandler != null)
                _animationComplete = _currentAnimationHandler.HandleUpdate();
        }

        /// <summary>
        /// Set and start a new animation if the animationState is different from the currentAnimationState.
        /// </summary>
        /// <param name="vehicleDirection">The new animation state.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetVehicleDirection(VehicleDirection vehicleDirection)
        {
            if (_currentVehicleDirection == vehicleDirection)
                return;

            _animationComplete = false;
            _currentVehicleDirection = vehicleDirection;
            _currentAnimationHandler = _currentVehicleDirection switch
            {
                VehicleDirection.Idle => _idleAnimationHandler,
                VehicleDirection.TurnLeft => _moveLeftAnimationHandler,
                VehicleDirection.TurnRight => _moveRightAnimationHandler,
                _ => null
            };
            _currentAnimationHandler?.StartAnimation();
        }
    }
}