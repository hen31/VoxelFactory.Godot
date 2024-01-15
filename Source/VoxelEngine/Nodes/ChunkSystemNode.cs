using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

namespace TurtleGames.VoxelEngine;

[GlobalClass]
public partial class ChunkSystemNode : Node3D
{
	private Node3D _cameraTransform;
	[Export] private ChunkGeneratorNode _chunkGenerator;
	[Export] private ChunkVisualsGeneratorNode _chunkVisualGenerator;
	[Export] public Camera3D Camera { get; set; }
	public VoxelGameState GameState { get; set; } = new();
	[Export] private Vector2 _chunkSize;

	[Export] public int Radius { get; set; } = 2;
	[Export] public float VoxelSize { get; set; } = 1;

	[Export] public bool OnlyInitialGeneration { get; set; } = false;

	[Export] public Material BlockMaterial { get; set; }

	private List<ChunkVisualNode> _currentVisuals = new();

	public List<BlockType> BlockTypes = new()
	{
		new BlockType()
		{
			BlockId = 1,
			Name = "Stone",
			BlockTexture = new BlockTextureInfo()
			{
				DefaultTexture = "stone.png"
			}
		},
		new BlockType()
		{
			BlockId = 2,
			Name = "Dirt",
			BlockTexture = new BlockTextureInfo()
			{
				DefaultTexture = "dirt.png"
			}
		},
		new BlockType()
		{
			BlockId = 3,
			Name = "Grass",
			BlockTexture = new BlockTextureInfo()
			{
				DefaultTexture = "dirt.png",
				TopTexture = "grass_block_top.png"
			}
		}
	};

	public override void _Ready()
	{
		var material =
			new BlockTextureCreator(BlockTypes).CreateBlockTextureAtlas(
				out Dictionary<ushort, BlockTextureUvMapping> blockMapping);
		_chunkVisualGenerator.BlockUvMapping = blockMapping;
		_chunkVisualGenerator.Material = BlockMaterial;
		_chunkSize = _chunkGenerator.ChunkSize;
	}

	private bool _once;

	public override void _Process(double delta)
	{
		if (OnlyInitialGeneration)
		{
			if (_once)
			{
				return;
			}
			else
			{
				_once = true;
			}
		}

		var currentPositionInChunkPositions = ToChunkPosition(Camera.GlobalPosition);
		var toDelete = _currentVisuals.ToList();

		for (int x = (int)currentPositionInChunkPositions.X - Radius;
			 x < currentPositionInChunkPositions.X + Radius;
			 x++)
		{
			for (int y = (int)currentPositionInChunkPositions.Y - Radius;
				 y < currentPositionInChunkPositions.Y + Radius;
				 y++)
			{
				var newPosition = new ChunkVector(x, y);
				var chunkData = GetChunkAt(newPosition);


				var currentVisual = _currentVisuals.FirstOrDefault(b => b.ChunkData == chunkData);
				if (currentVisual != null)
				{
					toDelete.Remove(currentVisual);
				}
				else
				{
					var neighbours = new ChunkData[4];
					neighbours[0] = GetChunkAt(new ChunkVector(x, y + 1));
					neighbours[1] = GetChunkAt(new ChunkVector(x + 1, y));
					neighbours[2] = GetChunkAt(new ChunkVector(x, y - 1));
					neighbours[3] = GetChunkAt(new ChunkVector(x - 1, y));

					var visualizationNode = new ChunkVisualNode();

					visualizationNode.Position = new Vector3(x * _chunkSize.X * VoxelSize,
						-_chunkGenerator.ChunkHeight / 2f * VoxelSize,
						y * _chunkSize.Y * VoxelSize);

					visualizationNode.ChunkData = chunkData;
					visualizationNode.Material = BlockMaterial;
					visualizationNode.VoxelSize = VoxelSize;
					visualizationNode.VisualGeneratorNode = _chunkVisualGenerator;
					visualizationNode.Neighbours = neighbours;
					AddChild(visualizationNode);

					_currentVisuals.Add(visualizationNode);
				}
			}
		}


		foreach (var visualNotLongerInRange in toDelete)
		{
			//   Entity.Scene.Entities.Remove(visualNotLongerInRange.Entity);
			_currentVisuals.Remove(visualNotLongerInRange);
		}
	}

