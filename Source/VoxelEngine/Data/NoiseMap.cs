using System;
using System.Collections.Generic;
using Godot;

namespace TurtleGames.VoxelEngine;

public class NoiseMap 
{
    private readonly int _seed;
    private readonly float _scale;
    private readonly FastNoiseLite _noiseGenerator;

    public int Octaves { get; set; } = 4;
    public float Lacunarity { get; set; } = 0.5f;
    public float Persistance { get; set; } = 1.87f;

    public NoiseMap(int seed, float scale)
    {
        _seed = seed;
        _scale = scale;
        _noiseGenerator = new FastNoiseLite(_seed);
        _noiseGenerator.SetFrequency(_scale);
    }


    public float GetNoise(float xPosition, float yPosition)
    {
        float amplitude = 1;
        float frequency = 1;
        float noiseHeight = 0;
        float maxPossibleHeight = 0;
        Random random = new Random((int)xPosition + (int)yPosition + _seed);

        for (int i = 0; i < Octaves; i++)
        {
            maxPossibleHeight += amplitude;

            float sampleX = xPosition * frequency; //+ random.Next(-10000, 10000);
            float sampleY = yPosition * frequency;// + random.Next(-10000, 10000);
            noiseHeight += _noiseGenerator.GetNoise(sampleX, sampleY) * amplitude;

            amplitude *= Persistance;
            frequency *= Lacunarity;
            //float offSet
        }

        return MathUtils.Map(noiseHeight, maxPossibleHeight * -1, maxPossibleHeight, -1f, 1f);
    }
    /*float[,] noiseMap = new float[mapWidth,mapHeight];

        System.Random prng = new System.Random (seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++) {
            float offsetX = prng.Next (-100000, 100000) + offset.x;
            float offsetY = prng.Next (-100000, 100000) + offset.y;
            octaveOffsets [i] = new Vector2 (offsetX, offsetY);
        }

        if (scale <= 0) {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;


        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++) {
                    float sampleX = (x-halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y-halfHeight) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight) {
                    maxNoiseHeight = noiseHeight;
                } else if (noiseHeight < minNoiseHeight) {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap [x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                noiseMap [x, y] = Mathf.InverseLerp (minNoiseHeight, maxNoiseHeight, noiseMap [x, y]);
            }
        }

        return noiseMap;*/
}