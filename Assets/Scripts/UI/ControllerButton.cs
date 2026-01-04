using Core;
using TriInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vehicle;

namespace UI
{
    /// <summary>
    /// A button that is used to control the player vehicle.
    /// </summary>
    public class ControllerButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        /// <summary>
        /// Defines how this controller button will interact with the player vehicle.
        /// </summary>
        public enum ControllerButtonAction
        {
            Accelerate, Decelerate, TurnLeft, TurnRight, SwitchGears
        }
        
        [Title("References")]
        [SerializeField] PlayerVehicle playerVehicle;
        [SerializeField] Image buttonImage;
        
        [Title("Parameters")]
        [SerializeField] ControllerButtonAction controllerButtonAction;
        [SerializeField] Sprite buttonUpSprite;
        [SerializeField] Sprite buttonDownSprite;
        
        private bool _isPressed;

        /// <summary>
        /// Handle the button being pushed down.
        /// </summary>
        /// <param name="eventData">Event properties.</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            GameLogger.Debug("Pressed controller button for action: " + controllerButtonAction, this);
            
            buttonImage.sprite = buttonDownSprite;
            _isPressed = true;
            
            // SwitchGears is a one-time toggle, so we keep it here and don't put it in Update
            if (controllerButtonAction == ControllerButtonAction.SwitchGears)
            {
                //playerVehicle.SwitchGears();
            }
        }

        /// <summary>
        /// Handle the button being released.
        /// </summary>
        /// <param name="eventData">Event properties.</param>
        public void OnPointerUp(PointerEventData eventData)
        {
            GameLogger.Debug("Released controller button for action: " + controllerButtonAction, this);

            buttonImage.sprite = buttonUpSprite;
            _isPressed = false;

        }
        
        private void Update()
        {
            if (_isPressed)
            {
                switch (controllerButtonAction)
                {
                    case ControllerButtonAction.Accelerate:
                        playerVehicle.Accelerate();
                        break;
                    case ControllerButtonAction.Decelerate:
                        playerVehicle.Decelerate();
                        break;
                    case ControllerButtonAction.TurnLeft:
                        playerVehicle.TurnLeft();
                        break;
                    case ControllerButtonAction.TurnRight:
                        playerVehicle.TurnRight();
                        break;
                    case ControllerButtonAction.SwitchGears:
                        playerVehicle.SetGearHigh();
                        break;
                }
            }
            else
            {
                switch (controllerButtonAction)
                {
                    case ControllerButtonAction.Accelerate:
                        playerVehicle.Idle();
                        break;
                    case ControllerButtonAction.TurnRight:
                        playerVehicle.GoStraight();
                        break;
                    case ControllerButtonAction.SwitchGears:
                        playerVehicle.SetGearLow();
                        break;
                }
            }
        }
    }
}