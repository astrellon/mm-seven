using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Chunk))]
public class ChunkRender : MonoBehaviour
{
    private class MeshContext
    {
        public List<Vector3> Verticies = new List<Vector3>();
        public Vector3 WorldPosition = Vector3.zero;

        public void AddVertex(Vector3 vertex)
        {
            var worldVertex = vertex + WorldPosition;
            var dx = (Mathf.PerlinNoise(worldVertex.x, worldVertex.y) - 0.5f) * 0.3f;
            var dy = (Mathf.PerlinNoise(worldVertex.y, worldVertex.z) - 0.5f) * 0.3f;
            var dz = (Mathf.PerlinNoise(worldVertex.z, worldVertex.x) - 0.5f) * 0.3f;

            Verticies.Add(new Vector3(vertex.x + dx, vertex.y + dy, vertex.z + dz));
        }
    }

    private static Vector3 FrontTop1 = new Vector3(-0.5f, 0.5f, 0.5f);
    private static Vector3 FrontTop2 = new Vector3(0.5f, 0.5f, 0.5f);
    private static Vector3 FrontBot1 = new Vector3(-0.5f, -0.5f, 0.5f);
    private static Vector3 FrontBot2 = new Vector3(0.5f, -0.5f, 0.5f);
    private static Vector3 BackTop1 = new Vector3(-0.5f, 0.5f, -0.5f);
    private static Vector3 BackTop2 = new Vector3(0.5f, 0.5f, -0.5f);
    private static Vector3 BackBot1 = new Vector3(-0.5f, -0.5f, -0.5f);
    private static Vector3 BackBot2 = new Vector3(0.5f, -0.5f, -0.5f);

    [System.Flags]
    public enum Direction : byte
    {
        XPos = 0x00,
        XNeg = 0x01,
        YPos = 0x02,
        YNeg = 0x04,
        ZPos = 0x08,
        ZNeg = 0x10
    }
    private static Vector3 DirectionToVector(Direction direction)
    {
        switch (direction)
        {
            case Direction.XNeg:
                return Vector3.left;
            case Direction.XPos:
                return Vector3.right;
            case Direction.YNeg:
                return Vector3.down;
            case Direction.YPos:
                return Vector3.up;
            case Direction.ZNeg:
                return Vector3.back;
            case Direction.ZPos:
                return Vector3.forward;
        }
        return Vector3.zero;
    }
    private static Direction[] CardinalDirections = new[] { Direction.ZPos, Direction.XPos, Direction.ZNeg, Direction.XNeg };

    public Chunk Chunk { get; private set; }

    private Dictionary<ushort, MeshContext> TypedBlocks = new Dictionary<ushort, MeshContext>();

