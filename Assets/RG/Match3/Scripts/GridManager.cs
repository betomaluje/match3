using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Rules")]
    [SerializeField]
    [Min(1)]
    private int _width = 5;

    [SerializeField]
    [Min(1)]
    private int _height = 5;

    [SerializeField]
    [Min(3)]
    private int _minPerRow = 3;

    [SerializeField]
    private CheckMode _checkMode = CheckMode.LINQ;

    [Header("Tiles")]
    [SerializeField]
    private Tile[] _tilePrefabs;

    [SerializeField]
    private Transform _tileContainer;

    public Dictionary<Vector3Int, Tile> Tiles { get; private set; }

    private void Start()
    {
        Tiles = new Dictionary<Vector3Int, Tile>();
        foreach (var point in EvaluateGridPoints())
        {
            var tilePrefab = _tilePrefabs[Random.Range(0, _tilePrefabs.Length)];
            var tile = Instantiate(tilePrefab, point, Quaternion.identity);
            tile.name = $"Tile ({point.x} {point.y})";
            tile.transform.SetParent(_tileContainer);

            Tiles.Add(point, tile);
        }

        _camera.position = new Vector3(_width / 2f - .5f, _height / 2f - .5f, -10);
    }

    #region Debug

    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        var size = Vector2.one;
        foreach (var point in EvaluateGridPoints()) Gizmos.DrawWireCube(point, size);
    }

    #endregion

    /// <summary>
    ///     Get all the Tiles that are equal or above a certain "y" height and in a certain column for a given "x" position
    /// </summary>
    /// <param name="x">The column number</param>
    /// <param name="y">The row number to start searching above</param>
    /// <returns>A dictionary for all the available tiles in that column</returns>
    private Dictionary<Vector3Int, Tile> GetColumn(int x, int y)
    {
        return Tiles.Where(pos =>
                pos.Value != null && pos.Value.TileKey.x == x && pos.Value.TileKey.y >= y)
            .ToDictionary(p => p.Key, p => p.Value);
    }

    /// <summary>
    ///     Get all the Tiles that are only above a certain "y" height and in a certain column for a given "x" position
    /// </summary>
    /// <param name="x">The column number</param>
    /// <param name="y">The row number to start searching above</param>
    /// <returns>A dictionary for all the available tiles in that column</returns>
    private Dictionary<Vector3Int, Tile> GetAboveTiles(int x, int y)
    {
        return Tiles.Where(pos =>
                pos.Value != null && pos.Value.TileKey.x == x && pos.Value.TileKey.y > y)
            .ToDictionary(p => p.Key, p => p.Value);
    }

    /// <summary>
    ///     Get all the row for a given "y" position
    /// </summary>
    /// <param name="y">The row number</param>
    /// <returns>A dictionary for all the available tiles in that row</returns>
    private List<Tile> GetHorizontalTiles(int y)
    {
        return Tiles.Where(pos => pos.Value != null && pos.Value.TileKey.y == y)
            .Select(p => p.Value)
            .OrderBy(p => p.TileKey.x)
            .ToList();
    }

    public async void OnTileDestroyed(Vector3Int tilePosition)
    {
        // 1. we remove it from the main list
        if (Tiles.TryGetValue(tilePosition, out var t))
        {
            t.DestroyTile();
            Tiles.Remove(tilePosition);
        }

        // 2. move tiles 1 down and update in our list
        await MoveColumnDown(tilePosition);

        // 3. after moving, check every row if there's any match
        var matches = await CheckHorizontal(tilePosition);

        // 4. if match, remove all those tiles
        // 5. repeat
        foreach (var match in matches)
        {
            ConsoleDebug.Instance.Log($"Should destroy {match}");
            OnTileDestroyed(match);
        }
    }

    private async Task MoveColumnDown(Vector3Int tilePosition)
    {
        var verticalAbove = GetAboveTiles(tilePosition.x, tilePosition.y);

        foreach (var tile in verticalAbove)
        {
            var previousPosition = tile.Value.TileKey;
            if (Tiles.ContainsKey(previousPosition))
                Tiles.Remove(previousPosition);

            var newPosition = await tile.Value.MoveDown();

            Tiles[newPosition] = tile.Value;
        }
    }

    private async Task<List<Vector3Int>> CheckHorizontal(Vector3Int tilePosition)
    {
        var column = GetColumn(tilePosition.x, tilePosition.y);

        var toCheck = new List<Vector3Int>();
        foreach (var tile in column)
        {
            var a = CheckHorizontalMatches(tile.Key);
            toCheck.AddRange(a);
        }

        return await Task.FromResult(toCheck);
    }

    private IEnumerable<Vector3Int> CheckHorizontalMatches(Vector3Int tilePosition)
    {
        ConsoleDebug.Instance.Log($"Check horizontal on {tilePosition.y}");

        // we get the filtered row for the same tile type
        var sameRow = GetHorizontalTiles(tilePosition.y);

        var match = new Match(_minPerRow);
        match.AddAll(sameRow);

        var matches = _checkMode == CheckMode.LINQ ? match.CheckWithLinq() : match.CheckManually();

        return (from m in matches from tile in m select tile.TileKey).ToList();
    }

    private IEnumerable<Vector3Int> EvaluateGridPoints()
    {
        for (var x = 0; x < _width; x++)
        for (var y = 0; y < _height; y++)
            yield return new Vector3Int(x, y, 0);
    }
}