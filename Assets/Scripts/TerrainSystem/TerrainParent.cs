using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TerrainParent : MonoBehaviour
{
    public TerrainParameters parameters;
    public Dictionary<Vector2Int, Terrain> TerrainList = new Dictionary<Vector2Int, Terrain>();
    public static event Action TerrainTileCreated;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<Vector2Int> GetEmptyTiles()
    {
        List<Vector2Int> emptyTiles = new List<Vector2Int>();
        foreach (KeyValuePair<Vector2Int, Terrain> tile in TerrainList)
        {
            int xkey = tile.Key.x;
            int ykey = tile.Key.y;

            // check left edge
            if (!TerrainList.ContainsKey(new Vector2Int(xkey - 1, ykey)))
            {
                if (!emptyTiles.Contains(new Vector2Int(xkey - 1, ykey))) emptyTiles.Add(new Vector2Int(xkey - 1, ykey));
            }

            // check right edge
            if (!TerrainList.ContainsKey(new Vector2Int(xkey + 1, ykey)))
            {
                if (!emptyTiles.Contains(new Vector2Int(xkey + 1, ykey))) emptyTiles.Add(new Vector2Int(xkey + 1, ykey));
            }

            // check bottom edge
            if (!TerrainList.ContainsKey(new Vector2Int(xkey, ykey - 1)))
            {
                if (!emptyTiles.Contains(new Vector2Int(xkey, ykey - 1))) emptyTiles.Add(new Vector2Int(xkey, ykey - 1));
            }

            // check top edge
            if (!TerrainList.ContainsKey(new Vector2Int(xkey, ykey + 1)))
            {
                if (!emptyTiles.Contains(new Vector2Int(xkey, ykey + 1))) emptyTiles.Add(new Vector2Int(xkey, ykey + 1));
            }
        }

        return emptyTiles;
    }

    public void CreateTerrain(Vector2Int terrainPos, Vector3 meshPos)
    {
        GameObject EmptyObj = new GameObject("Terrain" + TerrainList.Count.ToString());
        EmptyObj.transform.parent = transform;
        EmptyObj.transform.position = meshPos;
        Terrain newTerrain = EmptyObj.AddComponent<Terrain>();
        newTerrain.Parameters = parameters;
        newTerrain.TerrainPos = terrainPos;
        newTerrain.GenerateMesh();
        TerrainList[terrainPos] = newTerrain;
        TerrainTileCreated?.Invoke();
    }

    public void CreateBaseTerrain()
    {
        GameObject EmptyObj = new GameObject("Terrain" + TerrainList.Count.ToString());
        EmptyObj.transform.parent = transform;
        Terrain newTerrain = EmptyObj.AddComponent<Terrain>();
        newTerrain.Parameters = parameters;
        newTerrain.TerrainPos = new Vector2Int(0, 0);
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
    bool addTerrainMode;
    private List<Vector2Int> emptyTiles;

    //bool keyPressed;
    private void OnEnable()
    {
        terrainParent = (TerrainParent)target;
        TerrainParent.TerrainTileCreated += RefreshEmptyTiles;
    }

    private void OnDisable()
    {
        TerrainParent.TerrainTileCreated -= RefreshEmptyTiles;
    }

    private void OnSceneGUI()
    {
        //if (Keyboard.current.nKey.isPressed && !keyPressed)
        //{
        //    keyPressed = true;
        //    delauney.ProceduralTriangulate();
        //}
        //else
        //{
        //    keyPressed = false;
        //}

        if (addTerrainMode)
        {
            Vector3 rootPos = terrainParent.TerrainList[new Vector2Int(0, 0)].transform.position;
            float rootSize = terrainParent.parameters.Width/2;
            // render empty edges
            foreach (Vector2Int tile in emptyTiles)
            {
                Vector3 offset = new Vector3(tile.x * terrainParent.parameters.Width, 0, tile.y * terrainParent.parameters.Length);
                //Handles.DrawWireCube(rootPos + offset, rootSize);
                if(Handles.Button(rootPos + offset, Quaternion.Euler(90,0,0), rootSize, rootSize, Handles.RectangleHandleCap))
                {
                    terrainParent.CreateTerrain(tile, rootPos + offset);
                }
            }

            // if mouse is clicked over a grid, create new terrain in spot
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Base Terrain"))
        {
            terrainParent.CreateBaseTerrain();
        }

        if (GUILayout.Button("Add Terrain Tile"))
        {
            addTerrainMode = !addTerrainMode;
            if (addTerrainMode) RefreshEmptyTiles();
        }
        GUILayout.EndHorizontal();
    }

    private void RefreshEmptyTiles()
    {
        emptyTiles = terrainParent.GetEmptyTiles();
    }
}
#endif
