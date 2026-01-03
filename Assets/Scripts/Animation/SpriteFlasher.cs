using DG.Tweening;
using TriInspector;
using UnityEngine;

namespace Animation
{
    /// <summary>
    /// Special effect that flashes a sprite.
    /// </summary>
    public class SpriteFlasher : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] SpriteRenderer spriteRenderer;
        
        [Title("Parameters")]
        [SerializeField] float duration;
        [SerializeField] float frequency = 3;
        
        /// <summary>
        /// Get the flashing DoTween sequence.
        /// </summary>
        /// <returns>The flashing sequence.</returns>
        public Sequence PlayFX()
        {
            float sequenceDuration = duration / (2f * frequency);
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < frequency - 1; i++)
            {
                sequence.Append(spriteRenderer.DOFade(1f, sequenceDuration));
                sequence.Append(spriteRenderer.DOFade(0f, sequenceDuration));
            }
            sequence.Append(spriteRenderer.DOFade(1f, sequenceDuration));

            return sequence;
        }
        
    }
}