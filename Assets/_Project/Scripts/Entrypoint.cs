using Fifbox.Attributes;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZSToolkit.ZSTUtility.Extensions;

namespace Fifbox
{
    public class Entrypoint : MonoBehaviour
    {
        [SerializeField, Scene] private string _menuScene;
        [SerializeField, WithComponent(typeof(NetworkManager))] private GameObject _networkManager;

        private void Awake()
        {
            Debug.Log("Entrypoint awake");
            Debug.Log("Spawning network manager");
            _networkManager.gameObject.Spawn();
        }

        private void Start()
        {
            Debug.Log("Entrypoint finish, loading menu scene");
            SceneManager.LoadScene(_menuScene);
        }
    }
}