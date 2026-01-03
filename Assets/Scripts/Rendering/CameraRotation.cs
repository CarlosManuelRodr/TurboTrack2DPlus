using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Vehicle;

namespace Rendering
{
    /// <summary>
    /// FX that rotates the camera on curves.
    /// </summary>
    [ExecuteInEditMode]
    class CameraRotation: MonoBehaviour
    {
        [FormerlySerializedAs("body")]
        [Title("References")]
        [SerializeField] PlayerVehicle playerVehicle;
        
        [Title("Parameters")]
        [SerializeField] float maximumCameraRotation = 1.0f;
        [SerializeField] float rotationSpeed = 0.001f;

        private float _targetCameraRotation;
        
        private void Awake()
        {
            _targetCameraRotation = maximumCameraRotation;
        }

        private void FixedUpdate()
        {
            // Rotate.
            float currentAngle = transform.rotation.eulerAngles.z;
            float targetAngle;
            
            _targetCameraRotation = playerVehicle.TurnPercentage * maximumCameraRotation;
            
            if (playerVehicle.TurnDirection > 0)
                targetAngle = Mathf.LerpAngle(currentAngle, -_targetCameraRotation, rotationSpeed * Time.time);
            else if (playerVehicle.TurnDirection < 0)
                targetAngle = Mathf.LerpAngle(currentAngle, _targetCameraRotation, rotationSpeed * Time.time);
            else
                targetAngle = Mathf.LerpAngle(currentAngle, 0.0f, rotationSpeed * Time.time);

            transform.eulerAngles = new Vector3(0f, 0f, targetAngle);
        }
    }
}
