using System.Collections;
using RG.Match3.Scripts;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private TileType _type = TileType.None;
    [SerializeField] private Transform _destroyFX;
    [SerializeField] private float _timeToMove = .2f;

    private GridManager _manager;

    public Vector3Int TileKey { get; private set; }
    public TileType Type => _type;

    private void Awake()
    {
        _manager = FindObjectOfType<GridManager>();
    }

    private void Start()
    {
        TileKey = Vector3Int.FloorToInt(transform.position);
    }

    public void OnMouseDown()
    {
        // we will use it's position to detect row and column
        _manager.OnTileDestroyed(TileKey);
        DestroyTile();
    }

    public void DestroyTile()
    {
        if (_destroyFX != null) Instantiate(_destroyFX, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    public void MoveDown()
    {
        var targetPosition = Vector3Int.FloorToInt(transform.position);
        targetPosition.y -= 1;
        // we update our key
        TileKey = targetPosition;

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