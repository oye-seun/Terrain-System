using UnityEngine;
using HelperClasses;

public class IslandMapGen : MonoBehaviour
{
    [Header("Top Settings")]
    [SerializeField] private int _topMapWidth;
    [SerializeField] private int _topMapHeight;
    [SerializeField] private float _topMapXOrigin;
    [SerializeField] private float _topMapYOrigin;
    [SerializeField] private float _topMapNoiseScale;
    [SerializeField] private int _topMapOctaves;
    [Range(0f, 1f)]
    [SerializeField] private float _topMapPersistance;
    [SerializeField] private float _topMapLacunarity;
    public int TopMapWidth => _topMapWidth;
    public int TopMapHeight => _topMapHeight;


    [Header("Bottom Settings")]
    [SerializeField] private int _downMapWidth;
    [SerializeField] private int _downMapHeight;
    [SerializeField] private float _downMapXOrigin;
    [SerializeField] private float _downMapYOrigin;
    [SerializeField] private float _downMapNoiseScale;
    [SerializeField] private int _downMapOctaves;
    [Range(0f, 1f)]
    [SerializeField] private float _downMapPersistance;
    [SerializeField] private float _downMapLacunarity;
    public int DownMapWidth => _downMapWidth;
    public int DownMapHeight => _downMapHeight;


//#if UNITY_EDITOR
//    public bool AutoUpdate;
//#endif

    public float[,] GenerateTopMap()
    {
        return Noise.GenerateNoiseMap(_topMapXOrigin, _topMapYOrigin, _topMapWidth, _topMapHeight, _topMapNoiseScale, _topMapOctaves, _topMapPersistance, _topMapLacunarity);
    }

    public float[,] GenerateDownMap()
    {
        return Noise.GenerateNoiseMap(_downMapXOrigin, _downMapYOrigin, _downMapWidth, _downMapHeight, _downMapNoiseScale, _downMapOctaves, _downMapPersistance, _downMapLacunarity);
    }

    private void OnValidate()
    {
        _topMapWidth = (_topMapWidth < 1) ? 1 : _topMapWidth;
        _topMapHeight = (_topMapHeight < 1) ? 1 : _topMapHeight;
        _topMapOctaves = (_topMapOctaves < 1) ? 1 : _topMapOctaves;

        _downMapWidth = (_downMapWidth < 1) ? 1 : _downMapWidth;
        _downMapHeight = (_downMapHeight < 1) ? 1 : _downMapHeight;
        _downMapOctaves = (_downMapOctaves < 1) ? 1 : _downMapOctaves;
    }
}

