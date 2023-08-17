using System.Collections;
using RG.Match3.Scripts;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private TileType _type;
    [SerializeField] private Transform _destroyFX;
    [SerializeField] private float _timeToMove = .2f;

    private GridManager _manager;

    private Vector3Int _tileKey;

    private void Awake()
    {
        _manager = FindObjectOfType<GridManager>();
    }

    private void Start()
    {
        _tileKey = Vector3Int.FloorToInt(transform.position);
    }

    public void OnMouseDown()
    {
        if (_destroyFX != null) Instantiate(_destroyFX, transform.position, Quaternion.identity);

        // we will use it's position to detect row and column
        _manager.OnTileDestroyed(_tileKey);
        Destroy(gameObject);
    }

    public void MoveDown()
    {
        var targetPosition = Vector3Int.FloorToInt(transform.position);
        targetPosition.y -= 1;

        StartCoroutine(MoveTile(targetPosition));
    }

    private IEnumerator MoveTile(Vector3 targetPosition)
    {
        var elapsedTime = 0f;
        var startPosition = transform.position;

        while (elapsedTime < _timeToMove)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / _timeToMove);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
    }
}