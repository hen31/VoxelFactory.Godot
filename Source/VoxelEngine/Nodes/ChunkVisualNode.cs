﻿using System;
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
        public uint CollisionLayer { get; set; }

        public void Start()
        {
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
                _request = VisualGeneratorNode.EnequeVisualCreation(ChunkData, Neighbours.ToArray());
            }
            else
            {
                _hasModel = true;
                var meshNode = this.GetOrCreate<MeshInstance3D>();
                meshNode.Mesh = _request.Model;
                var staticBody = this.GetOrCreate<StaticBody3D>();
                staticBody.CollisionLayer = CollisionLayer;
                var collisionShape = staticBody.GetOrCreate<CollisionShape3D>();
                var shape = new ConcavePolygonShape3D();
                shape.Data = _request.VisualsData.Indexes.Select(b => _request.VisualsData.Vertexes[b].Position)
                    .ToArray();
                collisionShape.Shape = shape;
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