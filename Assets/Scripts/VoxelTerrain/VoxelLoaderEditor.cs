using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(VoxelLoader))]
[CanEditMultipleObjects]
public class VoxelLoaderEditor : Editor {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var loader = (VoxelLoader)target;

        loader.FilePath = EditorGUILayout.TextField("FilePath", loader.FilePath);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Load"))
        {
            loader.Load();
        }
        if (GUILayout.Button("Save"))
        {
            loader.Save();
        }
        GUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }
}
