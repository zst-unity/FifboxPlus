using Fifbox.Base;
using UnityEngine;
using ZSToolkit.ZSTUtility;

namespace Fifbox.API.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Layers", menuName = "Fifbox/Layers", order = 0)]
    public class FifboxLayers : SingletonScriptableObject<FifboxLayers>
    {
        [SerializeField] private SingleUnityLayer _playerLayer;
        [SerializeField] private SingleUnityLayer _localPlayerLayer;
        [SerializeField] private SingleUnityLayer _noclipingPlayerLayer;
        [SerializeField] private LayerMask _groundLayers;

        public static SingleUnityLayer PlayerLayer => Singleton._playerLayer;
        public static SingleUnityLayer LocalPlayerLayer => Singleton._localPlayerLayer;
        public static SingleUnityLayer NoclipingPlayerLayer => Singleton._noclipingPlayerLayer;
        public static LayerMask GroundLayers => Singleton._groundLayers;
    }
}