using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private bool _drawGizmos = true;
    [SerializeField] private int _width = 5;
    [SerializeField] private int _height = 5;
    [SerializeField] private Tile[] _tilePrefabs;
    [SerializeField] private Transform _tileContainer;

    private void Start()
    {
        foreach (var point in EvaluateGridPoints())
        {
            var tilePrefab = _tilePrefabs[Random.Range(0, _tilePrefabs.Length)];
            var tile = Instantiate(tilePrefab, point, Quaternion.identity);
            tile.transform.SetParent(_tileContainer);
        }
    }

    #region Debug

    private void OnDrawGizmos()
    {
        if (!_drawGizmos || Application.isPlaying) return;
        var size = Vector2.one;
        foreach (var point in EvaluateGridPoints())
            Gizmos.DrawWireCube(point, size);
    }

    #endregion

    public void OnTileDestroyed(Vector3 tilePosition)
    {
        Debug.Log($"clicked: {tilePosition} -> x:{tilePosition.x}, y:{tilePosition.y}");

        // get all the column to move down
        
        // check horizontal for matches
    }

    private IEnumerable<Vector3Int> EvaluateGridPoints()
    {
        for (var x = 0; x < _width; x++)
        for (var y = 0; y < _height; y++)
            yield return new Vector3Int(x, y, 0);
    }
}