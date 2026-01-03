namespace Physics
{
    /// <summary>
    /// Defines how the collider will behave.
    /// </summary>
    public enum ColliderType
    {
        /// <summary>
        /// No collider.
        /// </summary>
        None, 
        
        /// <summary>
        /// A fixed collider that makes the vehicle crash.
        /// </summary>
        Unmovable, 
        
        /// <summary>
        /// A collective item.
        /// </summary>
        Collectible, 
        
        /// <summary>
        /// Collider that fires-up an event.
        /// </summary>
        Event, 
        
        /// <summary>
        /// A collider that produces an elastic collision.
        /// </summary>
        Elastic
    }
}