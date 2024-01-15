

using Godot;

namespace TurtleGames.VoxelEngine;

[GlobalClass]
public partial class NoiseMapSettings: Resource
{
   [Export]
   public int Seed { get; set; }
   [Export]
   public int Octaves { get; set; } = 4;
   [Export]
   public float Lacunarity { get; set; } = 0.5f;
   [Export]
   public float Persistance { get; set; } = 1.87f;
   [Export]
   public float Scale { get; set; } = 0.0025f;
}