using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HelperClasses
{
    public static class Noise
    {
        public static float[,] GenerateNoiseMap(float xOrg, float yOrg, int width, int height, float scale, int octaves, float persistance, float lacunarity)
        {
            float[,] noiseMap = new float[width, height];
            if (scale <= 0) scale = 0.0001f;

            float minVal = float.MaxValue;
            float maxVal = float.MinValue;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (xOrg + x) * frequency / scale;
                        float sampleY = (yOrg + y) * frequency / scale;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    minVal = (noiseHeight < minVal) ? noiseHeight : minVal;
                    maxVal = (noiseHeight > maxVal) ? noiseHeight : maxVal;
                    noiseMap[x, y] = noiseHeight;
                }
            }
            NormalizeMap(noiseMap, minVal, maxVal);
            return noiseMap;
        }

        public static void NormalizeMap(float[,] map, float floor, float ceiling)
        {
            for (int y = 0; y < map.GetLength(0); y++)
            {
                for (int x = 0; x < map.GetLength(1); x++)
                {
                    map[x, y] = Mathf.InverseLerp(floor, ceiling, map[x, y]);
                }
            }
        }

        public static float GetNoiseVal(float xOrg, float yOrg, int xpos, int ypos, float scale, int octaves, float persistance, float lacunarity)
        {
            //float[,] noiseMap = new float[width, height];
            if (scale <= 0) scale = 0.0001f;

            //float minVal = float.MaxValue;
            //float maxVal = float.MinValue;

            float amplitude = 1;
            float frequency = 1;
            float noiseHeight = 0;

            for (int i = 0; i < octaves; i++)
            {
                float sampleX = (xOrg + xpos) * frequency / scale;
                float sampleY = (yOrg + ypos) * frequency / scale;

                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                noiseHeight += perlinValue * amplitude;

                amplitude *= persistance;
                frequency *= lacunarity;
            }

            return noiseHeight;

            //minVal = (noiseHeight < minVal) ? noiseHeight : minVal;
            //maxVal = (noiseHeight > maxVal) ? noiseHeight : maxVal;
            //noiseMap[x, y] = noiseHeight;

            //for (int y = 0; y < height; y++)
            //{
            //    for (int x = 0; x < width; x++)
            //    {

            //    }
            //}
            //NormalizeMap(noiseMap, minVal, maxVal);
            //return noiseMap;
        }
    }
}
