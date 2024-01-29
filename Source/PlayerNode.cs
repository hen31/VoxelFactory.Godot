using Godot;
using TurtleGames.VoxelEngine;

namespace VoxelFactory.Source;

[GlobalClass]
public partial class PlayerNode : Node3D
{
    private CharacterBody3D _characterBody3D;
    private Camera3D _camera;

    [Export] private float MovementSpeed { get; set; } = 4f;
    [Export] private float MouseSensitivity { get; set; } = 0.002f;

    public override void _Ready()
    {
        _characterBody3D = GetParent<CharacterBody3D>();
        _camera = this.GetOrCreate<Camera3D>();
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _PhysicsProcess(double delta)
    {
        _characterBody3D.Velocity = new Vector3(0f, -9.8f, 0f); //Gravity
        _characterBody3D.MoveAndSlide();
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseMotion mouseMotionEvent)
        {
            _characterBody3D.RotateY(-mouseMotionEvent.Relative.X * MouseSensitivity);
            var cameraAngle = Mathf.Clamp(_camera.Rotation.X + -mouseMotionEvent.Relative.Y * MouseSensitivity,
                -1.48f, 1.48f);
            _camera.Rotation = new Vector3(cameraAngle, 0, 0);
        }
    }
}