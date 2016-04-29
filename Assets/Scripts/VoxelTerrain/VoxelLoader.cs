using UnityEngine;
using System.Collections;
using System.Linq;

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

    public static string LevelPath(string path)
    {
        return System.IO.Path.Combine(@"Assets\Levels", path);
    }

    public static void Load(VoxelTerrain terrain, string path)
    {
        foreach (var line in System.IO.File.ReadAllLines(LevelPath(path)))
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

    public void Save()
    {
        var terrain = GetComponent<VoxelTerrain>();
        Save(terrain, FilePath);
    }

    public static void Save(VoxelTerrain terrain, string path)
    {
        using (var output = new System.IO.StreamWriter(LevelPath(path), false, System.Text.Encoding.UTF8))
        {
            output.WriteLine("# Level terrain data for {0}", path);
            output.WriteLine("# X, Y, Z, MeshShape, Rotation, IsUpsideDown, BlockType");
            foreach (var chunk in terrain.Chunks.Values)
            {
                SaveChunk(chunk, output);
            }
        }
    }

    private static void SaveChunk(Chunk chunk, System.IO.StreamWriter output)
    {
        var i = 0;
        for (var z = 0u; z < 8u; z++)
        for (var y = 0u; y < 8u; y++)
        for (var x = 0u; x < 8u; x++, i++)
        {
            var voxel = chunk.Voxels[i];
            if (voxel.MeshShape == Voxel.MeshShapeType.None)
            {
                continue;
            }

            var worldX = x + (int)chunk.ChunkPosition.x;
            var worldY = y + (int)chunk.ChunkPosition.y;
            var worldZ = z + (int)chunk.ChunkPosition.z;

            output.WriteLine(Join(",", worldX, worldY, worldZ, 
                ToMeshShapeString(voxel.MeshShape), 
                ToRotationString(voxel.Rotation), 
                voxel.IsUpsideDown ? "true" : "false", 
                voxel.BlockType));
        }
    }

    private static string Join(string separator, params object[] inputs)
    {
        var result = new System.Text.StringBuilder();
        var first = true;
        foreach (var o in inputs)
        {
            if (!first)
            {
                result.Append(separator);
            }
            first = false;
            result.Append(o);
        }
        return result.ToString();
    }

    private static string ToMeshShapeString(Voxel.MeshShapeType input)
    {
        switch (input)
        {
            case Voxel.MeshShapeType.None:
                return "none";
            case Voxel.MeshShapeType.Cube:
                return "cube";
            case Voxel.MeshShapeType.Ramp:
                return "ramp";
            case Voxel.MeshShapeType.SmallCorner:
                return "small-corner";
            case Voxel.MeshShapeType.LargeCorner:
                return "large-corner";
            case Voxel.MeshShapeType.MiterConvex:
                return "miter-convex";
            case Voxel.MeshShapeType.MiterConcave:
                return "miter-concave";
        }
        throw new System.ArgumentException("Unknown mesh shape type: " + input);
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

    private static string ToRotationString(Voxel.RotationType input)
    {
        switch (input)
        {
            case Voxel.RotationType.North:
                return "north";
            case Voxel.RotationType.East:
                return "east";
            case Voxel.RotationType.South:
                return "south";
            case Voxel.RotationType.West:
                return "west";
        }
        throw new System.ArgumentException("Unknown rotation type: " + input);
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
