using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Match
{
    private static readonly int DEFAULT_MINIUM = 3;

    private readonly int _minimumAmount = DEFAULT_MINIUM;

    private readonly Dictionary<TileType, List<Tile>> _tiles = new Dictionary<TileType, List<Tile>>();

    public Match()
    {
    }

    public Match(int minimumAmount)
    {
        _minimumAmount = minimumAmount;
    }

    public void AddAll(List<Tile> tiles)
    {
        _tiles.Clear();

        foreach (var tile in tiles) AddToList(tile);
    }

    private void AddToList(Tile tile)
    {
        if (!_tiles.TryGetValue(tile.Type, out var blockTiles)) blockTiles = new List<Tile>();

        if (blockTiles.Count > 0)
        {
            // we need to search any inside the current list that has 1 unit difference
            var isNeighbour = blockTiles.Any(t => Mathf.Abs(t.TileKey.x - tile.TileKey.x) == 1);
            if (isNeighbour)
                blockTiles.Add(tile);
        }
        else
        {
            blockTiles.Add(tile);
        }

        _tiles[tile.Type] = blockTiles;
    }

    private static List<Tile> GetConsecutiveSameElementList(List<Tile> list, TileType targetElement,
        int consecutiveCount)
    {
        var result = new List<Tile>();

        for (var i = 0; i <= list.Count - consecutiveCount; i++)
        {
            var sublist = list.Skip(i).Take(consecutiveCount).ToList();
            if (sublist.All(item => item.Type == targetElement)) result.AddRange(sublist);
        }

        return result;
    }

    public List<List<Tile>> CheckManually()
    {
        var result = new List<List<Tile>>();
        foreach (var keys in _tiles.Keys)
        {
            var list = GetConsecutiveSameElementList(_tiles[keys], keys, _minimumAmount);
            if (list.Count > 0)
                result.Add(list);
        }

        return result;
    }

    public List<List<Tile>> CheckWithLinq()
    {
        return _tiles.Where(t => t.Value.Count >= _minimumAmount)
            .Select(a => a.Value)
            .ToList();
    }
}