    private struct QuadPosition
    {
        public byte x, y, z;
        public MeshContext context;
        public Direction direction;

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + x.GetHashCode();
            hash = hash * 23 + y.GetHashCode();
            hash = hash * 23 + z.GetHashCode();
            hash = hash * 23 + direction.GetHashCode();

            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj is QuadPosition)
            {
                var quadPos = (QuadPosition)obj;

                return x == quadPos.x &&
                    y == quadPos.y &&
                    z == quadPos.z &&
                    direction == quadPos.direction;
            }
            return false;
        }

        public QuadPosition(Vector3 position, MeshContext context, Direction direction)
        {
            this.x = (byte)position.x;
            this.y = (byte)position.y;
            this.z = (byte)position.z;
            this.context = context;
            this.direction = direction;
        }
    }
        private static Direction FlipDirection(Direction input)
        {
            switch (input)
            {
                case Direction.XNeg:
                    return Direction.XPos;
                case Direction.XPos:
                    return Direction.XNeg;
                case Direction.YNeg:
                    return Direction.YPos;
                case Direction.YPos:
                    return Direction.YNeg;
                case Direction.ZNeg:
                    return Direction.ZPos;
                case Direction.ZPos:
                    return Direction.ZNeg;
            }
            return Direction.XNeg;
        }

        private static Vector3 MoveByDirection(Vector3 position, Direction direction)
        {
            switch (direction)
            {
                case Direction.XNeg:
                    return new Vector3(position.x - 1, position.y, position.z);
                case Direction.XPos:
                    return new Vector3(position.x + 1, position.y, position.z);
                case Direction.YNeg:
                    return new Vector3(position.x, position.y - 1, position.z);
                case Direction.YPos:
                    return new Vector3(position.x, position.y + 1, position.z);
                case Direction.ZNeg:
                    return new Vector3(position.x, position.y, position.z - 1);
                case Direction.ZPos:
                    return new Vector3(position.x, position.y, position.z + 1);
            }
            return Vector3.zero;
        }

    private HashSet<QuadPosition> QuadPositions = new HashSet<QuadPosition>();

    // Use this for initialization
    void Start()
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
            result.WorldPosition = transform.position;
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

        foreach (var quadPos in QuadPositions)
        {
            RenderQuad(quadPos.context, new Vector3(quadPos.x, quadPos.y, quadPos.z), DirectionToVector(quadPos.direction));
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
        meshObject.transform.localPosition = Vector3.zero;

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
        RenderQuad2(context, voxel, position, Direction.XNeg);
        RenderQuad2(context, voxel, position, Direction.XPos);

        RenderQuad2(context, voxel, position, Direction.YNeg);
        RenderQuad2(context, voxel, position, Direction.YPos);

        RenderQuad2(context, voxel, position, Direction.ZNeg);
        RenderQuad2(context, voxel, position, Direction.ZPos);
    }

    void RenderRamp(Vector3 position, Voxel voxel, MeshContext context)
    {
        RenderQuad2(context, voxel, position, Direction.YNeg);
        RenderQuad2(context, voxel, position, Direction.ZPos);
        var points = new List<Vector3>();

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
        RenderQuad2(context, voxel, position, Direction.ZPos);
        RenderQuad2(context, voxel, position, Direction.XPos);
        RenderQuad2(context, voxel, position, Direction.YNeg);

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
        RenderQuad2(context, voxel, position, Direction.YNeg);

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
        RenderQuad2(context, voxel, position, Direction.YNeg);
        RenderQuad2(context, voxel, position, Direction.ZPos);
        RenderQuad2(context, voxel, position, Direction.XPos);

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

    private static Direction TransformDirection(Direction input, Voxel voxel)
    {
        if (voxel.IsUpsideDown)
        {
            if (input == Direction.YPos)
            {
                return Direction.YNeg;
            }
            else if (input == Direction.YNeg)
            {
                return Direction.YPos;
            }
        }

        if (input == Direction.YPos || input == Direction.YNeg)
        {
            return input;
        }

        var rotationIndex = 0;
        switch (voxel.Rotation)
        {
            case Voxel.RotationType.East:
                rotationIndex = 1;
                break;
            case Voxel.RotationType.South:
                rotationIndex = 2;
                break;
            case Voxel.RotationType.West:
                rotationIndex = 3;
                break;
        }

        var directionIndex = 0;
        switch (input)
        {
            case Direction.XPos:
                directionIndex = 1;
                break;
            case Direction.ZNeg:
                directionIndex = 2;
                break;
            case Direction.XNeg:
                directionIndex = 3;
                break;
        }

        var finalIndex = rotationIndex + directionIndex;
        if (finalIndex >= 4)
        {
            finalIndex -= 4;
        }

        return CardinalDirections[finalIndex];
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
                context.AddVertex(position + (rotation * p2));
            }
        }
        else
        {
            foreach (var point in points)
            {
                context.AddVertex(position + (rotation * point));
            }
        }
    }

    void RenderQuad2(MeshContext context, Voxel voxel, Vector3 position, Direction direction)
    {
        var transformedDirection = TransformDirection(direction, voxel);
        var quadPos = new QuadPosition(position, context, transformedDirection);

        var flippedDir = FlipDirection(transformedDirection);
        var flippedPosition = MoveByDirection(position, transformedDirection);
        var flippedQuad = new QuadPosition(flippedPosition, context, flippedDir);

        if (QuadPositions.Contains(flippedQuad))
        {
            QuadPositions.Remove(flippedQuad);
            return;
        }
        QuadPositions.Add(quadPos);
    }

    void RenderQuad(MeshContext context, Vector3 position, Vector3 normal)
    {
        foreach (var point in GetQuadVertices(normal))
        {
            context.AddVertex(position + point);
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
