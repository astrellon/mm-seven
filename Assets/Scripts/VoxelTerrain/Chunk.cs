using UnityEngine;
using System.Collections.Generic;

public class Chunk : MonoBehaviour
{
    public const int TotalVoxels = 16 * 16 * 16;
    public readonly Voxel[] Voxels = new Voxel[TotalVoxels];
    public Vector3 ChunkPosition = Vector3.zero;

    public VoxelTerrain Parent;

    public void Start()
    {
        /*
        for (var i = 0; i < TotalVoxels; i++)
        {
            Voxels[i] = new Voxel(Voxel.MeshShapeType.None, Voxel.RotationType.North, false, 0);
        }
        */
    }

    public void SetVoxel(uint x, uint y, uint z, Voxel voxel)
    {
        Voxels[(z << 8) + (y << 4) + x] = voxel;
    }
    public bool GetVoxel(uint x, uint y, uint z, ref Voxel voxel)
    {
        if (x >= 16 || y >= 16 || z >= 16)
        {
            return false;
        }
        voxel = Voxels[(z << 8) + (y << 4) + x];
        return true;
    }
}
