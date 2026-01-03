using UnityEngine;

namespace UI
{
    /// <summary>
    /// Moves the object this script is attached to, in order to create a looping background effect.
    /// </summary>
    public class ScrollingBackground : MonoBehaviour
    {
        [Header("Parameters")]
        public float speed = 1.0f;
        public float groundHorizontalLength = 1920f;

        void Update()
        {
            transform.Translate(speed * Time.deltaTime * Vector2.right, Space.Self);
            if (transform.localPosition.x < -groundHorizontalLength)
                RepositionBackground();
        }

        private void RepositionBackground()
        {
            Vector2 groundOffSet = new Vector2(2f * groundHorizontalLength, 0);
            transform.localPosition = (Vector2)transform.localPosition + groundOffSet;
        }

    }
}