using Godot;
using TurtleGames.VoxelEngine;

namespace VoxelFactory.Source;

[GlobalClass]
public partial class PlayerNode : Node3D
{
	private CharacterBody3D _characterBody3D;
	private Camera3D _camera;

	[Export] private float MovementSpeed { get; set; } = 4f;
	[Export] private float JumpSpeed { get; set; } = 6f;
	[Export] private float MouseSensitivity { get; set; } = 0.002f;
	[Export] private float Gravity { get; set; } = -9.8f;
	[Export] private ChunkSystemNode ChunkSystem { get; set; }

	public override void _Ready()
	{
		_characterBody3D = GetParent<CharacterBody3D>();
		_camera = this.GetOrCreate<Camera3D>();
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _PhysicsProcess(double delta)
	{
		_characterBody3D.Velocity =
			new Vector3(_characterBody3D.Velocity.X, _characterBody3D.Velocity.Y + Gravity * (float)delta,
				_characterBody3D.Velocity.Z);
		SetVelocityFromInputs();
		_characterBody3D.MoveAndSlide();


		if (Input.IsActionPressed("primary_action"))
		{
			var result = _camera.RayCast((uint)VoxelCollisionLayers.Voxel, 10f);
			if (result != null)
			{
				var worldPosition = (Vector3)result["position"];
				var hitPoint = worldPosition - (Vector3)result["normal"] * 0.5f;
				var chunkVisual = ((Node)result["collider"]).GetParent<ChunkVisualNode>();

				var point = ChunkSystem.PointToChunkPosition(hitPoint);
				var chunkDataFromRay = chunkVisual?.ChunkData;
				if (chunkDataFromRay == null)
				{
					return;
				}

				ChunkSystem.DestroyBlock(chunkDataFromRay, point);
			}
		}
	}

	private void SetVelocityFromInputs()
	{
		var input = Input.GetVector("movement_left", "movement_right", "movement_forward", "movement_backward");
		var direction = _characterBody3D.Transform.Basis * new Vector3(input.X, 0, input.Y);
		_characterBody3D.Velocity = new Vector3(direction.X * MovementSpeed, _characterBody3D.Velocity.Y,
			direction.Z * MovementSpeed);
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
		else if (inputEvent is InputEventKey keyEvent && keyEvent.IsActionPressed("jump") &&
				 _characterBody3D.IsOnFloor())
		{
			_characterBody3D.Velocity =
				new Vector3(_characterBody3D.Velocity.X, JumpSpeed, _characterBody3D.Velocity.Z);
		}
	}
}
