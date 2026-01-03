using System;
using System.Collections;
using System.Collections.Generic;
using Animation;
using Audio;
using Core;
using Core.Types;
using DG.Tweening;
using JetBrains.Annotations;
using Physics;
using TriInspector;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using World;

namespace Vehicle
{
    /// <summary>
    /// Represents the in-game player vehicle and its physics.
    /// </summary>
    [DeclareTabGroup("Collider")]
    [DeclareTabGroup("State")]
    [DeclareTabGroup("Properties")]
    [DeclareTabGroup("Gear Low")]
    [DeclareTabGroup("Gear High")]
    public class PlayerVehicle : MonoBehaviour, IVehicle
    {
        [Title("Rendering references")]
        [SerializeField] protected SpriteRenderer vehicleRenderer;
        [SerializeField] protected SpriteFlasher spriteFlasher;
        [SerializeField] protected VehicleAnimationController vehicleAnimationController;
        
        [Title("Audio references")]
        [SerializeField] protected AudioSource engineAudio;
        [SerializeField] protected AudioSource crashAudio;
        [FormerlySerializedAs("hitAudio")] [SerializeField] protected AudioSource hitStrongAudio;
        [SerializeField] protected AudioSource hitWeakAudio;
        [SerializeField] protected AudioSource coinAudio;
        [SerializeField] protected LoopingSoundController turboSoundController;
        
        [FormerlySerializedAs("turboBar")]
        [Title("UI references")]
        [SerializeField] protected TurboPanel turboPanel;
        [SerializeField] protected SpeedMeter speedMeter;
        [SerializeField] protected CoinCounter coinCounter;
        [SerializeField] protected FilledBar throttleIndicator;

        [Title("Controls")]
        [SerializeField] protected InputAction directionalInput;
        [SerializeField] protected InputAction pedalInput;
        [SerializeField] protected InputAction turboInput;

        [Title("Parameters")]
        [SerializeField] protected bool allowReverse;
        [SerializeField] protected float steeringAmplitude = 2f;
        [SerializeField] protected float centrifugal = 0.1f;
        [SerializeField] protected float horizontalSpeed = 0.1f;
        [SerializeField] protected float turnThresholdSpeed = 3000;
        [SerializeField] protected float fuelCapacity = 100f;
        [SerializeField] protected float initialFuel = 100f;
        [SerializeField] protected float fuelConsumption = 1f;
        [SerializeField] protected float weakImpactSpeedChange = 500f;
        [SerializeField] protected float weakImpactHorizontalSpeedChange = 0.5f;
        [FormerlySerializedAs("impactSpeedChange")] [SerializeField] protected float strongImpactSpeedChange = 1000f;
        
        /// <summary>
        /// The vehicle collider properties.
        /// </summary>
        [SerializeField] protected Pseudo3DCollider pseudo3DCollider;
        
        /// <summary>
        /// Is the collision for this vehicle enabled?
        /// </summary>
        [SerializeField] protected bool collisionIsEnabled;
        
        [SerializeField] protected VehicleState vehicleState;
        
        /// <summary>
        /// Is this object enabled to appear in the world?
        /// </summary>
        [SerializeField] protected bool isEnabled;
        
        /// <summary>
        /// Is this object paused?
        /// </summary>
        [SerializeField] protected bool paused;
        
        /// <summary>
        /// The visible vehicle properties.
        /// </summary>
        [SerializeField] protected float customSpriteScale = 1f;
        
        [SerializeField] protected VehicleProperties vehicleProperties;
        
        [SerializeField] protected GearParameters gearLowParameters;
        [FormerlySerializedAs("gearHightParameters")]
        [SerializeField] protected GearParameters gearHighParameters;
        
