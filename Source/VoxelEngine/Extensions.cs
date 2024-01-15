
using System.Linq;
using Godot;

namespace TurtleGames.VoxelEngine;

public static class Extensions
{
    public static bool EqualsWithMargin(this Vector2 vector, Vector2 other)
    {
        return (vector - other).Length() < 0.001f;
    }
    public static ChunkVector ToChunkVector(this Vector2 vector)
    {
        return new ChunkVector((int)vector.X, (int)vector.Y);
    }

    public static T GetOrCreate<T>(this Node node) where T : Node, new()
    {
        var nodesOfType = node.GetChildren().OfType<T>();
        T nodeOfType = nodesOfType.SingleOrDefault();
        if (nodeOfType == null)
        {
            nodeOfType = new T();
            node.AddChild(nodeOfType);
        }
        return nodeOfType;
    }
}