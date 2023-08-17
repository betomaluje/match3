using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int _width = 5;
    [SerializeField] private int _height = 5;
    [SerializeField] private Tile[] _tilePrefabs;
    [SerializeField] private Transform _tileContainer;

    [Header("Camera")] [SerializeField] private Transform _camera;

    [Header("Debug")] [SerializeField] private bool _debug = true;

    private Dictionary<Vector3Int, Tile> _tiles;

    private void Start()
    {
        _tiles = new Dictionary<Vector3Int, Tile>();
        foreach (var point in EvaluateGridPoints())
        {
            var tilePrefab = _tilePrefabs[Random.Range(0, _tilePrefabs.Length)];
            var tile = Instantiate(tilePrefab, point, Quaternion.identity);
            tile.name = $"Tile ({point.x} {point.y})";
            tile.transform.SetParent(_tileContainer);

            _tiles.Add(point, tile);
        }

        _camera.position = new Vector3(_width / 2f - .5f, _height / 2f - .5f, -10);
    }

    #region Debug

    private void OnDrawGizmos()
    {
        if (!_debug || Application.isPlaying) return;
        var size = Vector2.one;
        foreach (var point in EvaluateGridPoints())
            Gizmos.DrawWireCube(point, size);
    }

    #endregion

    /// <summary>
    ///     Get all the Tiles that are above a certain "y" height and in a certain column for a given "x" position
    /// </summary>
    /// <param name="x">The column number</param>
    /// <param name="y">The row number to search above</param>
    /// <returns>A dictionary for all the available tiles in that column</returns>
    private Dictionary<Vector3Int, Tile> GetVerticalTiles(int x, int y)
    {
        return _tiles.Where(pos => pos.Key.x == x && pos.Key.y >= y)
            .ToDictionary(p => p.Key, p => p.Value);
    }

    /// <summary>
    ///     Get all the row for a given "y" position
    /// </summary>
    /// <param name="y">The row number</param>
    /// <returns>A dictionary for all the available tiles in that row</returns>
    private Dictionary<Vector3Int, Tile> GetHorizontalTiles(int y)
    {
        return _tiles.Where(pos => pos.Key.y == y)
            .ToDictionary(p => p.Key, p => p.Value);
    }

    public void OnTileDestroyed(Vector3Int tilePosition)
    {
        if (_debug)
            Debug.Log($"clicked ({tilePosition.x}, {tilePosition.y})");

        _tiles.Remove(tilePosition);

        // get all the column to move down
        var verticalAbove = GetVerticalTiles(tilePosition.x, tilePosition.y);
        foreach (var tile in verticalAbove)
            if (tile.Key.y > tilePosition.y)
                tile.Value.MoveDown();

        // check horizontal for matches
    }

    private IEnumerable<Vector3Int> EvaluateGridPoints()
    {
        for (var x = 0; x < _width; x++)
        for (var y = 0; y < _height; y++)
            yield return new Vector3Int(x, y, 0);
    }
}