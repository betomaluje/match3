using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Extensions;
using Matches;
using Tiles;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour {
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

    [SerializeField]
    private Transform _background;

    [SerializeField]
    private float _padding = 1f;

    private bool _isBusy;
    private ConcurrentDictionary<Vector3Int, Tile> _tiles;

    public Action OnTileDestroyed = delegate { };

    private void Start() {
        SetupGame();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            SetupGame();
        }
    }

    #region Debug

    private void OnDrawGizmos() {
        if (Application.isPlaying) {
            return;
        }

        var size = Vector2.one;
        var points = EvaluateGridPoints().ToList();
        for (var i = 0; i < points.Count(); i++) {
            Gizmos.DrawWireCube(points[i], size);
        }
    }

    #endregion

    private void OnValidate() {
        if (_tileContainer == null) {
            var container = new GameObject("Tiles Container");
            _tileContainer = container.transform;
        }

        if (_camera == null && Camera.main != null) {
            _camera = Camera.main.transform;
        }
    }

    /// <summary>
    ///     Sets up a new game. Removes every tile and generates a new tile set
    /// </summary>
    private void SetupGame() {
        _tiles = new ConcurrentDictionary<Vector3Int, Tile>();

        _tileContainer.Clear();

        var points = EvaluateGridPoints().ToList();
        var len = points.Count;
        for (var i = 0; i < len; i++) {
            var point = points[i];
            var tilePrefab = _tilePrefabs[Random.Range(0, _tilePrefabs.Length)];
            var tile = Instantiate(tilePrefab, point, Quaternion.identity);
            tile.name = $"Tile ({point.x} {point.y})";
            tile.transform.SetParent(_tileContainer);

            _tiles.TryAdd(point, tile);
        }

        _isBusy = false;

        var centerPosition = new Vector3(_width / 2f - .5f, _height / 2f - .5f, 0);
        _background.position = centerPosition;

        var scale = new Vector3(_width + _padding, _height + _padding, .1f);
        _background.localScale = scale;

        // Setup camera
        centerPosition.z = -10;
        _camera.position = centerPosition;
    }

    /// <summary>
    ///     Gets all the Tiles that are only above a certain "y" height and in a certain column for a given "x" position
    /// </summary>
    /// <param name="x">The column number</param>
    /// <param name="y">The row number to start searching above</param>
    /// <returns>A dictionary for all the available tiles in that column</returns>
    private List<Tile> GetAboveTiles(int x, int y) {
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
    private async Task<List<Tile>> GetRow(int y) {
        return await Task.FromResult(_tiles.Where(pos => pos.Value != null && pos.Value.TileKey.y == y)
            .Select(p => p.Value)
            .ToList());
    }

    /// <summary>
    ///     Called when a Tile is clicked
    /// </summary>
    /// <param name="tilePosition">The current tile position</param>
    public void OnTileClicked(Vector3Int tilePosition) {
        if (_isBusy) {
            return;
        }

        ConsoleDebug.Instance.Log($"Clicked {tilePosition}");

        DestroyTile(tilePosition);
    }

    /// <summary>
    ///     Destroys a single tile given it's position. When a tile is destroyed, all the above tiles move down,
    ///     we check for potential matches and finally we delete the matches and move every column with missing
    ///     tiles down
    /// </summary>
    /// <param name="tilePosition">The tile's position to be destroyed</param>
    private async void DestroyTile(Vector3Int tilePosition) {
        _isBusy = true;

        OnTileDestroyed?.Invoke();

        // 1. we remove it from the main list
        if (_tiles.TryGetValue(tilePosition, out var currentTile)) {
            currentTile.DestroyTile();
        }
        else {
            _isBusy = false;
            return;
        }

        await MoveColumnDown(tilePosition);

        await Matches(tilePosition);
    }

    private async Task Matches(Vector3Int tilePosition) {
        ConsoleDebug.Instance.Log($"Checking {tilePosition.y}");

        Assert.IsTrue(tilePosition.y >= 0);

        for (var y = tilePosition.y; y < _height; y++) {
            var matches = await GetMatches(y);
            ConsoleDebug.Instance.Log($"    matches: {matches.Count}");

            var matchCount = matches.Count;

            if (matchCount <= 0) continue;

            OnTileDestroyed?.Invoke();

            var tasks = new List<Task>(matchCount);
            for (var i = 0; i < matchCount; i++) {
                var match = matches[i];
                if (!_tiles.TryGetValue(match, out var t)) {
                    continue;
                }

                t.DestroyTile();
                tasks.Add(MoveColumnDown(match));
            }

            Assert.AreEqual(matchCount, tasks.Count);
            await Task.WhenAll(tasks);

            y--;
        }

        ConsoleDebug.Instance.Log("Done! No more matches");
        _isBusy = false;
    }

    /// <summary>
    ///     Searches every tile above and moves them down by 1 unit
    /// </summary>
    /// <param name="tilePosition">The initial position to start moving Tiles above</param>
    private async Task MoveColumnDown(Vector3Int tilePosition) {
        var verticalAbove = GetAboveTiles(tilePosition.x, tilePosition.y);

        var len = verticalAbove.Count;

        if (len == 0) {
            return;
        }

        var lastKey = verticalAbove.Last().TileKey;

        for (var i = 0; i < len; i++) {
            var tile = verticalAbove[i];
            var previousPosition = tile.TileKey;

            var targetPosition = previousPosition;
            targetPosition += Vector3Int.down;

            if (_tiles.TryGetValue(previousPosition, out var t)) {
                await t.MoveDown(targetPosition);
            }

            if (_tiles.ContainsKey(targetPosition) && _tiles.ContainsKey(previousPosition)) {
                _tiles[targetPosition] = _tiles[previousPosition];
            }
        }

        _tiles.TryRemove(lastKey, out var removed);
    }

    /// <summary>
    ///     Gets a list of positions that are matches for the given initial tilePosition
    /// </summary>
    /// <param name="y">The row position</param>
    /// <returns></returns>
    private async Task<List<Vector3Int>> GetMatches(int y) {
        // we get the filtered row for the same tile type
        var sameRow = await GetRow(y);

        ConsoleDebug.Instance.Log($"Check row {y}");

        var match = new Match(_minPerRow);
        var matches = await match.CheckRow(sameRow);

        if (matches == null) {
            return await Task.FromResult(new List<Vector3Int>(0));
        }

        return matches;
    }

    private IEnumerable<Vector3Int> EvaluateGridPoints() {
        for (var x = 0; x < _width; x++) {
            for (var y = 0; y < _height; y++) {
                yield return new Vector3Int(x, y, 0);
            }
        }
    }
}