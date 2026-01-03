using Physics;

namespace World
{
    /// <summary>
    /// An object on the game world that can be composed of multiple visible layers.
    /// </summary>
    public interface ILayeredWorldObject : ICollidable, ILayeredVisible
    {
        /// <summary>
        /// Is this world object enabled?
        /// </summary>
        public bool Enabled { get; set; }
        
        /// <summary>
        /// The sprite scale of this world object.
        /// </summary>
        public float CustomSpriteScale { get; }
    }
}