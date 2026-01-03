using UnityEngine;

namespace Core
{
    /// <summary>
    /// Debugging utilities.
    /// </summary>
    public static class DebugUtils
    {
        /// <summary>
        /// Draw a rect using the lower left and upper right coordinates.
        /// </summary>
        /// <param name="min">The lower left coordinate.</param>
        /// <param name="max">The upper right coordinate.</param>
        /// <param name="color">The line color.</param>
        public static void DrawRect(Vector3 min, Vector3 max, Color color)
        {
            Debug.DrawLine(min, new Vector3(min.x, max.y), color);
            Debug.DrawLine(new Vector3(min.x, max.y), max, color);
            Debug.DrawLine(max, new Vector3(max.x, min.y), color);
            Debug.DrawLine(min, new Vector3(max.x, min.y), color);
        }

        /// <summary>
        /// Draw a rect.
        /// </summary>
        /// <param name="rect">The rect to draw.</param>
        /// <param name="color">The line color.</param>
        public static void DrawRect(Rect rect, Color color)
        {
            DrawRect(rect.min, rect.max, color);
        }

        /// <summary>
        /// Draw a cube defined by two rects, back and front faces.
        /// </summary>
        /// <param name="back">The back face rect.</param>
        /// <param name="front">The front face rect.</param>
        /// <param name="color">The line color.</param>
        public static void DrawCube(Rect back, Rect front, Color color)
        {
            // Draw squares.
            DrawRect(back.min, back.max, color);   // Back rect.
            DrawRect(front.min, front.max, color); // Front rect.

            // Draw lines between squares.
            Debug.DrawLine(back.min, front.min); // Lower left.
            Debug.DrawLine(back.max, front.max); // Upper right.
            Debug.DrawLine(new Vector3(back.min.x, back.max.y), new Vector3(front.min.x, front.max.y)); // Upper left.
            Debug.DrawLine(new Vector3(back.max.x, back.min.y), new Vector3(front.max.x, front.min.y)); // Lower right.
        }
    }
}