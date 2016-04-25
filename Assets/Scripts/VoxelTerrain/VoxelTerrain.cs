using UnityEngine;
using System.Collections.Generic;

public class VoxelTerrain : MonoBehaviour
{
    //public List<Chunk> Chunks = new List<Chunk>();
    public List<BlockType> BlockTypes = new List<BlockType>();

    private Dictionary<Vector3, Chunk> Chunks = new Dictionary<Vector3, Chunk>();

	// Use this for initialization
	void Start ()
    {
        for (var x = -16; x < 16; x++)
        for (var y = 0; y < 4; y++)
        for (var z = -16; z < 16; z++)
        {
            SetVoxel(x, y, z, new Voxel(Voxel.MeshShapeType.Cube, Voxel.RotationType.East, false, 0));
        }

        for (var r = 0; r < 5; r++)
        for (var w = 0; w < 4; w++)
        {
            SetVoxel(4 + r, 4 + r, 4 + w, new Voxel(Voxel.MeshShapeType.Ramp, Voxel.RotationType.East, false, 0));
            SetVoxel(4 + r, 3 + r, 4 + w, new Voxel(Voxel.MeshShapeType.Cube, Voxel.RotationType.East, false, 0));
        }

        for (var x = 0; x < 4; x++)
        for (var z = 0; z < 4; z++)
        {
            SetVoxel(9 + x, 8, 4 + z, new Voxel(Voxel.MeshShapeType.Cube, Voxel.RotationType.North, false, 0));
        }
    }

    public void SetVoxel(int x, int y, int z, Voxel voxel)
    {
        var chunk = GetChunk(x, y, z);

        var localX = (byte)Mathf.Abs(x % 16);
        var localY = (byte)Mathf.Abs(y % 16);
        var localZ = (byte)Mathf.Abs(z % 16);

        chunk.SetVoxel(localX, localY, localZ, voxel);
    }
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    Chunk GetChunk(int x, int y, int z)
    {
        var worldX = Mathf.Floor(x / 16f);
        var worldY = Mathf.Floor(y / 16f);
        var worldZ = Mathf.Floor(z / 16f);

        var pos = new Vector3(worldX * 16f, worldY * 16f, worldZ * 16);
        Chunk chunk;
        if (!Chunks.TryGetValue(pos, out chunk))
        {
            var chunkObj = new GameObject();
            chunkObj.transform.parent = transform;
            chunkObj.name = "chunk_" + worldX + "_" + worldY + "_" + worldZ;
            Chunks[pos] = chunk = chunkObj.AddComponent<Chunk>();
            chunk.Parent = this;
            chunk.transform.position = pos;

            var chunkRender = chunkObj.AddComponent<ChunkRender>();
        }

        return chunk;
    }
}
