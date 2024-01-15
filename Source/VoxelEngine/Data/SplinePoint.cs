using Godot;

namespace TurtleGames.VoxelEngine;

[GlobalClass]
public partial class SplinePoint : Godot.Resource
{
    [Export]
    public float Point { get; set; }
    [Export]
    public ushort Value { get; set; }
}