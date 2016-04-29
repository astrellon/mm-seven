﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(VoxelTerrain))]
public class VoxelTerrainEditor : Editor
{
    private enum PlaneAlignment
    {
        None, XY, XZ, YZ, X, Y, Z
    }

    private Vector3 cursorPosition = Vector3.zero;
    private Vector3 tileCursorPosition = Vector3.zero;
    private Plane plane = new Plane();
    private Dictionary<PlaneAlignment, Vector3> planeNormals = 
        new Dictionary<PlaneAlignment, Vector3> {
            { PlaneAlignment.XY, Vector3.back },
            { PlaneAlignment.XZ, Vector3.down },
            { PlaneAlignment.YZ, Vector3.right },
            { PlaneAlignment.X, Vector3.down },
            { PlaneAlignment.Y, Vector3.left },
            { PlaneAlignment.Z, Vector3.up },
        };

    private List<Voxel.MeshShapeType> meshShapeTypes = new List<Voxel.MeshShapeType> {
        Voxel.MeshShapeType.None,
        Voxel.MeshShapeType.Cube,
        Voxel.MeshShapeType.Ramp,
        Voxel.MeshShapeType.SmallCorner,
        Voxel.MeshShapeType.LargeCorner,
        Voxel.MeshShapeType.MiterConvex,
        Voxel.MeshShapeType.MiterConcave,
    };

    private List<Voxel.RotationType> rotationTypes = new List<Voxel.RotationType> {
        Voxel.RotationType.North,
        Voxel.RotationType.East,
        Voxel.RotationType.South,
        Voxel.RotationType.West,
    };

    private bool createPlane = true;
    private ushort selectedBlockType = 0;
    private Voxel.MeshShapeType selectedMeshShape = Voxel.MeshShapeType.Cube;
    private Voxel.RotationType selectedRotation = Voxel.RotationType.North;
    private bool selectedUpsideDown = false;

    private PlaneAlignment alignment = PlaneAlignment.XZ;

