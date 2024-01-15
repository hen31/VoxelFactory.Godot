using Godot;

namespace TurtleGames.VoxelEngine;

[GlobalClass]
public partial class NoiseWithSpline : Resource
{
    [Export] public NoiseMapSettings NoiseMapSettings { get; set; }
    [Export] public NoiseSpline NoiseSpline { get; set; }

    public NoiseWithSpline()
    {
    }

    private NoiseMap _noiseMap;

    public void Initialize()
    {
        _noiseMap = new NoiseMap(NoiseMapSettings.Seed, NoiseMapSettings.Scale)
        {
            Octaves = NoiseMapSettings.Octaves,
            Lacunarity = NoiseMapSettings.Lacunarity,
            Persistance = NoiseMapSettings.Persistance
        };
    }

    public int GetValue(float xPosition, float yPosition)
    {
        return NoiseSpline.GetValue(_noiseMap.GetNoise(xPosition, yPosition));
    }
}