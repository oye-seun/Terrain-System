using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TerrainParent : MonoBehaviour
{
    [SerializeField] private TerrainParameters parameters;
    public Dictionary<Vector2Int, Terrain> TerrainList = new Dictionary<Vector2Int, Terrain>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateTerrain()
    {
        // loop through available terrain and plot glid

        // if mouse is clicked oevr a grid, create new terrain in spot
    }

    public void CreateBaseTerrain()
    {
        GameObject EmptyObj = new GameObject("Terrain" + TerrainList.Count.ToString());
        EmptyObj.transform.parent = transform;
        Terrain newTerrain = EmptyObj.AddComponent<Terrain>();
        newTerrain.Parameters = parameters;
        newTerrain.GenerateMesh();
        TerrainList[new Vector2Int(0,0)] = newTerrain;
    }
}


[System.Serializable]
public struct TerrainParameters
{
    [Header("Vert Resolution")]
    public int VertsWidth;
    public int VertsLength;

    [Header("Map Dimensions")]
    public float Width;
    public float Length;
    public float MinHeight;
    public float MaxHeight;

    [Header("Noise Parameters")]
    public float NoiseXOrigin;
    public float NoiseYOrigin;
    public float NoiseScale;
    public int NoiseOctaves;
    public float NoiseLacunarity;
    [Range(0f, 1f)]
    public float NoisePersistance;
}


#if (UNITY_EDITOR)
[CustomEditor(typeof(TerrainParent))]
public class TerrainParentEditor : UnityEditor.Editor
{
    TerrainParent terrainParent;
    //bool keyPressed;
    private void OnEnable()
    {
        terrainParent = (TerrainParent)target;
    }

    //private void OnSceneGUI()
    //{
    //    //if (Keyboard.current.nKey.isPressed && !keyPressed)
    //    //{
    //    //    keyPressed = true;
    //    //    delauney.ProceduralTriangulate();
    //    //}
    //    //else
    //    //{
    //    //    keyPressed = false;
    //    //}
    //}

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        if (GUILayout.Button("Add Base Terrain"))
        {
            terrainParent.CreateBaseTerrain();
        }
    }
}
#endif
