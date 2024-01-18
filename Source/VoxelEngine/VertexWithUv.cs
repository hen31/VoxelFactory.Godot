using Godot;

namespace TurtleGames.VoxelEngine;

public struct VertexWithUv
{
    public VertexWithUv()
    {
        Position = default;
        TextureCoordinate = default;
        LightIntensity =  0.5f;
    }

    public Vector3 Position { get; set; }
    public Vector2 TextureCoordinate { get; set; }
    public float LightIntensity { get; set; }
}