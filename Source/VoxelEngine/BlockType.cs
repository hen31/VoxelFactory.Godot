using System.Collections.Generic;

namespace TurtleGames.VoxelEngine;

public class BlockType
{
    public ushort BlockId { get; set; }
    public string Name { get; set; }
    public BlockTextureInfo BlockTexture { get; set; } = new BlockTextureInfo();
}

public class BlockTextureInfo
{
    public string DefaultTexture { get; set; }
    public string TopTexture { get; set; }

    public int AmountOfTextures
    {
        get
        {
            int amount = 0;
            AddOneIfNotEmpty(DefaultTexture, ref amount);
            AddOneIfNotEmpty(TopTexture, ref amount);
            return amount;
        }
    }

    private void AddOneIfNotEmpty(string texture, ref int amount)
    {
        if (string.IsNullOrWhiteSpace(texture))
        {
            amount++;
        }
    }

    public IEnumerable<(BlockSide BlockSide, string TextureFile)> GetTextures()
    {
        if (!string.IsNullOrWhiteSpace(DefaultTexture))
        {
            yield return (BlockSide.Default, DefaultTexture);
        }

        if (!string.IsNullOrWhiteSpace(TopTexture))
        {
            yield return (BlockSide.Top,TopTexture);
        }
    }
}

public enum BlockSide
{
    Default,
    Front,
    Back,
    Top,
    Bottom,
    Left,
    Right
}