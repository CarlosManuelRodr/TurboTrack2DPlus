using System.Collections.Generic;
using Animation;
using TriInspector;
using UnityEngine;
using World;

namespace Vehicle
{
    /// <summary>
    /// Centralized controller for the vehicle animations.
    /// </summary>
    public class VehicleAnimationController : MonoBehaviour, ILayeredVisible
    {
        [Title("References")]
        [SerializeField] private VehicleSpriteAnimator vehicleSpriteAnimator;
        [SerializeField] private VehicleFXAnimator smokeAnimator;
        [SerializeField] private VehicleFXAnimator turboAnimator;
        [SerializeField] private SpriteAnimator explosionAnimator;
        [SerializeField] private SpriteAnimator hitAnimator;
        [SerializeField] private SpriteAnimator coinAnimator;

        // Properties.
        public List<Sprite> CurrentSprites => CombineSprites();
        public Sprite VehicleSprite => vehicleSpriteAnimator.CurrentSprite;
        
        /// <summary>
        /// Set the facing direction of the vehicle.
        /// </summary>
        /// <param name="vehicleDirection">The direction.</param>
        public void SetDirection(VehicleDirection vehicleDirection)
        {
            vehicleSpriteAnimator.SetVehicleDirection(vehicleDirection);
            smokeAnimator.SetVehicleDirection(vehicleDirection);
            turboAnimator.SetVehicleDirection(vehicleDirection);
        }
        
        /// <summary>
        /// Combine all the layered vehicle sprites.
        /// </summary>
        /// <returns>The list of layered vehicle sprites.</returns>
        private List<Sprite> CombineSprites()
        {
            List<Sprite> spriteList = new List<Sprite>();
            if (coinAnimator.Enabled)
                spriteList.Add(coinAnimator.CurrentSprite);
            if (hitAnimator.Enabled)
                spriteList.Add(hitAnimator.CurrentSprite);
            if (vehicleSpriteAnimator.Enabled)
                spriteList.Add(vehicleSpriteAnimator.CurrentSprite);
            if (turboAnimator.Enabled)
                spriteList.Add(turboAnimator.CurrentSprite);
            if (smokeAnimator.Enabled)
                spriteList.Add(smokeAnimator.CurrentSprite);
            if (explosionAnimator.Enabled)
                spriteList.Add(explosionAnimator.CurrentSprite);

            return spriteList;
        }

        /// <summary>
        /// Activate/Deactivate the smoke animation.
        /// </summary>
        public void EnableSmoke(bool value)
        {
            smokeAnimator.SetVisible(value);
        }
        
        /// <summary>
        /// Activate/Deactivate the turbo animation.
        /// </summary>
        public void EnableTurbo(bool value)
        {
            turboAnimator.SetVisible(value);
        }

        /// <summary>
        /// Play the explosion animation.
        /// </summary>
        public void PlayExplosion()
        {
            explosionAnimator.Play();
        }

        /// <summary>
        /// Play the soft collision animation.
        /// </summary>
        public void PlayHit()
        {
            hitAnimator.Play();
        }

        /// <summary>
        /// Play the "coin collected" animation.
        /// </summary>
        public void PlayCollectCoin()
        {
            coinAnimator.Play();
        }
    }
}