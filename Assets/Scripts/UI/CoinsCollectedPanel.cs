using TriInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CoinsCollectedPanel : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] Image resultImage;
        [SerializeField] private TextMeshProUGUI collectedLabel;
        
        [Title("Parameters")]
        [SerializeField] private Sprite winSprite;
        [SerializeField] private Sprite loseSprite;

        public void SetResult(bool result)
        {
            gameObject.SetActive(true);
            resultImage.sprite = result ? winSprite : loseSprite;
        }

        public void SetCollectedCoins(int collectedCoins)
        {
            collectedLabel.text = collectedCoins.ToString();
        }

    }
}