using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace TurtleGames.VoxelEngine
{
    [GlobalClass]
    public partial class ChunkVisualNode : Node3D
    {
        private ChunkVisualsRequest _request;

        // Declared public member fields and properties will show in the game studio
        private bool _hasModel { get; set; }
        public float VoxelSize { get; set; } = 1f;
        public ChunkData ChunkData { get; set; }
        public Material Material { get; set; }
        public ChunkData[] Neighbours { get; set; }
        public ChunkVisualsGeneratorNode VisualGeneratorNode { get; set; }

        public void Start()
        {
        }

        private void GenerateVisuals(List<VertexWithUv> vertices, List<int> indexes)
        {
            var meshNode = this.GetOrCreate<MeshInstance3D>();

            if (vertices.Count == 0)
            {
                return;
            }

            SurfaceTool surfaceTool = new SurfaceTool();
            surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
            surfaceTool.SetMaterial(Material);

            foreach (var index in indexes)
            {
                var vertex = vertices[index];
                surfaceTool.SetUV(vertex.TextureCoordinate);
                surfaceTool.AddVertex(vertex.Position);
            }

            surfaceTool.GenerateNormals();
            surfaceTool.Index();
            surfaceTool.GenerateTangents();
            
            meshNode.Mesh = surfaceTool.Commit();
            surfaceTool.Dispose();
            /* var indices = indexes.ToArray();
           var vertexBuffer = Stride.Graphics.Buffer.Vertex.New(GraphicsDevice, vertices.ToArray(),
                 GraphicsResourceUsage.Default);

             var indexBuffer = Stride.Graphics.Buffer.Index.New(GraphicsDevice, indexes.ToArray());

             var vector3OfSize = new Vector3(ChunkData.Size.X, ChunkData.Height, ChunkData.Size.Y);
             var points = new[]
             {
                 -vector3OfSize,
                 vector3OfSize,
             };
             var boundingBox = BoundingBox.FromPoints(points);

             var customMesh = new Mesh
             {
                 Draw = new MeshDraw
                 {

                     PrimitiveType = PrimitiveType.TriangleList,
                     DrawCount = indices.Length,
                     IndexBuffer = new IndexBufferBinding(indexBuffer, true, indices.Length),
                     VertexBuffers = new[]
                     {
                         new VertexBufferBinding(vertexBuffer,
                             VertexPositionNormalTexture.Layout, vertexBuffer.ElementCount)
                     },
                 },
                 BoundingBox = boundingBox

             };

             model = new Model();
             // add the mesh to the model
             model.Meshes.Add(customMesh);
             model.BoundingBox = boundingBox;
             meshNode.Model = model;

             model.Materials.Add(Material);

             meshNode.IsShadowCaster = true;*/
        }


        public override void _PhysicsProcess(double delta)
        {
            if (_hasModel
                || !ChunkData.Calculated
                || Neighbours.Any(b => !b.Calculated)
                || _request is { IsCalculated: false })
            {
                return;
            }

            // Do stuff every new frame
            if (_request == null)
            {
                _request = VisualGeneratorNode.EnequeVisualCreation(ChunkData, Neighbours);
            }
            else
            {
                _hasModel = true;
                var meshNode = this.GetOrCreate<MeshInstance3D>();
                meshNode.Mesh = _request.Model;
               // GenerateVisuals(_request.VisualsData.Vertexes, _request.VisualsData.Indexes);

                /*   var staticColliderComponent = Entity.Get<StaticColliderComponent>();
                   var colliderShape = new StaticMeshColliderShape(
                       _request.VisualsData.Vertexes.Select(b => b.Position).ToList(),
                       _request.VisualsData.Indexes);
                   if (staticColliderComponent == null)
                   {
                       var staticCollider = new StaticColliderComponent();
                       staticCollider.ColliderShape = colliderShape;
                       Entity.Add(staticCollider);
                   }
                   else
                   {
                       staticColliderComponent.ColliderShape = colliderShape;
                   }*/

                _request = null;
            }
        }

        public void Remesh()
        {
            _hasModel = false;
            _request = null;
        }
    }
}