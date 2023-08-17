using RG.Match3.Scripts;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private TileType _type;
    [SerializeField] private Transform _destroyFX;

    private GridManager _manager;

    private void Awake()
    {
        _manager = FindObjectOfType<GridManager>();
    }

    public void OnMouseDown()
    {
        if (_destroyFX != null) Instantiate(_destroyFX, transform.position, Quaternion.identity);

        // we will use it's position to detect row and column
        _manager.OnTileDestroyed(transform.position);
        Destroy(gameObject);
    }

    private void MoveDown()
    {
        var targetPosition = Vector3Int.FloorToInt(transform.position);
        targetPosition.y -= 1;
        if (targetPosition.y > 0)
            transform.position = targetPosition;
    }
}