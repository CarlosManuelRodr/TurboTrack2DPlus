using System.Collections.Generic;
using Level;
using Rendering;
using TriInspector;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Vehicle;
using World;

namespace Core
{
    /// <summary>
    /// Game controller for the racing module.
    /// </summary>
    [ExecuteInEditMode]
    public class GameController : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] private List<PlayerVehicle> playerVehicles;
        [SerializeField] private List<Pseudo3DRenderer> pseudo3DRenderers;
        [SerializeField] private List<Pseudo3DCamera> pseudo3DCameras;
        
        [Title("UI")]
        [SerializeField] private CoinsCollectedPanel playerACoinsPanel;
        [SerializeField] private CoinsCollectedPanel playerBCoinsPanel;
        
        [FormerlySerializedAs("level")]
        [Title("Parameters")]
        [SerializeField] private LevelData levelData;

        [Title(("Controls"))]
        [SerializeField] private bool multiplayerMode;
        [SerializeField] private InputAction pauseInput;
        
        [Title("Debug")]
        [SerializeField] private LoadedWorld loadedWorld;
        
        // Properties.
        private static bool GameIsPaused { get; set; }
        public LoadedWorld LoadedWorld => loadedWorld;
        public static GameController I { get; private set; }

        // Private vars.
        private Dictionary<PlayerVehicle, int> _lapsPerVehicle;
        
        #region Unity hooks
        
        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
        #if UNITY_EDITOR
            if (!Application.isPlaying)
                Initialize();
        #endif
            pauseInput.Enable();
        }

        private void Initialize()
        {
            // Initialize as singleton.
            if (I != null) 
                return;
            
            I = this;
            
            // Initialize logger.
            GameLogger.Info("Initializing game controller.", I);

            // Initialize the level scene.
            LoadLevel();
            playerVehicles.ForEach(x => x.Initialize());
            pseudo3DCameras.ForEach(x => x.Initialize());
            
            _lapsPerVehicle = new Dictionary<PlayerVehicle, int>();
            foreach (PlayerVehicle vehicle in playerVehicles) 
                _lapsPerVehicle.Add(vehicle, 0);

            // Subscribe to events.
            foreach (PlayerVehicle vehicle in playerVehicles)
            {
                vehicle.onLapFinished += HandleLapFinished;
                vehicle.onCollectibleCollected += HandleCoinCollected;
            }
        }
        
        private void OnDisable()
        {
            pauseInput.Disable();
            
            // Unsubscribe to events.
            foreach (PlayerVehicle vehicle in playerVehicles)
            {
                vehicle.onLapFinished -= HandleLapFinished;
                vehicle.onCollectibleCollected -= HandleCoinCollected;
            }
        }

        [Button("Reload level")]
        private void LoadLevel()
        {
            loadedWorld = new LoadedWorld(levelData, playerVehicles.ToArray());
            pseudo3DRenderers.ForEach(x => x.Initialize());
        }

        private void Update()
        {
        #if UNITY_EDITOR
            if (!Application.isPlaying)
                pseudo3DCameras.ForEach(x => x.HandleUpdate());
        #endif
            
            HandleInput();
            pseudo3DRenderers.ForEach(x => x.Render());
        }

        private void FixedUpdate()
        {
            loadedWorld.HandleUpdate();
            
            // Move and handle vehicles.
            foreach (IVehicle vehicle in loadedWorld.Vehicles)
                vehicle.HandleUpdate();
            
            // Pseudo follow.
            pseudo3DCameras.ForEach(x => x.HandleUpdate());
            pseudo3DRenderers.ForEach(x => x.MoveParallaxBackground());
        }
        
        #endregion

        #region Handlers
        
        /// <summary>
        /// Called whenever a vehicle has completed one round of the level.
        /// </summary>
        /// <param name="playerVehicle">The vehicle that performed the action.</param>
        private void HandleLapFinished(PlayerVehicle playerVehicle)
        {
            _lapsPerVehicle[playerVehicle]++;
        }

        /// <summary>
        /// Called whenever a coin has been collected.
        /// </summary>
        /// <param name="playerVehicle">The vehicle that performed the collection.</param>
        private void HandleCoinCollected(PlayerVehicle playerVehicle)
        {
            playerVehicle.Fuel += 5.0f;
        }

        /// <summary>
        /// Handle input from players.
        /// </summary>
        private void HandleInput()
        {
            if (pauseInput.WasPressedThisFrame())
                HandlePauseButton();
            
            if (Application.isPlaying)
                playerVehicles.ForEach(x => x.HandleInput());
        }

        /// <summary>
        /// Handle a pause.
        /// </summary>
        private void HandlePauseButton()
        {
            GameLogger.Info("Pause button pressed.", this);

            if (GameIsPaused)
                ResumeGame();
            else 
                PauseGame();
        }
        
        /// <summary>
        /// Put this controller and the vehicles in pause state.
        /// </summary>
        private void PauseGame()
        {
            GameLogger.Info("Game was paused.", this);

            foreach (IVehicle vehicle in loadedWorld.Vehicles)
                vehicle.Paused = true;
            
            GameIsPaused = true;
        }

        /// <summary>
        /// Put this controller and the vehicles in not pause state.
        /// </summary>
        private void ResumeGame()
        {
            GameLogger.Info("Game was resumed.", this);
            
            foreach (IVehicle vehicle in loadedWorld.Vehicles)
                vehicle.Paused = false;
            
            GameIsPaused = false;
        }

        #endregion
    }
}