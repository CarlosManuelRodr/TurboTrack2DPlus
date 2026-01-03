using TriInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Vehicle;

namespace Rendering
{
    public class Pseudo3DCamera : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] PlayerVehicle playerVehicle;
        [FormerlySerializedAs("pseudo3DProjector")] [SerializeField] Pseudo3DRenderer pseudo3DRenderer;


        [Title("Parameters")]
        [SerializeField] float minCameraDistance = 1200;
        [SerializeField] float maxCameraDistance = 1400;
        [SerializeField] float acceleratingCameraSpeed = 50;
        [SerializeField] float deceleratingCameraSpeed = 150;


        [Title("Debug")]
        [SerializeField] [ReadOnly] float cameraDistance;
        
        public void Initialize()
        {
            cameraDistance = minCameraDistance;
        }

        public void HandleUpdate()
        {
            float targetDistance = playerVehicle.CurrentAcceleration > 0 ? maxCameraDistance : minCameraDistance;
            float cameraSpeed = playerVehicle.CurrentAcceleration > 0 ? acceleratingCameraSpeed : deceleratingCameraSpeed;
            
            cameraDistance = Mathf.MoveTowards(cameraDistance, targetDistance, cameraSpeed * Time.deltaTime);

            pseudo3DRenderer.CameraHorizontalPosition = playerVehicle.VehicleState.horizontalPosition;
            pseudo3DRenderer.CameraTrackPosition = playerVehicle.VehicleState.trackPosition - cameraDistance;
            pseudo3DRenderer.VehicleTrackPosition = playerVehicle.VehicleState.trackPosition;
            pseudo3DRenderer.UpdateProjections();
        }
    }
}