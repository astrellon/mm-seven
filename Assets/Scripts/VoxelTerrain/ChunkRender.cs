using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Chunk))]
public class ChunkRender : MonoBehaviour
{
    private class MeshContext
    {
        public List<int> Triangles = new List<int>();
        public List<Vector3> Verticies = new List<Vector3>();
    }
    public Chunk Chunk { get; private set; }

    private Dictionary<ushort, MeshContext> TypedBlocks = new Dictionary<ushort, MeshContext>();

	// Use this for initialization
	void Start ()
    {
        Chunk = GetComponent<Chunk>();
        RenderChunk();
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    private MeshContext GetMeshContext(Voxel voxel)
    {
        MeshContext result;
        if (!TypedBlocks.TryGetValue(voxel.BlockType, out result))
        {
            TypedBlocks[voxel.BlockType] = result = new MeshContext();
        }
        return result;
    }

    public void RenderChunk()
    {
        for (int z = 0, i = 0; z < 16; z++)
        {
            for (var y = 0; y < 16; y++)
            {
                for (var x = 0; x < 16; x++, i++)
                {
                    var voxel = Chunk.Voxels[i];
                    if (voxel.MeshShape == Voxel.MeshShapeType.None)
                    {
                        continue;
                    }

                    var context = GetMeshContext(voxel);
                    RenderCube(new Vector3(x, y, z), voxel, context);
                }
            }
        }

        foreach (var kvp in TypedBlocks)
        {
            RenderMeshContext(kvp.Key, kvp.Value);
        }
    }

    void RenderMeshContext(ushort blockType, MeshContext context)
    {
        var blockTypeObject = Chunk.Parent.BlockTypes[blockType];

        var meshObject = new GameObject();
        meshObject.name = blockTypeObject.BlockName;
        meshObject.transform.parent = transform;

        var meshFilter = meshObject.AddComponent<MeshFilter>();
        var meshCollider = meshObject.AddComponent<MeshCollider>();
        var meshRenderer = meshObject.AddComponent<MeshRenderer>();

        var mesh = new Mesh();
        meshFilter.mesh = mesh;

        mesh.vertices = context.Verticies.ToArray();
        mesh.triangles = context.Triangles.ToArray();
        mesh.Optimize();
        mesh.RecalculateNormals();

        meshCollider.sharedMesh = mesh;

        meshRenderer.material = blockTypeObject.BlockMaterial;
    }

    void RenderCube(Vector3 position, Voxel voxel, MeshContext context)
    {
        RenderQuad(context, position, Vector3.down);
        RenderQuad(context, position, Vector3.up);

        RenderQuad(context, position, Vector3.left);
        RenderQuad(context, position, Vector3.right);

        RenderQuad(context, position, Vector3.forward);
        RenderQuad(context, position, Vector3.back);
    }

    void RenderQuad(MeshContext context, Vector3 position, Vector3 normal)
    {
        var rotation = Quaternion.FromToRotation(Vector3.up, normal);
        var p1 = rotation * new Vector3(-0.5f, 0, -0.5f);
        var p2 = rotation * new Vector3(0.5f, 0, -0.5f);
        var p3 = rotation * new Vector3(-0.5f, 0, 0.5f);
        var p4 = rotation * new Vector3(0.5f, 0, 0.5f);
        var halfNormal = normal * 0.5f;

        context.Verticies.Add(position + p3 + halfNormal);
        context.Verticies.Add(position + p2 + halfNormal);
        context.Verticies.Add(position + p1 + halfNormal);

        context.Verticies.Add(position + p4 + halfNormal);
        context.Verticies.Add(position + p2 + halfNormal);
        context.Verticies.Add(position + p3 + halfNormal);

        for (var i = 0; i < 6; i++)
        {
            context.Triangles.Add(context.Triangles.Count);
        }
    }
}
