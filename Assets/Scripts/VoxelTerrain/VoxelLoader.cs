using UnityEngine;
using System.Collections;

[RequireComponent(typeof(VoxelTerrain))]
[ExecuteInEditMode]
public class VoxelLoader : MonoBehaviour {

    public string FilePath;

	// Use this for initialization
	void Start ()
    {
        //var terrain = GetComponent<VoxelTerrain>();
        //Load(terrain, FilePath);
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    public void Load()
    {
        var terrain = GetComponent<VoxelTerrain>();
        terrain.Clear();
        Load(terrain, FilePath);
        terrain.RenderAll();
    }

    public static void Load(VoxelTerrain terrain, string path)
    {
        foreach (var line in System.IO.File.ReadAllLines(System.IO.Path.Combine(@"Assets\Levels", path)))
        {
            if (line.Length == 0 || line[0] == '#')
            {
                continue;
            }

            var split = line.Split(',');
            var x = int.Parse(split[0]);
            var y = int.Parse(split[1]);
            var z = int.Parse(split[2]);

            var meshShape = ParseMeshShape(split[3]);
            var rotation = ParseRotation(split[4]);
            var isUpsideDown = split[5] == "true";
            var blockType = ushort.Parse(split[6]);

            terrain.SetVoxel(x, y, z, new Voxel(meshShape, rotation, isUpsideDown, blockType));
        }
    }

    public static void Save(VoxelTerrain terrain, string path)
    {

    }

    private static Voxel.MeshShapeType ParseMeshShape(string input)
    {
        switch (input)
        {
            case "none":
                return Voxel.MeshShapeType.None;
            case "cube":
                return Voxel.MeshShapeType.Cube;
            case "ramp":
                return Voxel.MeshShapeType.Ramp;
            case "small-corner":
                return Voxel.MeshShapeType.SmallCorner;
            case "large-corner":
                return Voxel.MeshShapeType.LargeCorner;
            case "miter-convex":
                return Voxel.MeshShapeType.MiterConvex;
            case "miter-concave":
                return Voxel.MeshShapeType.MiterConcave;
        }
        throw new System.ArgumentException("Unknown mesh shape type: " + input);
    }

    private static Voxel.RotationType ParseRotation(string input)
    {
        switch (input)
        {
            case "north":
                return Voxel.RotationType.North;
            case "east":
                return Voxel.RotationType.East;
            case "south":
                return Voxel.RotationType.South;
            case "west":
                return Voxel.RotationType.West;
        }
        throw new System.ArgumentException("Unknown rotation type: " + input);
    }
}
