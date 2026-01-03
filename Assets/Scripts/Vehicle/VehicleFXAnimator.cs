using System.Collections.Generic;
using Animation;
using TriInspector;
using UnityEngine;
using World;

namespace Vehicle
{
    /// <summary>
    /// Controller for animations that follow the vehicle direction.
    /// </summary>
    public class VehicleFXAnimator : MonoBehaviour, IVisible
    {
        private const int TurningStates = 3;
        
        [Title("References")]
        [SerializeField] [Required] private SpriteRenderer spriteRenderer;
        
        [InfoBox("Optional reference to play/stop an audio source alongside the animation.")] 
        [SerializeField] AudioSource audioSource;
        
        [Title("Parameters")]
        [SerializeField] private float animationsFrameRate = 0.16f;
        [SerializeField] private float turnFrameRate = 0.16f;
        
        [Title("Animation data")] 
        [PreviewObject] [SerializeField] private List<Sprite> idleSprites;
        [PreviewObject] [SerializeField] private List<Sprite> moveLeftSmallSprites;
        [PreviewObject] [SerializeField] private List<Sprite> moveLeftMildSprites;
        [PreviewObject] [SerializeField] private List<Sprite> moveLeftFullSprites;
        [PreviewObject] [SerializeField] private List<Sprite> moveRightSmallSprites;
        [PreviewObject] [SerializeField] private List<Sprite> moveRightMildSprites;
        [PreviewObject] [SerializeField] private List<Sprite> moveRightFullSprites;

        // Properties
        public Sprite CurrentSprite => spriteRenderer.sprite;
        public bool Enabled => spriteRenderer.enabled;
        
        // Private members.
        private VehicleDirection _currentVehicleDirection = VehicleDirection.None;
        private SpriteAnimationHandler _currentAnimationHandler;
        private SpriteAnimationHandler _idleAnimationHandler;
        private SpriteAnimationHandler _moveLeftSmallAnimationHandler;
        private SpriteAnimationHandler _moveLeftMildAnimationHandler;
        private SpriteAnimationHandler _moveLeftFullAnimationHandler;
        private SpriteAnimationHandler _moveRightSmallAnimationHandler;
        private SpriteAnimationHandler _moveRightMildAnimationHandler;
        private SpriteAnimationHandler _moveRightFullAnimationHandler;
        
        private int _currentTurnState;
        private float _timer;
        
        private void Start()
        {
            _idleAnimationHandler = new SpriteAnimationHandler(idleSprites, spriteRenderer, animationsFrameRate);
            _moveLeftSmallAnimationHandler = new SpriteAnimationHandler(moveLeftSmallSprites, spriteRenderer, animationsFrameRate);
            _moveLeftMildAnimationHandler = new SpriteAnimationHandler(moveLeftMildSprites, spriteRenderer, animationsFrameRate);
            _moveLeftFullAnimationHandler = new SpriteAnimationHandler(moveLeftFullSprites, spriteRenderer, animationsFrameRate);
            
            _moveRightSmallAnimationHandler = new SpriteAnimationHandler(moveRightSmallSprites, spriteRenderer, animationsFrameRate);
            _moveRightMildAnimationHandler = new SpriteAnimationHandler(moveRightMildSprites, spriteRenderer, animationsFrameRate);
            _moveRightFullAnimationHandler = new SpriteAnimationHandler(moveRightFullSprites, spriteRenderer, animationsFrameRate);
            
            SetVehicleDirection(VehicleDirection.Idle);
        }

        private void FixedUpdate()
        {
            if (_currentVehicleDirection != VehicleDirection.Idle)
            {
                _timer += Time.deltaTime;
                if ((_currentTurnState + 1) != TurningStates && _timer > turnFrameRate)
                {
                    _currentTurnState++;

                    if (_currentVehicleDirection == VehicleDirection.TurnLeft)
                    {
                        _currentAnimationHandler = _currentTurnState switch
                        {
                            0 => _moveLeftSmallAnimationHandler,
                            1 => _moveLeftMildAnimationHandler,
                            2 => _moveLeftFullAnimationHandler,
                            _ => _currentAnimationHandler
                        };
                    }
                    else if (_currentVehicleDirection == VehicleDirection.TurnRight)
                    {
                        _currentAnimationHandler = _currentTurnState switch
                        {
                            0 => _moveRightSmallAnimationHandler,
                            1 => _moveRightMildAnimationHandler,
                            2 => _moveRightFullAnimationHandler,
                            _ => _currentAnimationHandler
                        };
                    }

                    _timer -= turnFrameRate;
                    _currentAnimationHandler?.StartAnimation();
                }
            }

            _currentAnimationHandler?.HandleUpdate();
        }

        /// <summary>
        /// Set the visibility of this object's sprite renderer.
        /// </summary>
        /// <param name="value">Visibility value.</param>
        public void SetVisible(bool value)
        {
            spriteRenderer.enabled = value;
            
            if (!audioSource) 
                return;
            
            if (value)
                audioSource.Play();
            else
                audioSource.Stop();
        }
        
        /// <summary>
        /// Set the tracked vehicle direction to modify the animation handler.
        /// </summary>
        /// <param name="vehicleDirection">The direction.</param>
        public void SetVehicleDirection(VehicleDirection vehicleDirection)
        {
            if (_currentVehicleDirection == vehicleDirection)
                return;
            
            _currentVehicleDirection = vehicleDirection;
            _currentTurnState = 0;
            _timer = 0f;

            _currentAnimationHandler = vehicleDirection switch
            {
                VehicleDirection.TurnLeft => _moveLeftSmallAnimationHandler,
                VehicleDirection.TurnRight => _moveRightSmallAnimationHandler,
                _ => _idleAnimationHandler
            };
            
            _currentAnimationHandler?.StartAnimation();
        }
    }
}