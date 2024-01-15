using System;
using Godot;

namespace TurtleGames.VoxelEngine;

public struct ChunkVector
{
    public ChunkVector()
    {
        X = 0;
        Y = 0;
    }

    public ChunkVector(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X { get; set; }
    public int Y { get; set; }

    public override bool Equals(object obj)
    {
        return obj is ChunkVector cVector && cVector.X == X && cVector.Y == Y;
    }

    public bool Equals(ChunkVector other)
    {
        return X == other.X && Y == other.Y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public static ChunkVector operator *(ChunkVector vector, float amount)
    {
        return new ChunkVector((int)vector.X * (int)amount, (int)vector.Y * (int)amount);
    }
    public static ChunkVector operator *(ChunkVector vector, Vector2 vector2)
    {
        return new ChunkVector((int)vector.X * (int)vector2.X, (int)vector.Y * (int)vector2.Y);
    }

    public static bool operator ==(ChunkVector left, ChunkVector right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ChunkVector left, ChunkVector right)
    {
        return !(left == right);
    }

    public Vector3 ToVector3()
    {
        return new Vector3(X, 0, Y);
    }
}