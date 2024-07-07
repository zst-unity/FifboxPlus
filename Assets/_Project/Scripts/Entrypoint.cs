using Fifbox.Input;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZSToolkit.ZSTUtility.Extensions;

namespace Fifbox
{
    public class Entrypoint : MonoBehaviour
    {
        [SerializeField, Scene] private string _menuScene;
        [SerializeField] private GameObject _networkManager;

        private void Awake()
        {
            Debug.Log("Entrypoint awake");

            Debug.Log("Initializing actions");
            FifboxActions.Init();

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