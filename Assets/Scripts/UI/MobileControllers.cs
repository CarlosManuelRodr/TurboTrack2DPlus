using UnityEngine;

namespace UI
{
    /// <summary>
    /// Activate/deactivate the mobile controllers depending on the platform.
    /// </summary>
    [ExecuteInEditMode]
    public class MobileControllers : MonoBehaviour
    {
        private void Awake()
        {
            foreach (Transform child in transform)
            {
            #if UNITY_STANDALONE
                child.gameObject.SetActive(false);
            #endif

            #if UNITY_ANDROID
                child.gameObject.SetActive(true);
            #endif
            }
        }
    }
}
