namespace TurtleGames.VoxelEngine;

public class MathUtils
{
    public static float Map(float valueOfIn, float minIn, float maxIn, float minOut, float maxOut)
    {
        return (valueOfIn - minIn) * (maxOut - minOut) / (maxIn - minIn) + minOut;
    }
}