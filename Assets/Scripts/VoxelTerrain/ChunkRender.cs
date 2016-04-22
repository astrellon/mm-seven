using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Chunk))]
public class ChunkRender : MonoBehaviour
{
    private class MeshContext
    {
        public List<Vector3> Verticies = new List<Vector3>();
    }

    private static Vector3 FrontTop1 = new Vector3(-0.5f, 0.5f, 0.5f);
    private static Vector3 FrontTop2 = new Vector3(0.5f, 0.5f, 0.5f);
    private static Vector3 FrontBot1 = new Vector3(-0.5f, -0.5f, 0.5f);
    private static Vector3 FrontBot2 = new Vector3(0.5f, -0.5f, 0.5f);
    private static Vector3 BackTop1 = new Vector3(-0.5f, 0.5f, -0.5f);
    private static Vector3 BackTop2 = new Vector3(0.5f, 0.5f, -0.5f);
    private static Vector3 BackBot1 = new Vector3(-0.5f, -0.5f, -0.5f);
    private static Vector3 BackBot2 = new Vector3(0.5f, -0.5f, -0.5f);

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
                    var shape = voxel.MeshShape; 
                    if (shape == Voxel.MeshShapeType.None)
                    {
                        continue;
                    }

                    var context = GetMeshContext(voxel);
                    var position = new Vector3(x, y, z);

                    switch (shape)
                    {
                        case Voxel.MeshShapeType.Cube:
                            RenderCube(position, voxel, context);
                            break;
                        case Voxel.MeshShapeType.Ramp:
                            RenderRamp(position, voxel, context);
                            break;
                        case Voxel.MeshShapeType.SmallCorner:
                            RenderSmallCorner(position, voxel, context);
                            break;
                        case Voxel.MeshShapeType.LargeCorner:
                            RenderLargeCorner(position, voxel, context);
                            break;
                        case Voxel.MeshShapeType.MiterConvex:
                            RenderMiterConvex(position, voxel, context);
                            break;
                        case Voxel.MeshShapeType.MiterConcave:
                            RenderMiterConcave(position, voxel, context);
                            break;
                    }
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
        var triangles = new int[context.Verticies.Count];
        for (var i = 0; i < context.Verticies.Count; i++)
        {
            triangles[i] = i;
        }
        mesh.triangles = triangles;
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

    void RenderRamp(Vector3 position, Voxel voxel, MeshContext context)
    {
        var points = new List<Vector3>(GetQuadVertices(Vector3.forward));
        points.AddRange(GetQuadVertices(Vector3.down));

        // Ramp
        points.Add(FrontTop1);
        points.Add(FrontTop2);
        points.Add(BackBot1);

        points.Add(BackBot2);
        points.Add(BackBot1);
        points.Add(FrontTop2);

        // Left
        points.Add(FrontBot1);
        points.Add(FrontTop1);
        points.Add(BackBot1);

        // Right
        points.Add(BackBot2);
        points.Add(FrontTop2);
        points.Add(FrontBot2);

        AddPoints(position, points, voxel, context);
    }

    void RenderSmallCorner(Vector3 position, Voxel voxel, MeshContext context)
    {
        var points = new List<Vector3>();

        points.Add(FrontBot2);
        points.Add(FrontTop2);
        points.Add(FrontBot1);

        points.Add(FrontBot2);
        points.Add(BackBot2);
        points.Add(FrontTop2);

        points.Add(FrontBot1);
        points.Add(FrontTop2);
        points.Add(BackBot2);

        points.Add(BackBot2);
        points.Add(FrontBot2);
        points.Add(FrontBot1);

        AddPoints(position, points, voxel, context);
    }

