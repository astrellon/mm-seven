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

        var v1 = new Voxel(Voxel.MeshShapeType.LargeCorner, Voxel.RotationType.South, true, 0);
        var v2 = new Voxel(Voxel.MeshShapeType.Cube, Voxel.RotationType.North, false, 0);
        var v3 = new Voxel(Voxel.MeshShapeType.MiterConcave, Voxel.RotationType.East, true, 1);
        chunk.SetVoxel(0, 0, 0, v1);
        chunk.SetVoxel(1, 1, 0, v2);
        chunk.SetVoxel(2, 0, 1, v3);

        Chunks.Add(chunk);

        var chunkRender = chunkObj.AddComponent<ChunkRender>();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
