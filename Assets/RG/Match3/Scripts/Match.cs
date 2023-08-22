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

    public void AddToList(Tile tile)
    {
        var list = new List<Tile>();
        if (_tiles.TryGetValue(tile.Type, out var previousList))
            list = previousList;

        if (list.Count > 0)
        {
            var last = list.Last();
            if (Mathf.Abs(last.TileKey.x - tile.TileKey.x) == 1)
                list.Add(tile);
        }
        else
        {
            list.Add(tile);
        }

        _tiles[tile.Type] = list;
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

    public List<List<Tile>> Check()
    {
        var result = new List<List<Tile>>();
        foreach (var keys in _tiles.Keys)
        {
            var list = GetConsecutiveSameElementList(_tiles[keys], keys, _minimumAmount);
            if (list.Count > 0)
            {
                ConsoleDebug.Instance.Log(list, "Check");
                result.Add(list);
            }
        }

        return result;
    }

    public IEnumerable<List<Tile>> Check2()
    {
        return _tiles.Where(t => t.Value.Count >= _minimumAmount)
            .Select(a => a.Value);
    }
}