    void RenderLargeCorner(Vector3 position, Voxel voxel, MeshContext context)
    {
        var points = new List<Vector3>();
        points.AddRange(GetQuadVertices(Vector3.forward));
        points.AddRange(GetQuadVertices(Vector3.right));
        points.AddRange(GetQuadVertices(Vector3.down));

        // Top
        points.Add(FrontTop1);
        points.Add(FrontTop2);
        points.Add(BackTop2);

        // Back
        points.Add(BackBot1);
        points.Add(BackTop2);
        points.Add(BackBot2);

        // Left
        points.Add(FrontBot1);
        points.Add(FrontTop1);
        points.Add(BackBot1);

        // Middle
        points.Add(BackTop2);
        points.Add(BackBot1);
        points.Add(FrontTop1);

        AddPoints(position, points, voxel, context);
    }

    void RenderMiterConvex(Vector3 position, Voxel voxel, MeshContext context)
    {
        var points = new List<Vector3>();
        points.AddRange(GetQuadVertices(Vector3.down));

        points.Add(FrontBot2);
        points.Add(FrontTop2);
        points.Add(FrontBot1);

        points.Add(FrontBot2);
        points.Add(BackBot2);
        points.Add(FrontTop2);

        points.Add(FrontBot1);
        points.Add(FrontTop2);
        points.Add(BackBot1);

        points.Add(BackBot2);
        points.Add(BackBot1);
        points.Add(FrontTop2);

        AddPoints(position, points, voxel, context);
    }

    void RenderMiterConcave(Vector3 position, Voxel voxel, MeshContext context)
    {
        var points = new List<Vector3>();
        points.AddRange(GetQuadVertices(Vector3.down));
        points.AddRange(GetQuadVertices(Vector3.forward));
        points.AddRange(GetQuadVertices(Vector3.right));

        // Back
        points.Add(BackBot1);
        points.Add(BackTop2);
        points.Add(BackBot2);

        // Left
        points.Add(FrontBot1);
        points.Add(FrontTop1);
        points.Add(BackBot1);

        points.Add(FrontTop1);
        points.Add(FrontTop2);
        points.Add(BackBot1);

        points.Add(BackTop2);
        points.Add(BackBot1);
        points.Add(FrontTop2);

        AddPoints(position, points, voxel, context);
    }

    private static float GetRotationDegress(Voxel.RotationType rotation)
    {
        switch (rotation)
        {
            case Voxel.RotationType.East:
                return 90.0f;
            case Voxel.RotationType.South:
                return 180.0f;
            case Voxel.RotationType.West:
                return 270.0f;
            default:
                return 0.0f;
        }
    }

    void AddPoints(Vector3 position, List<Vector3> points, Voxel voxel, MeshContext context)
    {
        var rotation = Quaternion.AngleAxis(GetRotationDegress(voxel.Rotation), Vector3.up);
        if (voxel.IsUpsideDown)
        {
            for (var i = points.Count - 1; i >= 0; i--)
            {
                var point = points[i];
                var p2 = new Vector3(point.x, -point.y, point.z);
                context.Verticies.Add(position + (rotation * p2));
            }
        }
        else
        {
            foreach (var point in points)
            {
                context.Verticies.Add(position + (rotation * point));
            }
        }
    }

    void RenderQuad(MeshContext context, Vector3 position, Vector3 normal)
    {
        foreach (var point in GetQuadVertices(normal))
        {
            context.Verticies.Add(position + point);
        }
    }

    IEnumerable<Vector3> GetQuadVertices(Vector3 normal)
    {
        var rotation = Quaternion.FromToRotation(Vector3.up, normal);
        var p1 = rotation * new Vector3(-0.5f, 0, -0.5f);
        var p2 = rotation * new Vector3(0.5f, 0, -0.5f);
        var p3 = rotation * new Vector3(-0.5f, 0, 0.5f);
        var p4 = rotation * new Vector3(0.5f, 0, 0.5f);
        var halfNormal = normal * 0.5f;

        yield return p3 + halfNormal;
        yield return p2 + halfNormal;
        yield return p1 + halfNormal;

        yield return p4 + halfNormal;
        yield return p2 + halfNormal;
        yield return p3 + halfNormal;
    }
}
