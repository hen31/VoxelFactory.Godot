using Godot;

namespace TurtleGames.VoxelEngine;

public class ChunkData
{
    public ChunkVector Position { get; set; }
    public Vector2 Size { get; set; }
    public ushort[,,] Chunk { get; set; }
    public ushort Height { get; set; }
    public bool Calculated { get; set; }

}