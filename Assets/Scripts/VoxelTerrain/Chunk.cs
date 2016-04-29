using UnityEngine;
using System.Collections.Generic;

public class Chunk : MonoBehaviour
{
    public const int TotalVoxels = 8 * 8 * 8;
    public readonly Voxel[] Voxels = new Voxel[TotalVoxels];
    public Vector3 ChunkPosition = Vector3.zero;
    public bool IsDirty = true;

    public VoxelTerrain Parent;

    public void Start()
    {
    }

    public void SetVoxel(uint x, uint y, uint z, Voxel voxel)
    {
        Voxels[(z << 6) + (y << 3) + x] = voxel;
        IsDirty = true;
    }
    public bool GetVoxel(uint x, uint y, uint z, ref Voxel voxel)
    {
        if (x >= 8 || y >= 8 || z >= 8)
        {
            return false;
        }
        voxel = Voxels[(z << 6) + (y << 3) + x];
        return true;
    }
}
