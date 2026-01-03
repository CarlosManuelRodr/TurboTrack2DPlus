using TriInspector;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// The player vehicle speed meter UI component.
    /// </summary>
    public class SpeedMeter : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] TextMeshProUGUI speedLabel;

        /// <summary>
        /// Scale parameter that converts between game world coordinates speed into kilometer per hour.
        /// </summary>
        [Title("Parameters")]
        [SerializeField] float scaleParameter = 0.01f;

        /// <summary>
        /// Set the current scaled speed value.
        /// </summary>
        /// <param name="speed">The speed value.</param>
        public void SetSpeed(int speed)
        {
            int scaledSpeed = (int)(speed * scaleParameter);
            speedLabel.text = scaledSpeed.ToString();
        }
    }
}