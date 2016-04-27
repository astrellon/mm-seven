using UnityEngine;
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
    private Dictionary<PlaneAlignment, Vector3> planeNormals = null;
    private bool createPlane = true;

    private PlaneAlignment alignment = PlaneAlignment.XZ;

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
        if (planeNormals == null)
        {
            planeNormals = new Dictionary<PlaneAlignment, Vector3> {
                { PlaneAlignment.XY, Vector3.forward },
                { PlaneAlignment.YZ, Vector3.left },
                { PlaneAlignment.XZ, Vector3.up },
                { PlaneAlignment.X, Vector3.up },
                { PlaneAlignment.Y, Vector3.left },
                { PlaneAlignment.Z, Vector3.up },
            };
        }

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
        foreach (var kvp in planeNormals)
        {
            if (GUILayout.Toggle(alignment == kvp.Key, kvp.Key.ToString(), "Button"))
            {
                alignment = kvp.Key;
                createPlane = true;
            }
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
            if (e.keyCode == KeyCode.Alpha1)
            {
                newAlignment = PlaneAlignment.XY;
            }
            else if (e.keyCode == KeyCode.Alpha2)
            {
                newAlignment = PlaneAlignment.XZ;
            }
            else if (e.keyCode == KeyCode.Alpha3)
            {
                newAlignment = PlaneAlignment.YZ;
            }
            else if (e.keyCode == KeyCode.Alpha4)
            {
                newAlignment = PlaneAlignment.X;
            }
            else if (e.keyCode == KeyCode.Alpha5)
            {
                newAlignment = PlaneAlignment.Y;
            }
            else if (e.keyCode == KeyCode.Alpha6)
            {
                newAlignment = PlaneAlignment.Z;
            }

            if (newAlignment != PlaneAlignment.None)
            {
                createPlane = true;
                alignment = newAlignment;
                EditorUtility.SetDirty(terrain);
                e.Use();
            }
        }

        CreatePlanes(terrain);

        if (e.type == EventType.MouseDown)
        {
            GUIUtility.hotControl = controlId;

            //layerTarget.AddTile(tileCursorPosition, 0);

            EditorUtility.SetDirty(terrain);

            e.Use();
        }
        else if (e.type == EventType.MouseMove)
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
    }
}
