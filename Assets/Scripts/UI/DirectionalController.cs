using TriInspector;
using UnityEngine;
using Vehicle;

namespace UI
{
    public class DirectionalController : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] private ControllerButton leftButton;
        [SerializeField] private ControllerButton rightButton;
        [SerializeField] private PlayerVehicle playerVehicle;

        public void Update()
        {
            if (!leftButton.IsPressed && !rightButton.IsPressed)
                playerVehicle.GoStraight();
        }
    }
}