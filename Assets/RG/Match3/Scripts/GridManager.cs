using System;
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
    [Tooltip("The number of rows it will have. Minimum 1")]
    private int _width = 5;

    [SerializeField]
    [Min(1)]
    [Tooltip("The number of columns it will have. Minimum 1")]
    private int _height = 5;

    [SerializeField]
    [Min(3)]
    [Tooltip("How many similar items to search for. Minimum 3")]
    private int _minPerRow = 3;

    [SerializeField]
    [Tooltip(
        "How to check for matches. Only row searches only the clicked row (as in the video). " +
        "Whole column searches for each row in that column (slower and sometimes buggy)")]
    private CheckMode _checkMode = CheckMode.OnlyRow;

    [Header("Tiles")]
    [SerializeField]
    [Tooltip("The possible Tiles to populate the game")]
    private Tile[] _tilePrefabs;

    [SerializeField]
    [Tooltip("The transform were to add the tiles")]
    private Transform _tileContainer;

    [Header("Camera")]
    [SerializeField]
    [Tooltip("Just to try and fix the camera to look at all the tiles")]
    private Transform _camera;

    private bool _isBusy;
    private ConcurrentDictionary<Vector3Int, Tile> _tiles;

    public Action OnTileDestroyed = delegate { };

    private void Start()
    {
        SetupGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) SetupGame();
    }

    #region Debug

    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        var size = Vector2.one;
        foreach (var point in EvaluateGridPoints()) Gizmos.DrawWireCube(point, size);
    }

    #endregion

    private void OnValidate()
    {
        if (_tileContainer == null)
        {
            var container = new GameObject("Tiles Container");
            _tileContainer = container.transform;
        }

        if (_camera == null && Camera.main != null) _camera = Camera.main.transform;
    }

    /// <summary>
    ///     Sets up a new game. Removes every tile and generates a new tile set
    /// </summary>
    private void SetupGame()
    {
        _tiles = new ConcurrentDictionary<Vector3Int, Tile>();

        _tileContainer.Clear();

        foreach (var point in EvaluateGridPoints())
        {
            var tilePrefab = _tilePrefabs[Random.Range(0, _tilePrefabs.Length)];
            var tile = Instantiate(tilePrefab, point, Quaternion.identity);
            tile.name = $"Tile ({point.x} {point.y})";
            tile.transform.SetParent(_tileContainer);

            _tiles.TryAdd(point, tile);
        }

        _isBusy = false;

        // Setup camera
        _camera.position = new Vector3(_width / 2f - .5f, _height / 2f - .5f, -10);
    }

    /// <summary>
    ///     Gets all the Tiles that are only above a certain "y" height and in a certain column for a given "x" position
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
    ///     Gets all the Tiles in a row for a given "y" position
    /// </summary>
    /// <param name="y">The row number</param>
    /// <returns>A dictionary for all the available tiles in that row</returns>
    private List<Tile> GetRow(int y)
    {
        return _tiles.Where(pos => pos.Value != null && pos.Value.TileKey.y == y)
            .Select(p => p.Value)
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
        OnTileDestroyed?.Invoke();
        DestroyTile(tilePosition);
    }

    /// <summary>
    ///     Destroys a single tile given it's position. When a tile is destroyed, all the above tiles move down,
    ///     we check for potential matches and finally we delete the matches and move every column with missing
    ///     tiles down
    /// </summary>
    /// <param name="tilePosition">The tile's position to be destroyed</param>
    private async void DestroyTile(Vector3Int tilePosition)
    {
        _isBusy = true;
        // 1. we remove it from the main list
        if (_tiles.TryGetValue(tilePosition, out var t))
        {
            t.DestroyTile();
        }
        else
        {
            _isBusy = false;
            return;
        }

        var stopwatch = new Stopwatch();

        // 2. move tiles 1 down and update in our list
        stopwatch.Start();
        await MoveColumnDown(tilePosition);
        stopwatch.Stop();
        ConsoleDebug.Instance.Log($"Move took {stopwatch.ElapsedMilliseconds} ms");

        // 3. after moving, check every row if there's any match
        stopwatch.Start();
        var matches = await GetMatches(tilePosition);
        stopwatch.Stop();
        ConsoleDebug.Instance.Log($"Get matches took {stopwatch.ElapsedMilliseconds} ms -> {matches.Count} in total");

        // 4. if match, remove all those tiles
        if (matches.Count > 0)
            DestroyTiles(matches);
        else
            _isBusy = false;
    }

    /// <summary>
    ///     Destroys a list of tiles given their positions. When a tile is destroyed, all the above tiles move down,
    ///     we check for potential matches and finally we delete the matches and move every column with missing
    ///     tiles down
    /// </summary>
    /// <param name="tilePositions">A list of tile positions to be destroyed</param>
    private async void DestroyTiles(List<Vector3Int> tilePositions)
    {
        _isBusy = true;

        OnTileDestroyed?.Invoke();

        var stopwatch = new Stopwatch();

        stopwatch.Start();
        var tasks = new List<Task>();
        foreach (var tilePosition in tilePositions)
        {
            if (_tiles.TryGetValue(tilePosition, out var t))
                t.DestroyTile();

            tasks.Add(MoveColumnDown(tilePosition));
        }

        await Task.WhenAll(tasks);

        stopwatch.Stop();
        ConsoleDebug.Instance.Log($"Move ALL took {stopwatch.ElapsedMilliseconds} ms");

        _isBusy = false;
    }

    /// <summary>
    ///     Searches every tile above and moves them down by 1 unit
    /// </summary>
    /// <param name="tilePosition">The initial position to start moving Tiles above</param>
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

            if (_tiles.TryGetValue(previousPosition, out var t)) await t.MoveDown(targetPosition);

            if (_tiles.ContainsKey(targetPosition) && _tiles.ContainsKey(previousPosition))
                _tiles[targetPosition] = _tiles[previousPosition];
        }

        _tiles.TryRemove(lastKey, out var removed);
    }

    /// <summary>
    ///     Gets a list of positions that are matches for the given initial tilePosition
    /// </summary>
    /// <param name="tilePosition">The initial position to search for matches</param>
    /// <returns></returns>
    private async Task<List<Vector3Int>> GetMatches(Vector3Int tilePosition)
    {
        if (_checkMode == CheckMode.WholeColumn)
        {
            var toCheck = new List<Vector3Int>();
            for (var y = tilePosition.y; y < _height; y++)
            {
                var rowMatches = await GetTilesToBeRemoved(y);
                toCheck.AddRange(rowMatches);
            }

            return await Task.FromResult(toCheck);
        }

        return await GetTilesToBeRemoved(tilePosition.y);
    }

    /// <summary>
    ///     Gets a list of positions within a row to be removed (a.k.a that have a match)
    /// </summary>
    /// <param name="y">The row position</param>
    /// <returns>A list of positions within a row of tiles that match</returns>
    private async Task<List<Vector3Int>> GetTilesToBeRemoved(int y)
    {
        // we get the filtered row for the same tile type
        var sameRow = GetRow(y);

        ConsoleDebug.Instance.Log($"Check horizontal on {y}");

        var match = new Match(_minPerRow);
        var matches = match.CheckRow(sameRow);

        if (matches == null) return await Task.FromResult(new List<Vector3Int>(0));

        return await Task.FromResult(matches);
    }

    private IEnumerable<Vector3Int> EvaluateGridPoints()
    {
        for (var x = 0; x < _width; x++)
        for (var y = 0; y < _height; y++)
            yield return new Vector3Int(x, y, 0);
    }
}