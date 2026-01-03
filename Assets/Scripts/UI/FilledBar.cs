using TriInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// A filled bar whose fill level can be set by percentage.
    /// </summary>
    public class FilledBar : MonoBehaviour
    {
        [Title("References")] 
        [SerializeField] Image barContent;

        /// <summary>
        /// Set the filled bar percentage.
        /// </summary>
        /// <param name="percentage">The filled bar percentage.</param>
        public void SetBarPercentage(float percentage)
        {
            barContent.fillAmount = Mathf.Clamp(percentage, 0f, 1f);
        }

    }
}
