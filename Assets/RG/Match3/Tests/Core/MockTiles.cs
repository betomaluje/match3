using System.Collections.Concurrent;
using Tiles;
using UnityEngine;

public class MockTiles {
    private Tile blue;
    private Tile green;
    public int Height = 6;
    private Tile red;
    private Tile white;
    public int Width = 6;

    public ConcurrentDictionary<Vector3Int, Tile> Tiles_Example1() {
        var tiles = new ConcurrentDictionary<Vector3Int, Tile>();

        for (var x = 0; x < Width; x++) {
            for (var y = 0; y < Height; y++) {
                var point = new Vector3Int(x, y, 0);

                var tile = white;
                if (y % 3 == 0) {
                    tile = green;
                }

                tile.TileKey = point;
                tiles.TryAdd(point, tile);
            }
        }

        return tiles;
    }

    public void Init() {
        green = GetTile(TileType.Green);
        white = GetTile(TileType.White);
        red = GetTile(TileType.Red);
        blue = GetTile(TileType.Blue);
    }

    public static Tile GetTile(TileType type) {
        var config = ScriptableObject.CreateInstance<TileSO>();
        var tile = new GameObject().AddComponent<Tile>();
        tile.Config = config;
        tile.Type = type;
        return tile;
    }
}