        // Properties
        protected static LoadedWorld LoadedWorld => GameController.I != null ? GameController.I.LoadedWorld : null;
        public List<Sprite> CurrentSprites => vehicleAnimationController.CurrentSprites;
        public Sprite VehicleSprite => vehicleAnimationController.VehicleSprite;
        public float CustomSpriteScale => customSpriteScale;
        public VehicleState VehicleState => vehicleState;
        public VehicleProperties VehicleProperties => vehicleProperties;
        public Pseudo3DCollider Pseudo3DCollider => pseudo3DCollider;
        public SpriteRenderer SpriteRenderer => vehicleRenderer;
        public float TurnPercentage => Mathf.Clamp(Mathf.Abs(VehicleSpeed) / turnThresholdSpeed, 0f, 1f);
        public float VehicleSpeed => VehicleState.trackSpeed;
        public bool Paused
        {
            get => paused;
            set => paused = value;
        }
        
        public bool Enabled
        {
            get => isEnabled;
            set => isEnabled = value;
        }
        public bool CollisionIsEnabled
        {
            get => collisionIsEnabled;
            set => collisionIsEnabled = value;
        }
        
        [Title("Debug")]
        [ShowInInspector]
        public int CurrentSegment => LoadedWorld != null ? Mathf.FloorToInt(VehicleState.trackPosition / LoadedWorld.SegmentLength) : 0;

        private int Layer => gameObject.layer;
        
        /// <summary>
        /// Temp field to store the projected collider in world coordinates.
        /// </summary>
        public Dictionary<int, Rect> ColliderBuffer { get; set; }

        /// <summary>
        /// The number of fuel units of the vehicle.
        /// </summary>
        public float Fuel { get; set; }
        
        /// <summary>
        /// The maximum speed achievable on the current gear.
        /// </summary>
        public float CurrentGearMaxSpeed { get; private set; }
        
        /// <summary>
        /// The vehicle gear (low or high).
        /// </summary>
        public Gear Gear { get; private set; }

        /// <summary>
        /// The vehicle current frontal acceleration.
        /// </summary>
        public float CurrentAcceleration { get; protected set; }
        
        /// <summary>
        /// Direction is -1 for left and 1 for right.
        /// </summary>
        public int TurnDirection { get; private set; }
        
        /// <summary>
        /// The number of laps that this vehicle has completed.
        /// </summary>
        private int Laps { get; set; }
        
        /// <summary>
        /// Drifting occurs when the current horizontal movement does not coincide with the intended direction.
        /// </summary>
        private bool IsDrifting => (vehicleState.trackSpeed >= turnThresholdSpeed) && (Math.Sign(TurnDirection) != Math.Sign(vehicleState.horizontalSpeed));

        /// <summary>ChildGameObjectsOnly
        /// Is the vehicle performing a crash sequence?
        /// </summary>
        private bool IsCrashing => _crashCoroutine != null;
        
        /// <summary>
        /// Is the vehicle outside the road bounds?
        /// </summary>
        public bool IsOutsideRoad => (Mathf.Abs(vehicleState.horizontalPosition) > 1.2f);
        
        /// <summary>
        /// Get the parameters of the currently set gear type.
        /// </summary>
        protected GearParameters CurrentGearParameters =>
            Gear switch {
                Gear.Low => gearLowParameters,
                Gear.High => gearHighParameters,
                _ => throw new ArgumentOutOfRangeException(nameof(Gear))
            };
        
        // Subscribable events.
        public UnityAction<PlayerVehicle> onCollectibleCollected;
        public UnityAction<PlayerVehicle> onLapFinished;
        
        // Private members.
        private Coroutine _crashCoroutine;
        private Coroutine _inelasticCollisionCoroutine;
        private Gamepad _thisGamepad;
        private int _collectedCoins;
        
        protected Interval<float> positionBetweenFrames;

        #region Monobehavior entry points

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            SetGear(Gear.Low, true);
            Fuel = initialFuel;
        }

        public virtual void HandleUpdate()
        {
            if (Paused) 
                return;
            
            HandleVehicleMovement();
            HandleCollisions();
            HandleFuel();
            HandleVehicleSteering();
            HandleVehicleFX();
            HandleHUD();
        }

        public void Initialize()
        {
            ColliderBuffer = new Dictionary<int, Rect>();
            CollisionIsEnabled = true;
            vehicleState.trackSpeed = 0f;
            _collectedCoins = 0;
            Laps = 0;
        }
        
