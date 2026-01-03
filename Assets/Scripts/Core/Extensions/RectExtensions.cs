using UnityEngine;

namespace Core.Extensions
{
    internal static class RectExtensions
    {
        public static void RescaleX(this ref Rect original, float scalingFactor)
        {
            // Calculate the new width based on the scale factor
            float newWidth = original.width * scalingFactor;

            // Calculate the difference in width
            float widthDifference = original.width - newWidth;

            // Calculate the offset needed to keep the center position constant
            float offsetX = widthDifference / 2;
            
            original.width = newWidth;
            original.x += offsetX;
        }
        
        public static void RescaleY(this ref Rect original, float scalingFactor)
        {
            // Calculate the new width based on the scale factor
            float newHeight = original.height * scalingFactor;

            // Calculate the difference in width
            float heightDifference = original.height - newHeight;

            // Calculate the offset needed to keep the center position constant
            float offsetY = heightDifference / 2;
            
            original.height = newHeight;
            original.y += offsetY;
        }

        public static void Rescale(this ref Rect original, Vector2 scalingFactors)
        {
            original.RescaleX(scalingFactors.x);
            original.RescaleY(scalingFactors.y);
        }
    }
}