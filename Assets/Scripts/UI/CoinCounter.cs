using TriInspector;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Counter that keeps track of the collected coins.
    /// </summary>
    public class CoinCounter : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] TextMeshProUGUI coinLabel;

        private void Awake()
        {
            SetCoins(0);
        }

        /// <summary>
        /// Set the number of coins.
        /// </summary>
        /// <param name="coins">The number of coins.</param>
        public void SetCoins(int coins)
        {
            coinLabel.text = coins.ToString();
        }
    }
}