using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace TurtleGames.VoxelEngine;

[GlobalClass]
public partial class LightingSystemNode : Node3D
{
    [Export] public ChunkSystemNode ChunkSystemNode { get; set; }


    public void DoCalculation(ChunkVisualsRequest request)
    {
        for (int i = 0; i < request.Neighbours.Length; i++)
        {
            var neighbour = request.Neighbours[i];
            if (neighbour.LightCalculated == false)
            {
                neighbour = neighbour.CloneIfNeededForLightingCalculations();
                request.Neighbours[i] = neighbour;
                CalculateLighting(neighbour, ChunkSystemNode.GetNeighbours(neighbour.Position.X, neighbour.Position.Y));
            }
        }

        CalculateLighting(request.ChunkData, request.Neighbours);
        request.ChunkData.LightCalculated = true;
    }

    private CancellationTokenSource _cancellationToken;
    private ConcurrentQueue<ChunkVisualsRequest> _calculationQueue = new ConcurrentQueue<ChunkVisualsRequest>();
    private int _threadCount = 1;

    public void CalculateLighting(ChunkData chunk, ChunkData[] neighbours)
    {
        var allLightSources = new List<Vector3>();
        allLightSources.AddRange(AddLightSources(chunk));
        ProcessLightSources(chunk, neighbours, allLightSources);
    }

    private void ProcessLightSources(ChunkData chunk, ChunkData[] neighbours, List<Vector3> allLightSources)
    {
        Queue<Vector3> toProcess = new Queue<Vector3>(allLightSources);

        while (toProcess.TryDequeue(out Vector3 lightPosition))
        {
            var positionData = GetBlockPositionData(chunk, neighbours, lightPosition);
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
                var globalDownPosition = lightPosition - new Vector3(0, 1, 0);
                var downPosition = GetBlockPositionData(chunk, neighbours, globalDownPosition);
                if (SpreadLight(downPosition, currentLightValue))
                {
                    toProcess.Enqueue(globalDownPosition);
                }
            }

            /*   if (lightPosition.X > 0)
               {
                   var leftPosition = lightPosition - new Vector3(1, 0, 0);
                   var leftPositionData = GetBlockPositionData(visualRequest, leftPosition);
                   if (SpreadLight(leftPositionData, currentLightValue))
                   {
                       toProcess.Enqueue(leftPosition);
                   }
               }

               if (lightPosition.X < visualRequest.ChunkData.Size.X - 1)
               {
                   var rightPosition = lightPosition + new Vector3(1, 0, 0);
                   var rightPositionData = GetBlockPositionData(visualRequest, rightPosition);
                   if (SpreadLight(rightPositionData, currentLightValue))
                   {
                       toProcess.Enqueue(rightPosition);
                   }
               }*/
        }
    }

    private static bool SpreadLight((ChunkData ChunkData, Vector3 Block) downPosition, byte currentLightValue)
    {
        if (downPosition.ChunkData.Chunk[(int)downPosition.Block.X,
                (int)downPosition.Block.Y,
                (int)downPosition.Block.Z] == 0
            && downPosition.ChunkData.LightData[(int)downPosition.Block.X,
                (int)downPosition.Block.Y,
                (int)downPosition.Block.Z] < currentLightValue)
        {
            downPosition.ChunkData.LightData[(int)downPosition.Block.X,
                (int)downPosition.Block.Y,
                (int)downPosition.Block.Z] = currentLightValue;
            return true;
        }

        return false;
    }

    private (ChunkData ChunkData, Vector3 Block) GetBlockPositionData(ChunkData chunk, ChunkData[] neighbours,
        Vector3 lightPosition)
    {
        return (chunk, lightPosition);
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
                    else
                    {
                        chunk.LightData[x, y, z] = 0;
                    }
                }
            }
        }

        return sources;
    }
}