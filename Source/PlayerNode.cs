using Godot;
using TurtleGames.VoxelEngine;

namespace VoxelFactory.Source;

[GlobalClass]
public partial class PlayerNode : Node3D
{
    private CharacterBody3D _characterBody3D;

    public override void _Ready()
    {
        _characterBody3D = GetParent<CharacterBody3D>();
    }

    public override void _PhysicsProcess(double delta)
    {
        _characterBody3D.Velocity = new Vector3(0f, -9.8f, 0f);//Gravity
        _characterBody3D.MoveAndSlide();
    }

}