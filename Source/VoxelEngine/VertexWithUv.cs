using Godot;

namespace TurtleGames.VoxelEngine;

public struct VertexWithUv
{
    public Vector3 Position { get; set; }
    public Vector2 TextureCoordinate { get; set; }
    public float LightIntensity { get; set; }
}