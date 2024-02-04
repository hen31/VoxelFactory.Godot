using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace TurtleGames.VoxelEngine;

[GlobalClass]
public partial class LightingSystemNode : Node3D
{
    [Export] public ChunkSystemNode ChunkSystemNode { get; set; }

    public override void _Ready()
    {
        ChunkSystemNode.LightSystemTemp = this;
    }

    public void CalculateLighting(ChunkVisualsRequest requestData)
    {
        var allLightSources = new List<Vector3>();
        allLightSources.AddRange(AddLightSources(requestData.ChunkData));
        ProcessLightSources(requestData, allLightSources);
    }

    private void ProcessLightSources(ChunkVisualsRequest visualRequest, List<Vector3> allLightSources)
    {
        Queue<Vector3> _toProcess = new Queue<Vector3>(allLightSources);

        while (_toProcess.TryDequeue(out Vector3 lightPosition))
        {
            var positionData = GetBlockPositionData(visualRequest, lightPosition);
            if (positionData.ChunkData == null)
            {
                continue;
            }

            var currentLightValue = positionData.ChunkData.LightData[(int)positionData.Block.X,
                (int)positionData.Block.Y,
                (int)positionData.Block.Z];

            //spreadLight
            if (lightPosition.Y > 0)
            {
                var globalDownPosition = lightPosition - new Vector3(0,  1, 0);
                var downPosition = GetBlockPositionData(visualRequest, globalDownPosition);
                if (downPosition.ChunkData.Chunk[(int)downPosition.Block.X,
                        (int)downPosition.Block.Y,
                        (int)downPosition.Block.Z] == 0)
                {
                    downPosition.ChunkData.LightData[(int)downPosition.Block.X,
                        (int)downPosition.Block.Y,
                        (int)downPosition.Block.Z] = currentLightValue;
                    _toProcess.Enqueue(globalDownPosition);
                }
                else
                {
                    downPosition.ChunkData.LightData[(int)downPosition.Block.X,
                        (int)downPosition.Block.Y,
                        (int)downPosition.Block.Z] = currentLightValue;
                }
            }
        }
    }

    private (ChunkData ChunkData, Vector3 Block) GetBlockPositionData(ChunkVisualsRequest visualRequest,
        Vector3 lightPosition)
    {
        return (visualRequest.ChunkData, lightPosition);
    }


    private List<Vector3> AddLightSources(ChunkData chunk)
    {
        var sources = new List<Vector3>();
        chunk.LightData = new byte[(int)chunk.Size.X, chunk.Height, (int)chunk.Size.Y];
        for (int x = 0; x < chunk.Size.X; x++)
        {
            for (int y = 0; y < chunk.Height; y++)
            {
                for (int z = 0; z < chunk.Size.Y; z++)
                {
                    if (y == chunk.Height - 1)
                    {
                        chunk.LightData[x, y, z] = 255;
                        sources.Add(new Vector3(
                            x,
                            y,
                            z));
                    }
                }
            }
        }

        return sources;
    }
}