using UnityEditor;

[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor
{
    private static bool _isFolded;

    public override void OnInspectorGUI()
    {
        var gridManager = (GridManager) target;

        if (gridManager.Tiles != null && gridManager.Tiles.Count > 0)
        {
            _isFolded = EditorGUILayout.Foldout(_isFolded, "Tiles");
            if (_isFolded)
                foreach (var pair in gridManager.Tiles)
                    EditorGUILayout.Vector3Field($"{pair.Value.name}", pair.Key);
        }

        base.OnInspectorGUI();
    }
}