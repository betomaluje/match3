using System.Threading.Tasks;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private TileType _type = TileType.None;
    [SerializeField] private Transform _destroyFX;
    [SerializeField] private float _timeToMove = .2f;

    private GridManager _manager;

    public Vector3Int TileKey => Vector3Int.FloorToInt(transform.position);

    public TileType Type => _type;

    private void Awake()
    {
        _manager = FindObjectOfType<GridManager>();
    }

    public void OnMouseDown()
    {
        // we will use it's position to detect row and column
        _manager.OnTileDestroyed(TileKey);
    }

    public void DestroyTile()
    {
        if (_destroyFX != null) Instantiate(_destroyFX, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    public async Task<Vector3Int> MoveDown()
    {
        var currentPosition = transform.position;
        var targetPosition = Vector3Int.FloorToInt(currentPosition);
        targetPosition.y -= 1;

        var elapsedTime = 0f;

        while (elapsedTime < _timeToMove)
        {
            transform.position = Vector3.Lerp(currentPosition, targetPosition, elapsedTime / _timeToMove);
            elapsedTime += Time.deltaTime;
            await Task.Yield();
        }

        transform.position = targetPosition;

        return targetPosition;
    }
}