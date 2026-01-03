using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private GameObject loadingScreenPrefab;
        
        public void LoadSceneWithLoadingScreen(string sceneName)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            // Prevent this GameObject from being destroyed when loading the new scene.
            DontDestroyOnLoad(gameObject);
            
            // Instantiate the loading screen.
            GameObject loadingScreen = Instantiate(loadingScreenPrefab);

            // Prevent it from being destroyed when the scene loads.
            DontDestroyOnLoad(loadingScreen);

            // Start loading the scene asynchronously.
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (asyncLoad is { isDone: false })
            {
                yield return null; // Wait for the next frame.
            }

            // Destroy the loading screen once the scene has loaded.
            Destroy(loadingScreen);
            
            // Destroy this GameObject if it's no longer needed.
            Destroy(gameObject);
        }
    }
}