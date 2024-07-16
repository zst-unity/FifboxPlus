using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Fifbox
{
    public class FifboxEventSystem : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            foreach (var es in FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
            {
                if (es.gameObject == gameObject) continue;
                Destroy(es.gameObject);
            }
        }
    }
}