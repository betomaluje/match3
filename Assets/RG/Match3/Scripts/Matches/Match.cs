using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tiles;
using UnityEngine;

namespace Matches {
    public class Match {
        public static readonly int DEFAULT_MINIUM = 3;

        private readonly Dictionary<TileType, HashSet<Vector3Int>> _tiles =
            new Dictionary<TileType, HashSet<Vector3Int>>();

        public Match() { }

        public Match(int minimumAmount) {
            MinimumAmount = minimumAmount;
        }

        public int MinimumAmount { get; } = DEFAULT_MINIUM;

        /// <summary>
        ///     Checks all the matches for a given set of Tile objects.
        /// </summary>
        /// <param name="row">The list of tiles in a row to check</param>
        /// <returns>A list containing every position a tile matched</returns>
        public async Task<List<Vector3Int>> CheckRow(List<Tile> row) {
            if (row.Count == 0) {
                return null;
            }

            row = row.OrderBy(p => p.TileKey.x).ToList();

            foreach (TileType type in Enum.GetValues(typeof(TileType))) {
                _tiles[type] = GetConsecutiveSameElementList(row, type, MinimumAmount);
            }

            return await Task.FromResult(_tiles.SelectMany(t => t.Value)
                .Distinct()
                .ToList());
        }

        /// <summary>
        ///     Filters a collection for the given TileType and minimum amount. It returns a HashSet containing the
        ///     positions of each tile.
        /// </summary>
        /// <param name="list">The collection of Tile objects to search</param>
        /// <param name="tileType">The type of tile to filter</param>
        /// <param name="consecutiveCount">The minimum amount of consecutive elements</param>
        /// <returns>A HashSet containing the positions of each consecutive tile sublist</returns>
        private static HashSet<Vector3Int> GetConsecutiveSameElementList(IReadOnlyCollection<Tile> list,
            TileType tileType, int consecutiveCount) {
            var result = new HashSet<Vector3Int>();

            var len = list.Count - consecutiveCount;
            for (var i = 0; i <= len; i++) {
                var sublist = list.Skip(i).Take(consecutiveCount).ToList();
                if (sublist.Any(item => item.Type != tileType)) {
                    continue;
                }

                var sublistCount = sublist.Count;
                for (var j = 0; j < sublistCount; j++) {
                    var tile = sublist[j];
                    result.Add(tile.TileKey);
                }
            }

            return result;
        }
    }
}