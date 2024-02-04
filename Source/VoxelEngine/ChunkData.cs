using System;
using Godot;

namespace TurtleGames.VoxelEngine;

public class ChunkData
{
    public ChunkVector Position { get; set; }
    public Vector2 Size { get; set; }
    public ushort[,,] Chunk { get; set; }
    public byte[,,] LightData { get; set; }
    public ushort Height { get; set; }
    public bool Calculated { get; set; }
    public bool LightCalculated { get; set; }

    public ChunkData CloneIfNeededForLightingCalculations()
    {
        var clone = new ChunkData();
        clone.Position = Position;
        clone.Height = Height;
        clone.Size = Size;
        clone.Chunk = Chunk;
        clone.LightData = new byte[(int)Size.X, Height, (int)Size.Y];
        return clone;

    }
}