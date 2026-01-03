using UnityEngine;

namespace Core
{
    /// <summary>
    /// Controller that configures the multi-display set-up of this game.
    /// </summary>
    public class DisplayController : MonoBehaviour
    {
        private void Start()
        {
            // Initialize displays.
            for (int i = 1; i < Display.displays.Length; i++)
            {
                if (!Display.displays[i].active)
                    Display.displays[i].Activate();
            }
        }
    }
}