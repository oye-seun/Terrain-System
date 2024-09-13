using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HelperClasses;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private int _mapWidth;
    [SerializeField] private int _mapHeight;
    [SerializeField] private float _mapXOrigin;
    [SerializeField] private float _mapYOrigin;
    [SerializeField] private float _noiseScale;
    [SerializeField] private int _octaves;
    [Range(0f, 1f)]
    [SerializeField] private float _persistance;
    [SerializeField] private float _lacunarity;
    [SerializeField] private MeshGenerator _meshGenerator;

#if UNITY_EDITOR
    public bool AutoUpdate;
#endif

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(_mapXOrigin, _mapYOrigin,_mapWidth, _mapHeight, _noiseScale, _octaves, _persistance, _lacunarity);

        //int width = noiseMap.GetLength(0);
        //int height = noiseMap.GetLength(1);
        //for (int y = 0; y < height; y++)
        //{
        //    for (int x = 0; x < width; x++)
        //    {
        //        Debug.Log(noiseMap[x,y]);
        //    }
        //}
        
        MapDisplay display = FindObjectOfType<MapDisplay>();
        display.DrawNoiseMap(noiseMap);
        _meshGenerator.GenerateMesh(noiseMap);
    }

    private void OnValidate()
    {
        _mapWidth = (_mapWidth < 1)? 1 : _mapWidth;
        _mapHeight = (_mapHeight < 1)? 1 : _mapHeight;
        _octaves = (_octaves < 1)? 1 : _octaves;
    }
}
