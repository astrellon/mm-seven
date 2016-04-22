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

        chunk.SetVoxel(0, 0, 0, new Voxel(Voxel.MeshShapeType.MiterConcave, Voxel.RotationType.North, false, 0));
        chunk.SetVoxel(1, 0, 0, new Voxel(Voxel.MeshShapeType.MiterConcave, Voxel.RotationType.East, false, 0));
        chunk.SetVoxel(2, 0, 0, new Voxel(Voxel.MeshShapeType.MiterConcave, Voxel.RotationType.South, false, 0));
        chunk.SetVoxel(3, 0, 0, new Voxel(Voxel.MeshShapeType.MiterConcave, Voxel.RotationType.West, false, 0));

        /*
        chunk.SetVoxel(0, 0, 2, new Voxel(Voxel.MeshShapeType.Ramp, Voxel.RotationType.North, true, 0));
        chunk.SetVoxel(1, 0, 2, new Voxel(Voxel.MeshShapeType.Ramp, Voxel.RotationType.East, true, 0));
        chunk.SetVoxel(2, 0, 2, new Voxel(Voxel.MeshShapeType.Ramp, Voxel.RotationType.South, true, 0));
        chunk.SetVoxel(3, 0, 2, new Voxel(Voxel.MeshShapeType.Ramp, Voxel.RotationType.West, true, 0));
        */

        Chunks.Add(chunk);

        var chunkRender = chunkObj.AddComponent<ChunkRender>();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