        public void OnEnable()
        {
            directionalInput.Enable();
            pedalInput.Enable();
            turboInput.Enable();
        }

        public void OnDisable()
        {
            directionalInput.Disable();
            pedalInput.Disable();
            turboInput.Disable();
        }
        
        #endregion

        #region Simulation handlers

        /// <summary>
        /// Routine that handles a fatal car crash by restoring its position to the
        /// center and blocking control until the sequence finishes.
        /// </summary>
        private IEnumerator FatalCrashRoutine()
        {
            CollisionIsEnabled = false;
            
            Sequence flashSequence = spriteFlasher.PlayFX();
            CurrentAcceleration = 0;
            vehicleState.trackSpeed = 0;
            
            crashAudio.Play();
            vehicleAnimationController.PlayExplosion();
            Tween moveCenter = DOTween.To(() => vehicleState.horizontalPosition, v => vehicleState.horizontalPosition = v, 0, 1f);

            Sequence sequence = DOTween.Sequence();
            sequence.Join(flashSequence);
            sequence.Join(moveCenter);
            
            yield return sequence.WaitForCompletion();
            yield return new WaitWhile (() => crashAudio.isPlaying);
            
            _crashCoroutine = null;
            CollisionIsEnabled = true;
        }

        /// <summary>
        /// Handle a crash where the car explodes.
        /// </summary>
        private void HandleFatalCrash()
        {
            GameLogger.Info("Player vehicle crashed.", GetType());
            _crashCoroutine = StartCoroutine(FatalCrashRoutine());
        }

        /// <summary>
        /// Handle recollection of a road item.
        /// </summary>
        /// <param name="worldObject"></param>
        private void HandleCollectible(IWorldObject worldObject)
        {
            if (worldObject is not StaticWorldObject staticWorldObject)
                throw new Exception("Tried to handle collectible. Wasn't of type StaticWorldObject.");
            
            GameLogger.Info("Player collected collectible.", GetType());
            coinAudio.Play();
            vehicleAnimationController.PlayCollectCoin();

            staticWorldObject.Collect();
            _collectedCoins++;
            coinCounter.SetCoins(_collectedCoins);
            
            onCollectibleCollected.Invoke(this);
        }

        /// <summary>
        /// Handle the physics of an elastic collision.
        /// </summary>
        /// <param name="vehicle">The collided (victim) vehicle</param>
        private void HandleElasticCollision([CanBeNull] IVehicle vehicle = null)
        {
            vehicleAnimationController.PlayHit();
            hitStrongAudio.Play();

            float collidedObjectSpeed = vehicle != null ? vehicle.VehicleState.trackSpeed : 0f;
            vehicleState.trackSpeed = collidedObjectSpeed - strongImpactSpeedChange;
        }
        
        /// <summary>
        /// Handle the physics of an inelastic collision.
        /// </summary>
        /// <param name="vehicle"></param>
        private void HandleInelasticCollision([CanBeNull] IVehicle vehicle = null)
        {
            // Currently, this is just a crash.
            _inelasticCollisionCoroutine ??= StartCoroutine(InelasticCollisionRoutine(vehicle));
        }

        /// <summary>
        /// Crashing animation.
        /// </summary>
        /// <param name="vehicle">The target vehicle.</param>
        private IEnumerator InelasticCollisionRoutine([CanBeNull] IVehicle vehicle = null)
        {
            vehicleAnimationController.PlayHit();
            hitWeakAudio.Play();

            float collidedObjectSpeed = vehicle != null ? vehicle.VehicleState.trackSpeed : 0f;
            float collidedObjectHorizontalSpeed = vehicle != null ? vehicle.VehicleState.horizontalSpeed : 0f;

            vehicleState.trackSpeed = collidedObjectSpeed - weakImpactSpeedChange;
            vehicleState.horizontalSpeed = collidedObjectHorizontalSpeed - weakImpactHorizontalSpeedChange;

            yield return new WaitForSeconds(0.5f);

            _inelasticCollisionCoroutine = null;
        }

