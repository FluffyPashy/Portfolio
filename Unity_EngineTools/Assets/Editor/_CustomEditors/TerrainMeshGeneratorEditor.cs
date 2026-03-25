using ProceduralTerrain;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainMeshGenerator))]
public class TerrainMeshGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Terrain Tools", EditorStyles.boldLabel);

        TerrainMeshGenerator generator = (TerrainMeshGenerator)target;

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Place Prefabs"))
        {
            Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "Place Terrain Prefabs");
            int count = generator.PlacePrefabs();
            Debug.Log($"Placed {count} prefabs.", generator);
            EditorUtility.SetDirty(generator);
        }

        if (GUILayout.Button("Clear Placed Prefabs"))
        {
            Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "Clear Terrain Prefabs");
            generator.ClearPreviouslyPlacedPrefabs();
            EditorUtility.SetDirty(generator);
        }
        EditorGUILayout.EndHorizontal();
    }
}
