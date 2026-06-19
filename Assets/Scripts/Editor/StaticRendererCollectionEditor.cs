
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StaticRendererCollection))]
public class StaticRendererCollectionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Export"))
        {
            StaticRendererCollection collection = target as StaticRendererCollection;
            collection.Collect();
        }

        if (GUILayout.Button("Clear"))
        {
            StaticRendererCollection collection = target as StaticRendererCollection;
            collection.Clear();
        }
    }
}
