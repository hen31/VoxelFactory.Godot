using System.Collections.Generic;

namespace TurtleGames.VoxelEngine;

public class VoxelGameState 
{
    public Dictionary<ChunkVector, ChunkData> Chunks { get; set; } = new();
}