        /// <summary>
        /// Handle collisions with world objects and another vehicle.
        /// </summary>
        protected void HandleCollisions()
        {
            // Skip collision checks if they are not enabled.
            if (!CollisionIsEnabled)
                return;
            
            // World object collisions.
            foreach (TrackSegment segment in LoadedWorld.Segments)
            {
                if (segment.segmentObjects == null || !positionBetweenFrames.Contains(segment.WorldLine.Z))
                    continue;
                
                // Check if there is an object in the segment that collides with the vehicle.
                foreach (StaticWorldObject trackSegmentObject in segment.segmentObjects)
                {
                    if (
                        trackSegmentObject.Enabled && 
                        trackSegmentObject.Pseudo3DCollider.ColliderType is not ColliderType.None && 
                        trackSegmentObject.ColliderBuffer.Count > 0 &&
                        trackSegmentObject.ColliderBuffer[Layer].Overlaps(ColliderBuffer[Layer]) && 
                        _crashCoroutine == null
                        )
                    {
                        HandleObjectCollisionEvent(trackSegmentObject);
                    }
                }
            }
            
            // Vehicle collisions.
            foreach (RoadVehicle vehicle in LoadedWorld.RoadVehicles)
                if (CheckIfVehicleCollides(vehicle))
                    HandleVehicleCollisionEvent(vehicle.ColliderType, vehicle);

            foreach (PlayerVehicle vehicle in LoadedWorld.PlayerVehicles)
            {
                if (vehicle == this)
                    continue;

                if (CheckIfPlayerVehicleCollides(vehicle))
                    HandleInelasticCollision(vehicle);
                
            }
        }

        /// <summary>
        /// Check if this player vehicle is colliding with another vehicle.
        /// </summary>
        /// <param name="vehicle">The other vehicle.</param>
        /// <returns>True if a collision is occurring. False otherwise.</returns>
        private bool CheckIfVehicleCollides(IVehicle vehicle)
        {
            return vehicle.CollisionIsEnabled && vehicle.ColliderBuffer.ContainsKey(Layer) && vehicle.ColliderBuffer[Layer].Overlaps(ColliderBuffer[Layer]);
        }

        /// <summary>
        /// Check if this player vehicle is colliding with another player vehicle.
        /// </summary>
        /// <param name="vehicle">The other player vehicle.</param>
        /// <returns>True if a collision is occurring. False otherwise.</returns>
        private bool CheckIfPlayerVehicleCollides(PlayerVehicle vehicle)
        {
            return (vehicle.Pseudo3DCollider.ColliderType == ColliderType.Elastic) && CheckIfVehicleCollides(vehicle);
        }

        /// <summary>
        /// Handle a collision with an object on the game world.
        /// </summary>
        /// <param name="worldObject">The object in the game world.</param>
        /// <exception cref="NotImplementedException">If the type of collision is not valid.</exception>
        private void HandleObjectCollisionEvent(IWorldObject worldObject)
        {
            switch (worldObject.Pseudo3DCollider.ColliderType)
            {
                case ColliderType.Unmovable:
                    HandleFatalCrash();
                    break;
                case ColliderType.Collectible:
                    HandleCollectible(worldObject);
                    break;
                default:
                    throw new NotImplementedException("Collision type not implemented for in-game object.");
            }
        }
        
        /// <summary>
        /// Handle a collision with another vehicle.
        /// </summary>
        /// <param name="colliderType">The type of collider.</param>
        /// <param name="vehicle">The collided vehicle.</param>
        /// <exception cref="NotImplementedException">If the type of collision is not valid.</exception>
        private void HandleVehicleCollisionEvent(ColliderType colliderType, IVehicle vehicle)
        {
            switch (colliderType)
            {
                case ColliderType.Unmovable:
                    HandleFatalCrash();
                    break;
                case ColliderType.Elastic:
                    HandleElasticCollision(vehicle);
                    break;
                default:
                    throw new NotImplementedException("Collision type not implemented for vehicle.");
            }
        }

