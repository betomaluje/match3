using UnityEngine;

namespace Tiles {
    [CreateAssetMenu(fileName = "Tile", menuName = "Game/Tiles", order = 0)]
    public class TileSO : ScriptableObject {
        public TileType type = TileType.None;

        public Transform destroyFX;

        public float timeToMove = .2f;
    }
}