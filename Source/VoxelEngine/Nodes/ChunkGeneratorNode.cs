using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace TurtleGames.VoxelEngine;

[GlobalClass]
public partial class ChunkGeneratorNode : Node
{
    [Export] public ushort ChunkHeight { get; set; }
    [Export] public Vector2 ChunkSize { get; set; }
    public bool DebugWrite { get; set; }

    [Export]
    public NoiseWithSpline Continentalness { get; set; }

    [Export] public NoiseWithSpline Errosion { get; set; }
    private ConcurrentQueue<ChunkData> _calculationQueue = new ConcurrentQueue<ChunkData>();

    private CancellationTokenSource _cancellationToken;

    private void InitializeNoiseGenerators()
    {
        Continentalness.Initialize();
        Errosion.Initialize();
    }

    public ChunkData QueueNewChunkForCalculation(ChunkVector position)
    {
        var chunk = new ChunkData()
        {
            Size = ChunkSize,
            Position = position,
            Height = ChunkHeight,
            Chunk = new ushort[(int)ChunkSize.X, ChunkHeight, (int)ChunkSize.Y],
            Calculated = false
        };
        _calculationQueue.Enqueue(chunk);
        return chunk;
    }


    public void GenerateChunk(ChunkData chunkData)
    {
        Random tempRandom = new Random();

        var chunkPosition = new Vector2(chunkData.Position.X, chunkData.Position.Y) * ChunkSize;
        for (int x = 0; x < ChunkSize.X; x++)
        {
            float xPosition = (x + chunkPosition.X);
            for (int z = 0; z < ChunkSize.Y; z++)
            {
                float zPosition = (z + chunkPosition.Y);

                int baseHeight = 100 + Continentalness.GetValue(xPosition, zPosition) +
                                 Errosion.GetValue(xPosition, zPosition);
                for (int y = 0; y < ChunkHeight; y++)
                {
                    if (y < baseHeight)
                    {
                        chunkData.Chunk[x, y, z] = tempRandom.Next(0, 100) < 50 ? (ushort)1 : (ushort)2;
                    }
                    else
                    {
                        chunkData.Chunk[x, y, z] = 0;
                    }
                }
            }
        }
    }

    public override void _Ready()
    {
        InitializeNoiseGenerators();
        _cancellationToken = new CancellationTokenSource();
        var token = _cancellationToken.Token;
        Task.Factory.StartNew(() => RunCalculationThread(token), TaskCreationOptions.LongRunning);
    }

    public void Update()
    {
        /*  if (Input.IsKeyPressed(Keys.N))
          {
              var bitmapCon = new Bitmap(1920, 1080);
              var bitmapEr = new Bitmap(1920, 1080);

              for (var x = 0; x < bitmapCon.Width; x++)
              {
                  for (var y = 0; y < bitmapCon.Height; y++)
                  {
                      var value = (int)MathUtils.Map(Continentalness.GetValue(x, y),
                          Continentalness.NoiseSpline.SplinePoints.Min(b => b.Value),
                          Continentalness.NoiseSpline.SplinePoints.Max(b => b.Value), 0, 254);
                      //Lerp(0,255,_continentalness.GetNoise(x,y)
                      var color = System.Drawing.Color.FromArgb(
                          value,
                          value, value);
                      bitmapCon.SetPixel(x, y, color);

                      value = (int)MathUtils.Map(Errosion.GetValue(x, y),
                          Errosion.NoiseSpline.SplinePoints.Min(b => b.Value),
                          Errosion.NoiseSpline.SplinePoints.Max(b => b.Value), 0, 254);
                      //Lerp(0,255,_continentalness.GetNoise(x,y)
                      color = System.Drawing.Color.FromArgb(
                          value,
                          value, value);
                      bitmapEr.SetPixel(x, y, color);
                  }
              }

              bitmapCon.Save("C:\\Development\\Noises\\c.bmp");
              bitmapEr.Save("C:\\Development\\Noises\\e.bmp");
          }*/
    }


    private void RunCalculationThread(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            RunCalculations();
        }
    }

    public void Cancel()
    {
        _cancellationToken?.Cancel();
        _cancellationToken?.Dispose();
    }

    public void RunCalculations()
    {
        if (_calculationQueue.TryDequeue(out ChunkData toCalculate))
        {
            Debug.WriteLineIf(DebugWrite,
                $"Start calculation for cunk X:{toCalculate.Position.X},Y:{toCalculate.Position.Y}");
            GenerateChunk(toCalculate);
            toCalculate.Calculated = true;
            Debug.WriteLineIf(DebugWrite,
                $"End calculation for cunk X:{toCalculate.Position.X},Y:{toCalculate.Position.Y}");
        }
        else
        {
            Thread.Sleep(1);
        }
    }
}