    private List<KeyCode> Row1Keys = new List<KeyCode>
    {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0
    };
    private List<KeyCode> Row2Keys = new List<KeyCode>
    {
        KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P
    };
    private List<KeyCode> Row3Keys = new List<KeyCode>
    {
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L,
    };
    private List<KeyCode> Row4Keys = new List<KeyCode>
    {
        KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B, KeyCode.N, KeyCode.M,
    };

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    void CreatePlanes(VoxelTerrain terrain)
    {
        if (createPlane)
        {
            var normal = planeNormals[alignment];
            plane = new Plane(normal, tileCursorPosition);

            createPlane = false;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var terrain = (VoxelTerrain)target;

        CreatePlanes(terrain);

        EditorGUILayout.LabelField("Plane alignment");
        GUILayout.BeginHorizontal();
        foreach (var kvp in planeNormals)
        {
            if (GUILayout.Toggle(alignment == kvp.Key, kvp.Key.ToString(), "Button"))
            {
                alignment = kvp.Key;
                createPlane = true;
            }
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Available block types");
        ushort blockTypeCount = 0;
        foreach (var blockType in terrain.BlockTypes)
        {
            if (GUILayout.Toggle(selectedBlockType == blockTypeCount, blockType.BlockName, "Button"))
            {
                selectedBlockType = blockTypeCount;
            }
            blockTypeCount++;
        }

        EditorGUILayout.LabelField("Mesh shape type");
        foreach (var meshShape in meshShapeTypes)
        {
            if (GUILayout.Toggle(selectedMeshShape == meshShape, meshShape.ToString(), "Button"))
            {
                selectedMeshShape = meshShape;
            }
        }

        EditorGUILayout.LabelField("Rotation direction");
        foreach (var rotation in rotationTypes)
        {
            if (GUILayout.Toggle(selectedRotation == rotation, rotation.ToString(), "Button"))
            {
                selectedRotation = rotation;
            }
        }

        EditorGUILayout.LabelField("Delete all terrain!?");
        if (GUILayout.Button("Clear terrain"))
        {
            terrain.Clear();
        }

        serializedObject.ApplyModifiedProperties();
    }

    void OnSceneGUI()
    {
        var e = Event.current;
        var controlId = GUIUtility.GetControlID(FocusType.Passive);
        var terrain = (VoxelTerrain)target;

        if (e.type == EventType.KeyDown)
        {
            var newAlignment = PlaneAlignment.None;
            var useEvent = false;
            var index = 0;
            foreach (var planeNormal in planeNormals)
            {
                if (index >= Row1Keys.Count) return;

                if (e.keyCode == Row1Keys[index++])
                {
                    newAlignment = planeNormal.Key;
                }
            }

            if (newAlignment != PlaneAlignment.None)
            {
                createPlane = true;
                alignment = newAlignment;
                EditorUtility.SetDirty(terrain);
                useEvent = true;
            }

            index = 0;
            foreach (var blockType in terrain.BlockTypes)
            {
                if (index >= Row2Keys.Count) return;

                if (e.keyCode == Row2Keys[index])
                {
                    selectedBlockType = (ushort)index;
                    useEvent = true;
                }
                index++;
            }

            index = 0;
            foreach (var meshShape in meshShapeTypes)
            {
                if (index >= Row3Keys.Count) return;

                if (e.keyCode == Row3Keys[index++])
                {
                    selectedMeshShape = meshShape;
                    useEvent = true;
                }
            }

            index = 0;
            foreach (var rotation in rotationTypes)
            {
                if (index >= Row4Keys.Count) return;

                if (e.keyCode == Row4Keys[index++])
                {
                    selectedRotation = rotation;
                    useEvent = true;
                }
            }

            if (useEvent)
            {
                GUIUtility.hotControl = controlId;
                EditorUtility.SetDirty(terrain);
                e.Use();
            }
        }

        CreatePlanes(terrain);

        if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
        {
            GUIUtility.hotControl = controlId;

            var x = (int)tileCursorPosition.x;
            var y = (int)tileCursorPosition.y;
            var z = (int)tileCursorPosition.z;
            terrain.SetVoxel(x, y, z, new Voxel(selectedMeshShape, selectedRotation, selectedUpsideDown, selectedBlockType));

            EditorUtility.SetDirty(terrain);

            if (e.type == EventType.MouseDown)
            {
                e.Use();
            }
        }

        if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag)
        {
            float rayDistance;
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (plane.Raycast(ray, out rayDistance))
            {
                cursorPosition = ray.GetPoint(rayDistance);
                var newCursor = new Vector3(Mathf.Round(cursorPosition.x), Mathf.Round(cursorPosition.y), Mathf.Round(cursorPosition.z));

                if (alignment == PlaneAlignment.X)
                {
                    tileCursorPosition = new Vector3(newCursor.x, tileCursorPosition.y, tileCursorPosition.z);
                }
                else if (alignment == PlaneAlignment.Y)
                {
                    tileCursorPosition = new Vector3(tileCursorPosition.x, newCursor.y, tileCursorPosition.z);
                }
                else if (alignment == PlaneAlignment.Z)
                {
                    tileCursorPosition = new Vector3(tileCursorPosition.x, tileCursorPosition.y, newCursor.z);
                }
                else
                {
                    tileCursorPosition = newCursor;
                }
                SceneView.RepaintAll();
            }
        }

        Handles.color = new Color(0.35f, 0.4f, 0.8f, 0.5f);
        Handles.CubeCap(0, tileCursorPosition, Quaternion.identity, 1.0f);

        var xColour = (alignment == PlaneAlignment.XY || alignment == PlaneAlignment.XZ || alignment == PlaneAlignment.X) ? Color.red : new Color(1, 1, 1, 0.5f);
        var yColour = (alignment == PlaneAlignment.YZ || alignment == PlaneAlignment.XY || alignment == PlaneAlignment.Y) ? Color.green : new Color(1, 1, 1, 0.5f);
        var zColour = (alignment == PlaneAlignment.XZ || alignment == PlaneAlignment.YZ || alignment == PlaneAlignment.Z) ? Color.blue : new Color(1, 1, 1, 0.5f);

        Handles.color = xColour;
        Handles.DrawLine(tileCursorPosition + Vector3.left * 100, tileCursorPosition + Vector3.right * 100);
        Handles.color = yColour;
        Handles.DrawLine(tileCursorPosition + Vector3.up * 100, tileCursorPosition + Vector3.down * 100);
        Handles.color = zColour;
        Handles.DrawLine(tileCursorPosition + Vector3.forward * 100, tileCursorPosition + Vector3.back * 100);

        var screenPos = Camera.current.WorldToScreenPoint(tileCursorPosition);

        Handles.BeginGUI();
        GUI.Label(new Rect(screenPos.x, Screen.height - screenPos.y, 100, 40), string.Format("({0}, {1}, {2})", tileCursorPosition.x, tileCursorPosition.y, tileCursorPosition.z));
        Handles.EndGUI();
    }
}
