using System.Collections;
using TriInspector;
using UnityEngine;

namespace Audio
{
    /// <summary>
    /// Controller for a looping sound that has an intro part, main loop and outro part.
    /// </summary>
    public class LoopingSoundController : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] private AudioSource audioSource;

        [Title("Parameters")]
        [SerializeField] private AudioClip introClip;
        [SerializeField] private AudioClip loopClip;
        [SerializeField] private AudioClip outroClip;
        
        // Private members.
        private Coroutine _loopCoroutine;

        private void Start()
        {
            _loopCoroutine = null;
        }

        /// <summary>
        /// Start the sound loop.
        /// </summary>
        public void Play()
        {
            _loopCoroutine ??= StartCoroutine(LoopSequence());
        }

        /// <summary>
        /// End the sound loop.
        /// </summary>
        public void Stop()
        {
            if (_loopCoroutine != null)
            {
                StopCoroutine(_loopCoroutine);
                _loopCoroutine = null;
            }

            audioSource.Stop();
            audioSource.clip = outroClip;
            audioSource.loop = false;
            audioSource.Play();
        }
        
        /// <summary>
        /// Coroutine for the loop sequence.
        /// </summary>
        private IEnumerator LoopSequence()
        {
            // Play intro.
            audioSource.clip = introClip;
            audioSource.loop = false;
            audioSource.Play();
            yield return new WaitUntil(() => audioSource.isPlaying == false);

            // Play loop.
            audioSource.clip = loopClip;
            audioSource.loop = true;
            audioSource.Play();

            _loopCoroutine = null;
        }
    }
}