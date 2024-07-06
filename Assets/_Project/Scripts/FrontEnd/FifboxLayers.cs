using UnityEngine;

namespace Fifbox.FrontEnd
{
    public static class FifboxLayers
    {
        public const int LOCAL_PLAYER_LAYER = 6;
        public const int PLAYER_LAYER = 7;
        public const int NOCLIPING_PLAYER_LAYER = 8;

        public static readonly int MapLayers = LayerMask.GetMask("Map", "Player");
    }
}