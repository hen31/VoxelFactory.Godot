using Godot;

namespace TurtleGames.VoxelEngine;

[GlobalClass]
public partial class LightingSystemNode : Node3D
{
    [Export] public ChunkSystemNode ChunkSystemNode { get; set; }


    public void CalculateLighting()
    {
        var chunks = ChunkSystemNode.GetActiveChunks();

     /*   foreach (var chunk in chunks.Values)
        {
            chunk.LightingData = "";
        }*/
    }
}