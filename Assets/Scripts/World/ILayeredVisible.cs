using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace World
{
    /// <summary>
    /// A visible element that can be composed of multiple layers.
    /// </summary>
    public interface ILayeredVisible
    {
        /// <summary>
        /// The layer sprites.
        /// </summary>
        public List<Sprite> CurrentSprites { get; }
        
        /// <summary>
        /// The current visible sprite.
        /// </summary>
        public Sprite CurrentSprite => CurrentSprites.First();
    }
}