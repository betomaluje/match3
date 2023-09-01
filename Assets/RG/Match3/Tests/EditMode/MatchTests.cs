using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Matches;
using NUnit.Framework;
using Tiles;
using UnityEngine;

public class MatchTests {
    private MockTiles _mockTiles;

    [Test]
    public void DefaultMinimum() {
        var match = new Match();
        Assert.AreEqual(Match.DEFAULT_MINIUM, match.MinimumAmount);
    }

    [Test]
    public void CustomMinimum() {
        const int minimum = 5;
        var match = new Match(minimum);
        Assert.AreEqual(minimum, match.MinimumAmount);
    }

    [Test]
    public void CheckRow() {
        _mockTiles = new MockTiles();
        _mockTiles.Init();

        var match = new Match();

        var tiles = _mockTiles.Tiles_Example1();

        var row = GetRow(tiles, 0);
        var matches = match.CheckRow(row).GetAwaiter().GetResult();

        Assert.IsTrue(matches.Count == 1);

        var match2 = new Match(_mockTiles.Width + 1);

        var row2 = GetRow(tiles, 0);
        var matches2 = match2.CheckRow(row2).GetAwaiter().GetResult();

        Assert.IsTrue(matches2.Count == 0);
    }

    private static List<Tile> GetRow(ConcurrentDictionary<Vector3Int, Tile> tiles, int y) {
        return tiles.Where(pos => pos.Key.y == y)
            .Select(p => p.Value)
            .ToList();
    }
}