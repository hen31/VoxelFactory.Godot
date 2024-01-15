using System.Collections.Generic;
using Godot;
using Vector2 = Godot.Vector2;

namespace TurtleGames.VoxelEngine;

public class BlockTextureCreator
{
    private int _textureSize = 256;
    private readonly List<BlockType> _blockTypes;

    private string _textureFolder = @"C:\Users\hendr\Documents\my games\VoxelFactory\textures\";

    public BlockTextureCreator(List<BlockType> blockTypes)
    {
        _blockTypes = blockTypes;
    }

    public BaseMaterial3D CreateBlockTextureAtlas(out Dictionary<ushort, BlockTextureUvMapping> blockMapping)
    {
        blockMapping = new();
        return GD.Load<BaseMaterial3D>("res://Assets/testmaterial.material");
        /*  var blockTextures = _blockTypes.ToList();
          var amountOfImages = blockTextures.Sum(b => b.BlockTexture.AmountOfTextures);
          int rowSize = 16;
          var numberOfRows = (int)MathF.Ceiling(amountOfImages / (float)rowSize);
          var firstImage = Image.Load(File.OpenRead(_textureFolder + blockTextures.First().BlockTexture.DefaultTexture));

          var imageSizeWidth = (int)(rowSize * _textureSize);
          var imageSizeHeight = (int)(numberOfRows * _textureSize);
          Image fullTextureImage = Image.New2D(imageSizeWidth, imageSizeHeight, 1, firstImage.PixelBuffer[0].Format);

          int imageY = 0;
          int imageX = 0;
          foreach (var blockTypeToHandle in blockTextures)
          {
              Dictionary<BlockSide, Vector2> texturesForBlock = new Dictionary<BlockSide, Vector2>();
              foreach (var texture in blockTypeToHandle.BlockTexture.GetTextures())
              {
                  var textureForBlock =
                      Image.Load(File.OpenRead(_textureFolder + texture.TextureFile));

                  for (int x = 0; x < _textureSize; x++)
                  {
                      for (int y = 0; y < _textureSize; y++)
                      {
                          var textureX = (imageX * _textureSize) + x;
                          var textureY = (imageY * _textureSize) + y;
                          fullTextureImage.PixelBuffer[0]
                              .SetPixel(textureX, textureY,
                                  textureForBlock.PixelBuffer[0].GetPixel<int>(x, y));
                      }
                  }

                  texturesForBlock.Add(texture.BlockSide,
                      new Vector2(imageX / (float)rowSize, imageY / (float)numberOfRows));
                  imageX++;
                  if (imageX % rowSize == 0)
                  {
                      imageY++;
                  }
              }

              blockMapping.Add(blockTypeToHandle.BlockId, new BlockTextureUvMapping()
              {
                  UvScale = new Vector2(1 / (float)rowSize, 1 / (float)numberOfRows),
                  TopSide = texturesForBlock.TryGetValue(BlockSide.Top, out var topVector)
                      ? topVector
                      : texturesForBlock[BlockSide.Default],
                  BottomSide = texturesForBlock.TryGetValue(BlockSide.Bottom, out var bottomVector)
                      ? bottomVector
                      : texturesForBlock[BlockSide.Default],
                  RightSide = texturesForBlock.TryGetValue(BlockSide.Right, out var rightVector)
                      ? rightVector
                      : texturesForBlock[BlockSide.Default],
                  LeftSide = texturesForBlock.TryGetValue(BlockSide.Left, out var leftVector)
                      ? leftVector
                      : texturesForBlock[BlockSide.Default],
                  FrontSide = texturesForBlock.TryGetValue(BlockSide.Front, out var frontVector)
                      ? frontVector
                      : texturesForBlock[BlockSide.Default],
                  BackSide = texturesForBlock.TryGetValue(BlockSide.Back, out var backVector)
                      ? backVector
                      : texturesForBlock[BlockSide.Default],
              });
          }




          fullTextureImage.Save(File.OpenWrite("C:\\Development\\Noises\\atlas.png"), ImageFileType.Png);*/
        return null; // fullTextureImage;
    }
}

public struct BlockTextureUvMapping
{
    public BlockTextureUvMapping()
    {
        TopSide = default;
        BottomSide = default;
        RightSide = default;
        LeftSide = default;
        FrontSide = default;
        BackSide = default;
        UvScale = Vector2.One;
    }

    public Vector2 TopSide { get; set; }
    public Vector2 BottomSide { get; set; }
    public Vector2 RightSide { get; set; }
    public Vector2 LeftSide { get; set; }
    public Vector2 FrontSide { get; set; }
    public Vector2 BackSide { get; set; }
    public Vector2 UvScale { get; set; }
}