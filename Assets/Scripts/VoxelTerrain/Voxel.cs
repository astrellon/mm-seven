using UnityEngine;
using System.Collections;

public struct Voxel
{
    public enum MeshShapeType : byte
    {
        None = 0x00,
        Cube = 0x01,
        Ramp = 0x02,
        SmallCorner = 0x03,
        LargeCorner = 0x04,
        MiterConvex = 0x05,
        MiterConcave = 0x06,
        Reserved = 0x07
    }
    public enum RotationType : byte
    {
        North = 0x00 << 0x03,
        East = 0x01 << 0x03,
        South = 0x02 << 0x03,
        West = 0x03 << 0x03
    }
    public const byte UpsideDownFlag = 0x01 << 0x05;

    public readonly byte ShapeData;
    public readonly ushort BlockType;

    public Voxel(MeshShapeType meshShape, RotationType rotation, bool upsideDown, ushort blockType)
    {
        ShapeData = CalcShape(meshShape, rotation, upsideDown);
        BlockType = blockType;
    }

    public MeshShapeType MeshShape
    {
        get
        {
            return (MeshShapeType)(ShapeData & 0x07);
        }
    }
    public RotationType Rotation
    {
        get
        {
            return (RotationType)(ShapeData & 0x18);
        }
    }
    public bool IsUpsideDown
    {
        get
        {
            return (ShapeData & UpsideDownFlag) > 0;
        }
    }

    public static byte CalcShape(MeshShapeType meshShape, RotationType rotation, bool upsideDown)
    {
        var result = (byte)((byte)meshShape | (byte)rotation);
        if (upsideDown)
        {
            result |= UpsideDownFlag;
        }
        return result;
    }

    public override string ToString()
    {
        return string.Format("Voxel {0} {1} {2}", MeshShape, Rotation, IsUpsideDown ? "upside down" : "upwards");
    }

}
