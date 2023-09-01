using NUnit.Framework;
using Tiles;
using UnityEngine;

public class MockTilesTest {
    private MockTiles _mockTiles;

    [Test]
    public void CheckTilesSize() {
        _mockTiles = new MockTiles();
        _mockTiles.Init();

        var tiles = _mockTiles.Tiles_Example1();

        Assert.IsFalse(tiles.IsEmpty);

        Assert.AreEqual(_mockTiles.Width * _mockTiles.Height, tiles.Count);
    }

    [Test]
    public void CheckTileTypes() {
        _mockTiles = new MockTiles();
        _mockTiles.Init();

        var tiles = _mockTiles.Tiles_Example1();

        for (var x = 0; x < _mockTiles.Width; x++) {
            for (var y = 0; y < _mockTiles.Height; y++) {
                var point = new Vector3Int(x, y, 0);
                var tile = tiles[point];

                Assert.AreEqual(y % 3 == 0 ? TileType.Green : TileType.White, tile.Type);
            }
        }
    }

    [Test]
    public void CheckSomeTileTypes() {
        _mockTiles = new MockTiles();
        _mockTiles.Init();

        var tiles = _mockTiles.Tiles_Example1();

        var greenTile1 = tiles[new Vector3Int(1, 0, 0)];
        var greenTile2 = tiles[new Vector3Int(0, 3, 0)];
        var whiteTile1 = tiles[new Vector3Int(1, 4, 0)];
        var whiteTile2 = tiles[new Vector3Int(5, 5, 0)];

        Assert.IsTrue(greenTile1.Type == TileType.Green);
        Assert.IsTrue(greenTile2.Type == TileType.Green);
        Assert.IsTrue(whiteTile1.Type == TileType.White);
        Assert.IsTrue(whiteTile2.Type == TileType.White);
    }

    [Test]
    public void CheckTryGetValues() {
        _mockTiles = new MockTiles();
        _mockTiles.Init();

        var tiles = _mockTiles.Tiles_Example1();

        var point1 = new Vector3Int(0, 1, 0);
        var shouldBeGreenTile = tiles.TryGetValue(point1, out var greenTile1);
        Assert.IsTrue(shouldBeGreenTile);
        Assert.NotNull(greenTile1);

        var point2 = new Vector3Int(0, _mockTiles.Height, 0);
        var shouldBeNullTile = tiles.TryGetValue(point2, out var nullTile);
        Assert.False(shouldBeNullTile);
        Assert.Null(nullTile);
    }

    [Test]
    public void CheckTryRemoveValues() {
        _mockTiles = new MockTiles();
        _mockTiles.Init();

        var tiles = _mockTiles.Tiles_Example1();

        var point1 = new Vector3Int(0, 1, 0);
        var shouldBeGreenTile = tiles.TryRemove(point1, out var greenTile1);
        Assert.IsTrue(shouldBeGreenTile);
        Assert.NotNull(greenTile1);

        var point2 = new Vector3Int(0, _mockTiles.Height, 0);
        var shouldBeNullTile = tiles.TryRemove(point2, out var nullTile);
        Assert.False(shouldBeNullTile);
        Assert.Null(nullTile);
    }

    [Test]
    public void CheckTryAddValues() {
        _mockTiles = new MockTiles();
        _mockTiles.Init();

        var tiles = _mockTiles.Tiles_Example1();

        var point1 = new Vector3Int(0, 1, 0);
        var greenTile = MockTiles.GetTile(TileType.Green);
        Assert.NotNull(greenTile);

        // already there
        var shouldNotAddGreen = tiles.TryAdd(point1, greenTile);
        Assert.IsFalse(shouldNotAddGreen);
    }

    [Test]
    public void CheckTryUpdateValues() {
        _mockTiles = new MockTiles();
        _mockTiles.Init();

        var tiles = _mockTiles.Tiles_Example1();

        var point1 = new Vector3Int(0, 0, 0);
        var greenTile = tiles[point1];
        Assert.NotNull(greenTile);

        var whiteTile = MockTiles.GetTile(TileType.Green);
        Assert.NotNull(whiteTile);

        // already there
        var shouldUpdateToWhite = tiles.TryUpdate(point1, whiteTile, greenTile);
        Assert.IsTrue(shouldUpdateToWhite);

        var shouldBeWhite = tiles[point1];
        Assert.IsTrue(whiteTile.Type == shouldBeWhite.Type);
        Assert.IsTrue(whiteTile.TileKey == shouldBeWhite.TileKey);
    }
}