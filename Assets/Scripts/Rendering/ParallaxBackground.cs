using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

namespace Rendering
{
    /// <summary>
    /// Simulate a looped parallax background by translating them at different speeds. 
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        /// <summary>
        /// A parallax layer is composed by a background loop and a parallax factor in the range [0,1].
        /// A value of 0 means that the layer is not affected by translation. A value of 1 means
        /// that the layer is fully affected by the translation.
        /// </summary>
        [Serializable]
        public class ParallaxLayer
        {
            public BackgroundLoop layerLoop;
            public float parallaxFactor;
        }

        [Title("Parameters")]
        [SerializeField] List<ParallaxLayer> layers;

        /// <summary>
        /// Apply a translation simulating a parallax effect.
        /// </summary>
        /// <param name="translation"></param>
        public void Translate(Vector3 translation)
        {
            foreach (ParallaxLayer layer in layers)
                layer.layerLoop.Translate(layer.parallaxFactor * translation);
        }
    }
}