        /// <summary>
        /// Handle frontal car movement and updates its world position.
        /// </summary>
        protected void HandleVehicleMovement()
        {
            // Update speed and position.
            float terrainFriction = IsOutsideRoad ? CurrentGearParameters.outOfRoadFriction : 0f;
            float driftingFriction = IsDrifting ? CurrentGearParameters.driftingFriction : 0f;
            float totalFriction = CurrentGearParameters.friction + driftingFriction + terrainFriction;
            vehicleState.trackSpeed += (CurrentAcceleration - totalFriction * vehicleState.trackSpeed) * Time.fixedDeltaTime;
            
            if (!allowReverse && vehicleState.trackSpeed < 0)
                vehicleState.trackSpeed = 0;

            float oldPosition = vehicleState.trackPosition;
            float newPosition = vehicleState.trackPosition += vehicleState.trackSpeed * Time.fixedDeltaTime;
            float halfColliderLength = pseudo3DCollider.ColliderLength / 2f;
            positionBetweenFrames = new Interval<float>(oldPosition - halfColliderLength, newPosition + halfColliderLength);
            
            // Loop track.
            int trackTrip = LoadedWorld.Length * LoadedWorld.SegmentLength;
            while (vehicleState.trackPosition >= trackTrip)
            {
                vehicleState.trackPosition -= trackTrip;
                Laps++;
                
                onLapFinished.Invoke(this);
            }

            while (vehicleState.trackPosition < 0)
                vehicleState.trackPosition += trackTrip;
        }

        /// <summary>
        /// Handle fuel consumption.
        /// </summary>
        private void HandleFuel()
        {
            if (Gear != Gear.High)
                return;

            Fuel -= fuelConsumption * Time.fixedDeltaTime;
            Fuel = Mathf.Clamp(Fuel, 0f, fuelCapacity);
            if (Fuel == 0f)
                SetGearLow();
        }

        /// <summary>
        /// Handle directional moving and corresponding car animations.
        /// </summary>
        private void HandleVehicleSteering()
        {
            // Simulate wheel-turning physics.
            int trackPositionIndex = Mathf.FloorToInt(vehicleState.trackPosition) / LoadedWorld.SegmentLength;

            TrackSegment currentSegment = LoadedWorld.Segments[trackPositionIndex];
            vehicleState.horizontalSpeed = TurnDirection * horizontalSpeed * TurnPercentage - currentSegment.Curve * centrifugal * Mathf.Abs(vehicleState.trackSpeed);
            vehicleState.horizontalPosition += vehicleState.horizontalSpeed * Time.fixedDeltaTime;
            if (TurnDirection != 0)
            {
                VehicleDirection direction = TurnDirection == 1 ? VehicleDirection.TurnRight : VehicleDirection.TurnLeft;
                vehicleAnimationController.SetDirection(direction);
            }
            else
            {
                vehicleAnimationController.SetDirection(VehicleDirection.Idle);
            }

            // Restrict the horizontal position.
            vehicleState.horizontalPosition = Mathf.Clamp(vehicleState.horizontalPosition, -steeringAmplitude, steeringAmplitude);
        }

        /// <summary>
        /// Handle drifting smoke and engine noise.
        /// </summary>
        private void HandleVehicleFX()
        {
            // Simulate engine noise.
            float noisePercentage = Mathf.Clamp(vehicleState.trackSpeed / CurrentGearMaxSpeed, 0f, 1f);
            engineAudio.pitch = noisePercentage;
            vehicleAnimationController.EnableSmoke(IsDrifting);
        }
        
        /// <summary>
        /// Update HUD properties.
        /// </summary>
        private void HandleHUD()
        {
            speedMeter.SetSpeed(Mathf.RoundToInt(vehicleState.trackSpeed));
            throttleIndicator.SetBarPercentage(vehicleState.trackSpeed / CurrentGearMaxSpeed);
            turboPanel.SetFuelPercentage(Fuel / fuelCapacity);
        }
        
        #endregion

        #region Interactions

