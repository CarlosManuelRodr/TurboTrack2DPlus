using TriInspector;
using UnityEngine;

namespace Rendering
{
    /// <summary>
    /// Simulate a horizontally looped background. It works by repositioning mirror images.
    /// </summary>
    public class BackgroundLoop : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] SpriteRenderer mainSprite;
        [SerializeField] SpriteRenderer leftCopy;
        [SerializeField] SpriteRenderer rightCopy;
        
        private Vector3 _initialPosition;
        private float _spriteLength;
        private SpriteRenderer _leftSprite;
        private SpriteRenderer _centerSprite;
        private SpriteRenderer _rightSprite;
        
        private float Displacement => _centerSprite.transform.position.x - _initialPosition.x;

        #region Unity hooks
        
        private void Awake()
        {
            _spriteLength = mainSprite.bounds.size.x;
            _initialPosition = mainSprite.transform.position;

            _leftSprite = leftCopy;
            _centerSprite = mainSprite;
            _rightSprite = rightCopy;
        }

        private void Start()
        {
            Sprite spriteContent = mainSprite.sprite;

            if (_leftSprite.sprite == null)
                _leftSprite.sprite = spriteContent;

            if (_rightSprite.sprite == null)
                _rightSprite.sprite = spriteContent;

            _leftSprite.transform.position = new Vector3(_initialPosition.x - _spriteLength, _initialPosition.y);
            _rightSprite.transform.position = new Vector3(_initialPosition.x + _spriteLength, _initialPosition.y);
        }
        
        #endregion

        #region Public API
        
        /// <summary>
        /// Apply a translation to the background.
        /// </summary>
        /// <param name="translation"></param>
        public void Translate(Vector3 translation)
        {
            mainSprite.transform.Translate(translation, Space.Self);
            leftCopy.transform.Translate(translation, Space.Self);
            rightCopy.transform.Translate(translation, Space.Self);

            if (Displacement < 0)
                RepositionRight();
            else if (Displacement > _spriteLength)
                RepositionLeft();
        }
        
        #endregion

        #region Loop effect implementation
        
        /// <summary>
        /// Reposition the left image to the right.
        /// </summary>
        private void RepositionLeft()
        {
            SpriteRenderer oldLeftSprite = _leftSprite;
            SpriteRenderer oldCenterSprite = _centerSprite;
            SpriteRenderer oldRightSprite = _rightSprite;
            
            _rightSprite.transform.position = _leftSprite.transform.position - new Vector3(_spriteLength, _initialPosition.y);

            _leftSprite = oldRightSprite;
            _centerSprite = oldLeftSprite;
            _rightSprite = oldCenterSprite;

        }

        /// <summary>
        /// Reposition the right image to the left.
        /// </summary>
        private void RepositionRight()
        {
            SpriteRenderer oldLeftSprite = _leftSprite;
            SpriteRenderer oldCenterSprite = _centerSprite;
            SpriteRenderer oldRightSprite = _rightSprite;

            _leftSprite.transform.position = _rightSprite.transform.position + new Vector3(_spriteLength, _initialPosition.y);
            
            _leftSprite = oldCenterSprite;
            _centerSprite = oldRightSprite;
            _rightSprite = oldLeftSprite;
        }
        
        #endregion
    }
}