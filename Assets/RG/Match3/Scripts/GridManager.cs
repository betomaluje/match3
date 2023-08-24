using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

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
    private CheckMode _checkMode = CheckMode.OnlyRow;

    [Header("Tiles")]
    [SerializeField]
    private Tile[] _tilePrefabs;

    [SerializeField]
    private Transform _tileContainer;

    [Header("Camera")]
    [SerializeField]
    private Transform _camera;

    private bool _isBusy;

    private ConcurrentDictionary<Vector3Int, Tile> _tiles;

    private void Start()
    {
        _tiles = new ConcurrentDictionary<Vector3Int, Tile>();
        foreach (var point in EvaluateGridPoints())
        {
            var tilePrefab = _tilePrefabs[Random.Range(0, _tilePrefabs.Length)];
            var tile = Instantiate(tilePrefab, point, Quaternion.identity);
            tile.name = $"Tile ({point.x} {point.y})";
            tile.transform.SetParent(_tileContainer);

            _tiles.TryAdd(point, tile);
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
    private List<Vector3Int> GetColumn(int x, int y)
    {
        return _tiles.Where(pos =>
                pos.Value != null && pos.Value.TileKey.x == x && pos.Value.TileKey.y >= y)
            .Select(p => p.Key)
            .OrderBy(p => p.y)
            .ToList();
    }

    /// <summary>
    ///     Get all the Tiles that are only above a certain "y" height and in a certain column for a given "x" position
    /// </summary>
    /// <param name="x">The column number</param>
    /// <param name="y">The row number to start searching above</param>
    /// <returns>A dictionary for all the available tiles in that column</returns>
    private List<Tile> GetAboveTiles(int x, int y)
    {
        return _tiles.Where(pos =>
                pos.Value != null && pos.Value.TileKey.x == x && pos.Value.TileKey.y > y)
            .Select(p => p.Value)
            .OrderBy(p => p.TileKey.y)
            .ToList();
    }

    /// <summary>
    ///     Get all the row for a given "y" position
    /// </summary>
    /// <param name="y">The row number</param>
    /// <returns>A dictionary for all the available tiles in that row</returns>
    private List<Tile> GetRow(int y)
    {
        return _tiles.Where(pos => pos.Value != null && pos.Value.TileKey.y == y)
            .Select(p => p.Value)
            .OrderBy(p => p.TileKey.x)
            .ToList();
    }

    /// <summary>
    ///     Called when a Tile is clicked
    /// </summary>
    /// <param name="tilePosition">The current tile position</param>
    public void OnTileClicked(Vector3Int tilePosition)
    {
        if (_isBusy) return;
        ConsoleDebug.Instance.Log($"Clicked {tilePosition}");
        DestroyTile(tilePosition);
    }

    private async void DestroyTile(Vector3Int tilePosition)
    {
        _isBusy = true;
        // 1. we remove it from the main list
        if (_tiles.TryGetValue(tilePosition, out var t))
            t.DestroyTile();

        var stopwatch = new Stopwatch();

        // 2. move tiles 1 down and update in our list
        stopwatch.Start();
        await MoveColumnDown(tilePosition);
        stopwatch.Stop();
        ConsoleDebug.Instance.Log($"Move took {stopwatch.ElapsedMilliseconds} ms");

        // 3. after moving, check every row if there's any match
        stopwatch.Start();
        var matches = await GetMatches(tilePosition);

        // 4. if match, remove all those tiles
        matches = matches.Distinct().ToList();

        stopwatch.Stop();
        ConsoleDebug.Instance.Log($"Get matches took {stopwatch.ElapsedMilliseconds} ms -> {matches.Count} in total");

        if (matches.Count > 0)
            DestroyTiles(matches);
        else
            _isBusy = false;
    }

    private async void DestroyTiles(List<Vector3Int> tilePositions)
    {
        _isBusy = true;

        var stopwatch = new Stopwatch();

        stopwatch.Start();
        var tasks = new List<Task>();
        foreach (var tilePosition in tilePositions)
        {
            if (_tiles.TryGetValue(tilePosition, out var t))
                t.DestroyTile();

            // 2. move tiles 1 down and update in our list
            tasks.Add(MoveColumnDown(tilePosition));
        }

        await Task.WhenAll(tasks);

        stopwatch.Stop();
        ConsoleDebug.Instance.Log($"Move ALL took {stopwatch.ElapsedMilliseconds} ms");

        _isBusy = false;
    }

    private async Task MoveColumnDown(Vector3Int tilePosition)
    {
        var verticalAbove = GetAboveTiles(tilePosition.x, tilePosition.y);

        if (verticalAbove.Count == 0) return;

        var lastKey = verticalAbove.Last().TileKey;

        foreach (var tile in verticalAbove)
        {
            var previousPosition = tile.TileKey;

            var targetPosition = previousPosition;
            targetPosition += Vector3Int.down;
            // tile.name = $"Tile ({targetPosition.x} {targetPosition.y})";

            if (_tiles.TryGetValue(previousPosition, out var t)) await t.MoveDown(targetPosition);

            if (_tiles.ContainsKey(targetPosition) && _tiles.ContainsKey(previousPosition))
                _tiles[targetPosition] = _tiles[previousPosition];
        }

        _tiles.TryRemove(lastKey, out var removed);
    }

    private async Task<List<Vector3Int>> GetMatches(Vector3Int tilePosition)
    {
        if (_checkMode == CheckMode.WholeColumn)
        {
            var column = GetColumn(tilePosition.x, tilePosition.y);

            var toCheck = new List<Vector3Int>();
            foreach (var position in column)
            {
                var rowMatches = await GetTilesToBeRemoved(position);
                toCheck.AddRange(rowMatches);
            }

            return await Task.FromResult(toCheck);
        }
        else
        {
            var toCheck = new List<Vector3Int>();
            var rowMatches = await GetTilesToBeRemoved(tilePosition);
            toCheck.AddRange(rowMatches);
            return await Task.FromResult(toCheck);
        }
    }

    private async Task<List<Vector3Int>> GetTilesToBeRemoved(Vector3Int tilePosition)
    {
        // we get the filtered row for the same tile type
        var sameRow = GetRow(tilePosition.y);

        ConsoleDebug.Instance.Log($"Check horizontal on {tilePosition.y}");

        var match = new Match(_minPerRow);
        var matches = match.CheckRow(sameRow);

        if (matches == null) return await Task.FromResult(new List<Vector3Int>(0));

        // var list = (from m in matches from tile in m select tile.TileKey).ToList();

        var list = matches.SelectMany(listOfMatches => listOfMatches).ToList();

        return await Task.FromResult(list);
    }

    private IEnumerable<Vector3Int> EvaluateGridPoints()
    {
        for (var x = 0; x < _width; x++)
        for (var y = 0; y < _height; y++)
            yield return new Vector3Int(x, y, 0);
    }
}