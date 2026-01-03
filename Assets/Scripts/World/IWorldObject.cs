using Physics;

namespace World
{
    /// <summary>
    /// An object on the game world.
    /// </summary>
    public interface IWorldObject : ICollidable, IVisible
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