	private ChunkData GetChunkAt(ChunkVector newPosition)
	{
		ChunkData chunkData = null;

		if (!GameState.Chunks.TryGetValue(newPosition, out chunkData))
		{
			chunkData = _chunkGenerator.QueueNewChunkForCalculation(newPosition);
			GameState.Chunks.Add(newPosition, chunkData);
		}

		return chunkData;
	}

	private Vector2 ToChunkPosition(Vector3 cameraTransformPosition)
	{
		int x = (int)MathF.Round(cameraTransformPosition.X / _chunkSize.X / VoxelSize);
		int y = (int)MathF.Round(cameraTransformPosition.Z / _chunkSize.Y / VoxelSize);
		return new Vector2(x, y);
	}

	public (ChunkVector Chunk, Vector3 Block, ChunkData ChunkData) PointToChunkPosition(Vector3 point)
	{
		var chunkPosition = ToChunkPosition(point).ToChunkVector();
		var inChunkPosition = PointToInChunkPosition(point, chunkPosition);
		var chunkData = GetChunkAt(chunkPosition);

		return (chunkPosition, inChunkPosition, chunkData);
	}

	private Vector3 PointToInChunkPosition(Vector3 point, ChunkVector chunkPosition)
	{
		var pointInSideChunk = point - (chunkPosition * _chunkSize * VoxelSize).ToVector3() +
							   new Vector3(_chunkGenerator.ChunkSize.X / 2f, _chunkGenerator.ChunkHeight / 2f,
								   _chunkGenerator.ChunkSize.Y / 2f);

		int x = (int)(pointInSideChunk.X / VoxelSize);
		int y = (int)(pointInSideChunk.Y / VoxelSize);
		int z = (int)(pointInSideChunk.Z / VoxelSize);
		return new Vector3(x, y, z);
	}

	public void DestroyBlock(ChunkData chunkData, (ChunkVector Chunk, Vector3 Block, ChunkData ChunkData) point)
	{
		chunkData.Chunk[(int)point.Block.X, (int)point.Block.Y, (int)point.Block.Z] = 0;
		_currentVisuals.FirstOrDefault(b =>
			b.ChunkData.Position.X == chunkData.Position.X && b.ChunkData.Position.Y == chunkData.Position.Y)?.Remesh();
		if ((int)point.Block.X == 0)
		{
			_currentVisuals.FirstOrDefault(b =>
					b.ChunkData.Position.X == chunkData.Position.X - 1 &&
					b.ChunkData.Position.Y == chunkData.Position.Y)
				?.Remesh();
		}
		else if ((int)point.Block.X == (int)_chunkSize.X - 1)
		{
			_currentVisuals.FirstOrDefault(b =>
					b.ChunkData.Position.X == chunkData.Position.X + 1 &&
					b.ChunkData.Position.Y == chunkData.Position.Y)
				?.Remesh();
		}

		if ((int)point.Block.Z == 0)
		{
			_currentVisuals.FirstOrDefault(b =>
					b.ChunkData.Position.X == chunkData.Position.X &&
					b.ChunkData.Position.Y == chunkData.Position.Y - 1)
				?.Remesh();
		}
		else if ((int)point.Block.Z == (int)_chunkSize.Y - 1)
		{
			_currentVisuals.FirstOrDefault(b =>
					b.ChunkData.Position.X == chunkData.Position.X &&
					b.ChunkData.Position.Y == chunkData.Position.Y + 1)
				?.Remesh();
		}
	}
}
