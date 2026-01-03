using TriInspector;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI
{
    /// <summary>
    /// A panel that shows the turbo status and the amount of fuel. When activated,
    /// it shakes and glows.
    /// </summary>
    public class TurboPanel : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] private Image image;
        [SerializeField] private FilledBar fuelBar;
        
        [Title("Parameters")]
        [SerializeField] private Sprite turboOffPanel;
        [SerializeField] private Sprite turboOnPanel;
        [SerializeField] private float shakeAmplitude = 1f;
        
        // Private members.
        private RectTransform _rectTransform;
        private Vector2 _startingPos;
        private bool _turboOn;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _startingPos = _rectTransform.anchoredPosition;
            _turboOn = false;
        }

        private void FixedUpdate()
        {
            if (_turboOn)
            {
                _rectTransform.anchoredPosition = new Vector2(
                    _startingPos.x + Random.Range(0f, shakeAmplitude),
                    _startingPos.y + Random.Range(0f, shakeAmplitude)
                    );
            }
        }

        /// <summary>
        /// Set the amount of fuel.
        /// </summary>
        /// <param name="percentage">Amount of fuel in the interval [0,1]</param>
        public void SetFuelPercentage(float percentage)
        {
            fuelBar.SetBarPercentage(percentage);
        }

        /// <summary>
        /// Activate the turbo mode.
        /// </summary>
        public void SetTurboOn()
        {
            image.sprite = turboOnPanel;
            _turboOn = true;
        }

        /// <summary>
        /// Deactivate the turbo mode.
        /// </summary>
        public void SetTurboOff()
        {
            _rectTransform.anchoredPosition = _startingPos;
            image.sprite = turboOffPanel;
            _turboOn = false;
        }

    }
}