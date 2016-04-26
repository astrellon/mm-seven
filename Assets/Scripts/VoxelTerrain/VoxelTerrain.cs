using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[ExecuteInEditMode]
public class VoxelTerrain : MonoBehaviour
{
    //public List<Chunk> Chunks = new List<Chunk>();
    public List<BlockType> BlockTypes = new List<BlockType>();

    private Dictionary<Vector3, Chunk> Chunks = new Dictionary<Vector3, Chunk>();

	// Use this for initialization
	void Start ()
    {

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

    public void Clear()
    {
        var foundChunks = GetComponentsInChildren<Chunk>();
        foreach (var chunk in foundChunks)
        {
            var chunkRender = chunk.GetComponent<ChunkRender>();
            chunkRender.Clear();

            DestroyImmediate(chunk.gameObject);
        }
        Chunks.Clear();
    }

    public void RenderAll()
    {
        var foundChunks = GetComponentsInChildren<Chunk>();
        foreach (var chunk in foundChunks)
        {
            var chunkRender = chunk.GetComponent<ChunkRender>();
            if (chunkRender == null)
            {
                chunkRender = chunk.gameObject.AddComponent<ChunkRender>();
            }
            chunkRender.RenderChunk();

            Chunks[chunk.ChunkPosition] = chunk;
        }
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

            chunk.ChunkPosition = pos;
            chunk.Parent = this;
            chunk.transform.position = pos;

            var chunkRender = chunkObj.AddComponent<ChunkRender>();
        }

        return chunk;
    }
}
