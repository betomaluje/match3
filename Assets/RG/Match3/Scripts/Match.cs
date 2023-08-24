using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Match
{
    private static readonly int DEFAULT_MINIUM = 3;

    private readonly int _minimumAmount = DEFAULT_MINIUM;

    private readonly Dictionary<TileType, HashSet<Tile>> _tiles = new Dictionary<TileType, HashSet<Tile>>();

    public Match()
    {
    }

    public Match(int minimumAmount)
    {
        _minimumAmount = minimumAmount;
    }

    public List<List<Vector3Int>> CheckRow(List<Tile> row)
    {
        if (row.Count == 0) return null;

        var matchCounter = 0;
        for (var x = 1; x < row.Count; x++)
        {
            var prev = row[x - 1];
            var tile = row[x];

            if (prev.Type == tile.Type)
            {
                matchCounter++;
                ConsoleDebug.Instance.Log($"{tile.Type} -> {matchCounter}");
                // we need to save both
                _tiles[prev.Type] = AddToDictionary(prev);
                _tiles[tile.Type] = AddToDictionary(tile);
            }
            else
            {
                matchCounter = 1;
            }
        }

        return _tiles.Where(t => t.Value.Count >= _minimumAmount)
            .Select(t => t.Value.Select(t2 => t2.TileKey).ToList())
            .Distinct()
            .ToList();
    }

    private HashSet<Tile> AddToDictionary(Tile tile)
    {
        if (!_tiles.TryGetValue(tile.Type, out var blockTiles)) blockTiles = new HashSet<Tile>();

        if (blockTiles.Count > 0)
        {
            var last = blockTiles.Last();
            var diff = Mathf.Abs(last.TileKey.x - tile.TileKey.x) == 1;
            if (diff) blockTiles.Add(tile);
        }

        blockTiles.Add(tile);
        return blockTiles;
    }
}