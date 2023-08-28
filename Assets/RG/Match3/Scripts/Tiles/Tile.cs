using System.Threading.Tasks;
using UnityEngine;

namespace Tiles {
    public class Tile : MonoBehaviour {
        [SerializeField]
        private TileSO _tileConfig;

        private GridManager _manager;

        // we will use it's position to detect row and column
        public Vector3Int TileKey => Vector3Int.FloorToInt(transform.position);

        public TileType Type => _tileConfig.type;

        private void Awake() {
            _manager = FindObjectOfType<GridManager>();
        }

        public void OnMouseDown() {
            _manager.OnTileClicked(TileKey);
        }

        public void DestroyTile() {
            if (_tileConfig.destroyFX != null) {
                Instantiate(_tileConfig.destroyFX, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }

        public async Task MoveDown(Vector3Int targetPosition) {
            if (targetPosition.y < 0) return;

            var currentPosition = TileKey;

            var elapsedTime = 0f;

            while (elapsedTime < _tileConfig.timeToMove) {
                transform.position =
                    Vector3.Lerp(currentPosition, targetPosition, elapsedTime / _tileConfig.timeToMove);
                elapsedTime += Time.deltaTime;
                await Task.Yield();
            }

            transform.position = targetPosition;
        }
    }
}