using UnityEngine;
using System.Collections.Generic;

public class VoxelTerrain : MonoBehaviour
{
    public List<Chunk> Chunks = new List<Chunk>();
    public List<BlockType> BlockTypes = new List<BlockType>();

	// Use this for initialization
	void Start () {
        var chunkObj = new GameObject();
        chunkObj.transform.parent = transform;
        var chunk = chunkObj.AddComponent<Chunk>();
        chunk.Parent = this;

        var types = new[] {
            Voxel.MeshShapeType.Cube,
            Voxel.MeshShapeType.Ramp,
            Voxel.MeshShapeType.SmallCorner,
            Voxel.MeshShapeType.LargeCorner,
            Voxel.MeshShapeType.MiterConcave,
            Voxel.MeshShapeType.MiterConvex,
        };

        var ypos = 0u;
        foreach (var type in types)
        {
            chunk.SetVoxel(0, ypos, 0, new Voxel(type, Voxel.RotationType.North, false, 0));
            chunk.SetVoxel(2, ypos, 0, new Voxel(type, Voxel.RotationType.East, false, 0));
            chunk.SetVoxel(4, ypos, 0, new Voxel(type, Voxel.RotationType.South, false, 0));
            chunk.SetVoxel(6, ypos, 0, new Voxel(type, Voxel.RotationType.West, false, 0));

            chunk.SetVoxel(0, ypos, 2, new Voxel(type, Voxel.RotationType.North, true, 0));
            chunk.SetVoxel(2, ypos, 2, new Voxel(type, Voxel.RotationType.East, true, 0));
            chunk.SetVoxel(4, ypos, 2, new Voxel(type, Voxel.RotationType.South, true, 0));
            chunk.SetVoxel(6, ypos, 2, new Voxel(type, Voxel.RotationType.West, true, 0));

            ypos += 2u;
        }

        Chunks.Add(chunk);

        var chunkRender = chunkObj.AddComponent<ChunkRender>();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