        /// <summary>
        /// Handle user input.
        /// </summary>
        public virtual void HandleInput()
        {
            HandleHorizontalDirectionInput();
            HandlePedalInput();
            HandleTurboInput();
        }

        /// <summary>
        /// Switch the vehicle gear if there is enough fuel.
        /// </summary>
        public void SwitchGears()
        {
            if (Gear == Gear.Low && Fuel > 0f)
                SetGearHigh();
            else
                SetGearLow();
        }

        /// <summary>
        /// Input handler for the directional control.
        /// </summary>
        private void HandleHorizontalDirectionInput()
        {
            float intValue = directionalInput.ReadValue<float>();
            if (intValue == 0)
                GoStraight();
            else if (intValue > 0)
                TurnRight();
            else
                TurnLeft();
        }

        /// <summary>
        /// Input handler for the acceleration/deceleration control.
        /// </summary>
        private void HandlePedalInput()
        {
            float intValue = pedalInput.ReadValue<float>();
            if (intValue == 0)
                Idle();
            else if (intValue > 0)
                Accelerate();
            else
                Decelerate();
        }

        /// <summary>
        /// Input handler for the turbo control.
        /// </summary>
        private void HandleTurboInput()
        {
            if (turboInput.IsPressed())
                SetGearHigh();
            else
                SetGearLow();
        }
        
        /// <summary>
        /// Make the vehicle turn left.
        /// </summary>
        public void TurnLeft()
        {
            TurnDirection = -1;
        }

        /// <summary>
        /// Make the vehicle turn right.
        /// </summary>
        public void TurnRight()
        {
            TurnDirection = 1;
        }

        /// <summary>
        /// Make the vehicle go straight.
        /// </summary>
        public void GoStraight()
        {
            TurnDirection = 0;
        }

        /// <summary>
        /// Increase the acceleration (push the pedal).
        /// </summary>
        public void Accelerate()
        {
            if (!IsCrashing && CurrentAcceleration <= 0)
                CurrentAcceleration = CurrentGearParameters.acceleration;
        }

        /// <summary>
        /// Set no acceleration (remove foot from pedal).
        /// </summary>
        public void Idle()
        {
            CurrentAcceleration = 0;
        }

        /// <summary>
        /// Reduce acceleration (brake).
        /// </summary>
        public void Decelerate()
        {
            if (!IsCrashing && CurrentAcceleration >= 0)
                CurrentAcceleration = -CurrentGearParameters.acceleration;
        }

        /// <summary>
        /// Set the vehicle gear as low.
        /// </summary>
        protected void SetGearLow()
        {
            if (Gear == Gear.High)
                SetGear(Gear.Low);
        }

        /// <summary>
        /// Set the vehicle gear as high.
        /// </summary>
        protected void SetGearHigh()
        {
            if (Gear == Gear.Low && Fuel > 0f)
                SetGear(Gear.High);
        }

        /// <summary>
        /// Switch gear while also adjusting acceleration.
        /// </summary>
        /// <param name="gear">The gear level.</param>
        /// <param name="silent"></param>
        private void SetGear(Gear gear, bool silent = false)
        {
            // Release the pedal.
            int accelerationSign = Math.Sign(CurrentAcceleration);
            if (accelerationSign != 0)
                Idle();
            
            // Switch gear.
            GameLogger.Debug("Switching to gear: " + gear, this);
            Gear = gear;
            CurrentGearMaxSpeed = CurrentGearParameters.CalculateMaxSpeed();
            
            switch (Gear)
            {
                case Gear.High when !silent:
                    turboPanel.SetTurboOn();
                    vehicleAnimationController.EnableTurbo(true);
                    turboSoundController.Play();
                    break;
                case Gear.Low when !silent:
                    turboPanel.SetTurboOff();
                    vehicleAnimationController.EnableTurbo(false);
                    turboSoundController.Stop();
                    break;
            }

            // Press the pedal again if it was previously pressed.
            if (accelerationSign == 1)
                Accelerate();
            else if (accelerationSign == -1)
                Decelerate();
        }
        
        #endregion
    }
}
