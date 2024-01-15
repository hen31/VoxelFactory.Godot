using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace TurtleGames.VoxelEngine;

[GlobalClass]
public partial class ChunkVisualsGeneratorNode : Node
{
    [Export] private float _voxelSize = 1f;
    private CancellationTokenSource _cancellationToken;
    private ConcurrentQueue<ChunkVisualsRequest> _calculationQueue = new ConcurrentQueue<ChunkVisualsRequest>();

    public Dictionary<ushort, BlockTextureUvMapping> BlockUvMapping { get; set; }
    public Material Material { get; set; }
    public override void _Ready()
    {
        //_voxelSize = Entity.Get<ChunkSystemComponent>().VoxelSize;

        _cancellationToken = new CancellationTokenSource();
        var token = _cancellationToken.Token;
        Task.Factory.StartNew(() => RunCalculationThread(token), TaskCreationOptions.LongRunning);
    }

    public void Cancel()
    {
        _cancellationToken?.Cancel();
    }

    private void RunCalculationThread(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            DoCalculation();
        }
    }

    private void DoCalculation()
    {
        if (_calculationQueue.TryDequeue(out ChunkVisualsRequest request))
        {
            ChunkVisualData chunkVisualData = new ChunkVisualData();
            chunkVisualData.Vertexes = new List<VertexWithUv>();
            chunkVisualData.Indexes = new List<int>();

            CalculateModel(request.ChunkData, request.Neighbours, chunkVisualData.Vertexes, chunkVisualData.Indexes,
                BlockUvMapping);
            request.Model = GenerateVisuals(chunkVisualData.Vertexes, chunkVisualData.Indexes, Material);
            request.VisualsData = chunkVisualData;
            request.IsCalculated = true;
        }
        else
        {
            Thread.Sleep(1);
        }
    }


    private void CalculateModel(ChunkData chunkData, ChunkData[] neighbourChunks,
        List<VertexWithUv> vertices,
        List<int> indexes, Dictionary<ushort, BlockTextureUvMapping> blockUvMapping)
    {
        var offSet = chunkData.Size / 2f * _voxelSize;
        for (int x = 0; x < chunkData.Size.X; x++)
        {
            for (int y = 0; y < chunkData.Height; y++)
            {
                for (int z = 0; z < chunkData.Size.Y; z++)
                {
                    if (chunkData.Chunk[x, y, z] > 0)
                    {
                        var neighbours = CalculateNeighbours(chunkData, neighbourChunks, x, y, z);

                        CreateCubeMesh(vertices, indexes, neighbours, x, y, z, -offSet,
                            new BlockTextureUvMapping()
                            {
                            }); //blockUvMapping[chunkData.Chunk[x, y, z]]
                    }
                }
            }
        }
    }

    private static int[] CalculateNeighbours(ChunkData chunkData, ChunkData[] neighbourChunks, int x, int y, int z)
    {
        var neighbours = new int[]
        {
            0, //Left 0
            0, //Back 1
            0, //Right 2
            0, //Front 3
            0, //Top 4
            0 //Bottom 5
        };

        neighbours[0] = x == 0
            ? neighbourChunks[3].Chunk[(int)chunkData.Size.X - 1, y, z]
            : chunkData.Chunk[x - 1, y, z];
        neighbours[2] = x == (int)chunkData.Size.X - 1
            ? neighbourChunks[1].Chunk[0, y, z]
            : chunkData.Chunk[x + 1, y, z];

        neighbours[1] = z == 0
            ? neighbourChunks[2].Chunk[x, y, (int)chunkData.Size.Y - 1]
            : chunkData.Chunk[x, y, z - 1];
        neighbours[3] = z == (int)chunkData.Size.Y - 1
            ? neighbourChunks[0].Chunk[x, y, 0]
            : chunkData.Chunk[x, y, z + 1];


        neighbours[5] = y == 0 ? 0 : chunkData.Chunk[x, y - 1, z];
        neighbours[4] = y == chunkData.Height - 1 ? 0 : chunkData.Chunk[x, y + 1, z];
        return neighbours;
    }

    private void CreateCubeMesh(List<VertexWithUv> vertices, List<int> indexes, int[] neighBours, int x,
        int y, int z,
        Vector2 offSet,
        BlockTextureUvMapping blockUvMapping)
    {
        var offsetWithHeight = new Vector3(offSet.X, 0, offSet.Y);
        if (neighBours[2] == 0)
        {
            AddRightSide(vertices, indexes, x, y, z, offsetWithHeight, blockUvMapping.LeftSide, blockUvMapping.UvScale);
        }

        if (neighBours[0] == 0)
        {
            AddLeftSide(vertices, indexes, x, y, z, offsetWithHeight, blockUvMapping.LeftSide, blockUvMapping.UvScale);
        }

        if (neighBours[4] == 0)
        {
            AddTopSide(vertices, indexes, x, y, z, offsetWithHeight, blockUvMapping.TopSide, blockUvMapping.UvScale);
        }

        if (neighBours[5] == 0)
        {
            AddBottomSide(vertices, indexes, x, y, z, offsetWithHeight, blockUvMapping.BottomSide,
                blockUvMapping.UvScale);
        }

        if (neighBours[3] == 0)
        {
            AddFrontSide(vertices, indexes, x, y, z, offsetWithHeight, blockUvMapping.FrontSide,
                blockUvMapping.UvScale);
        }

        if (neighBours[1] == 0)
        {
            AddBackSide(vertices, indexes, x, y, z, offsetWithHeight, blockUvMapping.BackSide, blockUvMapping.UvScale);
        }
    }

    private void AddBackSide(List<VertexWithUv> vertices, List<int> indexes, int x, int y, int z,
        Vector3 offSet, Vector2 uvPositionTop, Vector2 uvScale)
    {
        //var normal = Vector3.UnitZ;
        var startIndex = vertices.Count;
        var vectorOfPosition = new Vector3((x * _voxelSize), (y * _voxelSize), (z * _voxelSize));
        var sideVertexes = new VertexWithUv[4];
        sideVertexes[0].Position = vectorOfPosition + new Vector3(0f, 0f, 0f) + offSet;
        sideVertexes[0].TextureCoordinate = uvPositionTop + uvScale;
        //sideVertexes[0].Normal = normal;

        sideVertexes[1].Position = vectorOfPosition + new Vector3(0f, _voxelSize, 0f) + offSet;
        sideVertexes[1].TextureCoordinate = uvPositionTop + new Vector2(uvScale.X, 0);
        //  sideVertexes[1].Normal = normal;

        sideVertexes[2].Position = vectorOfPosition + new Vector3(_voxelSize, _voxelSize, 0f) + offSet;
        sideVertexes[2].TextureCoordinate = uvPositionTop;
        //sideVertexes[2].Normal = normal;

        sideVertexes[3].Position = vectorOfPosition + new Vector3(_voxelSize, 0f, 0f) + offSet;
        sideVertexes[3].TextureCoordinate = uvPositionTop + new Vector2(0, uvScale.Y);
        //sideVertexes[3].Normal = normal;

        int[] indices = { 1, 2, 0, 0, 2, 3 };

        vertices.AddRange(sideVertexes);
        indexes.AddRange(indices.Select(b => b + startIndex).Reverse());
    }

    private void AddFrontSide(List<VertexWithUv> vertices, List<int> indexes, int x, int y, int z,
        Vector3 offSet, Vector2 uvPositionTop, Vector2 uvScale)
    {
        //  var normal = Vector3.UnitZ;

        var startIndex = vertices.Count;
        var vectorOfPosition = new Vector3((x * _voxelSize), (y * _voxelSize), (z * _voxelSize));
        var sideVertexes = new VertexWithUv[4];
        sideVertexes[0].Position = vectorOfPosition + new Vector3(0f, 0f, _voxelSize) + offSet;
        sideVertexes[0].TextureCoordinate = uvPositionTop + new Vector2(0, uvScale.Y);
        //  sideVertexes[0].Normal = normal;

        sideVertexes[1].Position = vectorOfPosition + new Vector3(0f, _voxelSize, _voxelSize) + offSet;
        sideVertexes[1].TextureCoordinate = uvPositionTop;
        //sideVertexes[1].Normal = normal;

        sideVertexes[2].Position = vectorOfPosition + new Vector3(_voxelSize, _voxelSize, _voxelSize) + offSet;
        sideVertexes[2].TextureCoordinate = uvPositionTop + new Vector2(uvScale.X, 0);
        // sideVertexes[2].Normal = normal;

        sideVertexes[3].Position = vectorOfPosition + new Vector3(_voxelSize, 0f, _voxelSize) + offSet;
        sideVertexes[3].TextureCoordinate = uvPositionTop + uvScale;
        //  sideVertexes[3].Normal = normal;

        int[] indices = { 1, 2, 0, 0, 2, 3 };

        vertices.AddRange(sideVertexes);
        indexes.AddRange(indices.Select(b => b + startIndex));
    }

    private void AddBottomSide(List<VertexWithUv> vertices, List<int> indexes, int x, int y, int z,
        Vector3 offSet, Vector2 uvPositionTop, Vector2 uvScale)
    {
        //  var normal = Vector3.UnitY;
        var startIndex = vertices.Count;
        var vectorOfPosition = new Vector3((x * _voxelSize), (y * _voxelSize), (z * _voxelSize));
        var sideVertexes = new VertexWithUv[4];
        sideVertexes[0].Position = vectorOfPosition + new Vector3(0f, 0f, 0f) + offSet;
        sideVertexes[0].TextureCoordinate = uvPositionTop + new Vector2(0, uvScale.Y);
        //  sideVertexes[0].Normal = normal;

        sideVertexes[1].Position = vectorOfPosition + new Vector3(_voxelSize, 0f, 0f) + offSet;
        sideVertexes[1].TextureCoordinate = uvPositionTop + uvScale;
        //  sideVertexes[1].Normal = normal;

        sideVertexes[2].Position = vectorOfPosition + new Vector3(_voxelSize, 0f, _voxelSize) + offSet;
        sideVertexes[2].TextureCoordinate = uvPositionTop + new Vector2(uvScale.X, 0);
        //  sideVertexes[2].Normal = normal;

        sideVertexes[3].Position = vectorOfPosition + new Vector3(0f, 0f, _voxelSize) + offSet;
        sideVertexes[3].TextureCoordinate = uvPositionTop;
        // sideVertexes[3].Normal = normal;


        int[] indices = { 1, 2, 0, 0, 2, 3 };

        vertices.AddRange(sideVertexes);
        indexes.AddRange(indices.Select(b => b + startIndex).Reverse());
    }

    private void AddTopSide(List<VertexWithUv> vertices, List<int> indexes, int x, int y, int z,
        Vector3 offSet, Vector2 uvPositionTop, Vector2 uvScale)
    {
        // var normal = Vector3.UnitY;
        var startIndex = vertices.Count;
        var vectorOfPosition = new Vector3((x * _voxelSize), (y * _voxelSize), (z * _voxelSize));
        var sideVertexes = new VertexWithUv[4];
        sideVertexes[0].Position = vectorOfPosition + new Vector3(0f, _voxelSize, 0f) + offSet;
        sideVertexes[0].TextureCoordinate = uvPositionTop;
        //sideVertexes[0].Normal = normal;

        sideVertexes[1].Position = vectorOfPosition + new Vector3(_voxelSize, _voxelSize, 0f) + offSet;
        sideVertexes[1].TextureCoordinate = uvPositionTop + new Vector2(uvScale.X, 0);
        //  sideVertexes[1].Normal = normal;

        sideVertexes[2].Position = vectorOfPosition + new Vector3(_voxelSize, _voxelSize, _voxelSize) + offSet;
        sideVertexes[2].TextureCoordinate = uvPositionTop + uvScale;
        // sideVertexes[2].Normal = normal;

        sideVertexes[3].Position = vectorOfPosition + new Vector3(0f, _voxelSize, _voxelSize) + offSet;
        sideVertexes[3].TextureCoordinate = uvPositionTop + new Vector2(0, uvScale.Y);
        // sideVertexes[3].Normal = normal;


        int[] indices = { 1, 2, 0, 0, 2, 3 };

        vertices.AddRange(sideVertexes);
        indexes.AddRange(indices.Select(b => b + startIndex));
    }

    private void AddRightSide(List<VertexWithUv> vertices, List<int> indexes, int x, int y, int z,
        Vector3 offSet, Vector2 uvPositionTop, Vector2 uvScale)
    {
        //var normal = Vector3.UnitZ;
        var startIndex = vertices.Count;
        var vectorOfPosition = new Vector3((x * _voxelSize), (y * _voxelSize), (z * _voxelSize));
        var sideVertexes = new VertexWithUv[4];
        sideVertexes[0].Position = vectorOfPosition + new Vector3(_voxelSize, 0f, _voxelSize) + offSet;
        sideVertexes[0].TextureCoordinate = uvPositionTop + new Vector2(0, uvScale.Y);
        //sideVertexes[0].Normal = normal;

        sideVertexes[1].Position = vectorOfPosition + new Vector3(_voxelSize, _voxelSize, 0f) + offSet;
        sideVertexes[1].TextureCoordinate = uvPositionTop + new Vector2(uvScale.X, 0);
        // sideVertexes[1].Normal = normal;

        sideVertexes[2].Position = vectorOfPosition + new Vector3(_voxelSize, _voxelSize, _voxelSize) + offSet;
        sideVertexes[2].TextureCoordinate = uvPositionTop;
        //sideVertexes[2].Normal = normal;

        sideVertexes[3].Position = vectorOfPosition + new Vector3(_voxelSize, 0f, 0f) + offSet;
        sideVertexes[3].TextureCoordinate = uvPositionTop + uvScale;
        //sideVertexes[3].Normal = normal;

        int[] indices = { 0, 2, 1, 0, 1, 3 };

        vertices.AddRange(sideVertexes);
        indexes.AddRange(indices.Select(b => b + startIndex));
    }

    private void AddLeftSide(List<VertexWithUv> vertices, List<int> indexes, int x, int y, int z,
        Vector3 offSet, Vector2 uvPositionTop, Vector2 uvScale)
    {
        //var normal = Vector3.UnitZ;
        var startIndex = vertices.Count;
        var vectorOfPosition = new Vector3((x * _voxelSize), (y * _voxelSize), (z * _voxelSize));
        var sideVertexes = new VertexWithUv[4];
        sideVertexes[0].Position = vectorOfPosition + new Vector3(0f, 0f, _voxelSize) + offSet;
        sideVertexes[0].TextureCoordinate = uvPositionTop + uvScale;
        //  sideVertexes[0].Normal = normal;

        sideVertexes[1].Position = vectorOfPosition + new Vector3(0f, _voxelSize, 0f) + offSet;
        sideVertexes[1].TextureCoordinate = uvPositionTop;
        //sideVertexes[1].Normal = normal;

        sideVertexes[2].Position = vectorOfPosition + new Vector3(0f, _voxelSize, _voxelSize) + offSet;
        sideVertexes[2].TextureCoordinate = uvPositionTop + new Vector2(uvScale.X, 0);
        // sideVertexes[2].Normal = normal;

        sideVertexes[3].Position = vectorOfPosition + new Vector3(0f, 0f, 0f) + offSet;
        sideVertexes[3].TextureCoordinate = uvPositionTop + new Vector2(0, uvScale.Y);
        // sideVertexes[3].Normal = normal;

        int[] indices = { 0, 2, 1, 0, 1, 3 };

        vertices.AddRange(sideVertexes);
        indexes.AddRange(indices.Select(b => b + startIndex).Reverse());
    }

    private Mesh GenerateVisuals(List<VertexWithUv> vertices, List<int> indexes, Material material)
    {

        SurfaceTool surfaceTool = new SurfaceTool();
        surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        surfaceTool.SetMaterial(material);

        foreach (var index in indexes)
        {
            var vertex = vertices[index];
            surfaceTool.SetUV(vertex.TextureCoordinate);
            surfaceTool.AddVertex(vertex.Position);
        }

        surfaceTool.GenerateNormals();
        surfaceTool.Index();
        surfaceTool.GenerateTangents();

        var model = surfaceTool.Commit();
        surfaceTool.Dispose();
        return model;
    }

    public ChunkVisualsRequest EnequeVisualCreation(ChunkData chunkData, ChunkData[] neighbours)
    {
        var request = new ChunkVisualsRequest()
        {
            ChunkData = chunkData,
            Neighbours = neighbours
        };
        _calculationQueue.Enqueue(request);
        return request;
    }
}

public class ChunkVisualsRequest
{
    public ChunkData ChunkData { get; set; }
    public ChunkVisualData VisualsData { get; set; }
    public bool IsCalculated { get; set; } = false;
    public ChunkData[] Neighbours { get; set; }
    public Mesh Model { get; set; }
}

public struct ChunkVisualData
{
    public List<VertexWithUv> Vertexes { get; set; }
    public List<int> Indexes { get; set; }
}