using TriInspector;
using UnityEngine;

namespace Vehicle
{
    /// <summary>
    /// Controls the emission of off-road dust from the vehicle wheels.
    /// </summary>
    public class RoadDustController : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] private PlayerVehicle playerVehicle;
        
        [Title("Internal references")]
        [SerializeField] private ParticleSystem particlesLeft;
        [SerializeField] private ParticleSystem particlesRight;

        [Title("Parameters")]
        [SerializeField] private float thresholdVehicleSpeed;
        [SerializeField] private float maxVehicleSpeed;
        [SerializeField] private float minParticleSpeed;
        [SerializeField] private float maxParticleSpeed;
        
        // Private members.
        private bool _isEmittingDust;

        private void Awake()
        {
            _isEmittingDust = false;
        }

        private void FixedUpdate()
        {
            if (playerVehicle.IsOutsideRoad && playerVehicle.VehicleSpeed > thresholdVehicleSpeed)
            {
                float speedPercentage = (playerVehicle.VehicleSpeed - thresholdVehicleSpeed) / (maxVehicleSpeed - thresholdVehicleSpeed);
                float particleSpeed = Mathf.Lerp(minParticleSpeed, maxParticleSpeed, speedPercentage);

                ParticleSystem.MainModule mainLeft = particlesLeft.main;
                ParticleSystem.MainModule mainRight = particlesRight.main;

                mainLeft.startSpeed = particleSpeed;
                mainRight.startSpeed = particleSpeed;

                if (!_isEmittingDust)
                {
                    particlesLeft.gameObject.SetActive(true);
                    particlesRight.gameObject.SetActive(true);
                    _isEmittingDust = true;
                }
            }
            else
            {
                if (_isEmittingDust)
                {
                    particlesLeft.gameObject.SetActive(false);
                    particlesRight.gameObject.SetActive(false);
                    _isEmittingDust = false;
                }
            }
        }
    }
}