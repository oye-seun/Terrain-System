using System;
using System.Collections.Generic;
//using System.Drawing;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using HelperClasses;

public class TerrainParent : MonoBehaviour
{
    public TerrainParameters parameters;
    public Dictionary<Vector2Int, Terrain> TerrainList = new Dictionary<Vector2Int, Terrain>();
    public static event Action TerrainTileCreated;
    public Shader shader;
    public Material material;
    public BrushSettings brushSettings;
    public Texture2D brush;
    public Texture2D mask;
    public String maskPath;
    private Vector2 tilingMultiplier = Vector2.one;
    public float brushSize;
    [Range(0, 1)] public float opacity;
    public Vector2Int maskDimensions;
    public List<TerrainTextureLayer> textureLayers;
    public int layerColorIndex = 0;
    private Color[] layerColors;

    private List<int> freeIDs = new List<int>();

    private Vector2Int minTile;
    private Vector2Int maxTile;
    public int TerrainLayer { get; private set; }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetupTerrainParent()
    {
        TerrainLayer = LayerAdd.AddLayerAt(9, "Terrain", false, true);
        if (TerrainLayer == -1) TerrainLayer = 0;

        layerColors = new Color[] { new Color(0, 0, 0, 0), new Color(1, 0, 0, 0), new Color(1, 1, 0, 0), new Color(1, 1, 1, 0), new Color(1, 1, 1, 1) };
    }

    private void CreateMaterial()
    {
        material = new Material(shader);
        ApplyMaterialParams();
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
        IncreaseMask(terrainPos);
        // loop through to find free name
        string objectName = "";
        if (freeIDs.Count > 0) 
        {
            objectName = "Terrain" + freeIDs[0];
            freeIDs.RemoveAt(0);
        }
        else objectName = "Terrain" + TerrainList.Count.ToString();

        GameObject EmptyObj = new GameObject(objectName);
        EmptyObj.transform.parent = transform;
        EmptyObj.layer = TerrainLayer;
        EmptyObj.transform.position = meshPos;
        Terrain newTerrain = EmptyObj.AddComponent<Terrain>();
        newTerrain.Parameters = parameters;
        //newTerrain.Parameters.NoiseXOrigin += terrainPos.x * parameters.VertsWidth;
        //newTerrain.Parameters.NoiseYOrigin += terrainPos.y * parameters.VertsLength;
        newTerrain.TerrainPos = terrainPos;
        newTerrain.GenerateMesh();
        TerrainList[terrainPos] = newTerrain;
        FindTerrainExtremesAndGenerateUVs(terrainPos, true);
        TerrainTileCreated?.Invoke();

        EmptyObj.GetComponent<Renderer>().material = material;
        ApplyMaterialParams();
    }

    public void CreateBaseTerrain()
    {
        if (TerrainList.ContainsKey(Vector2Int.zero)) return;
        GameObject EmptyObj = new GameObject("Terrain" + TerrainList.Count.ToString());
        EmptyObj.transform.parent = transform;
        EmptyObj.layer = TerrainLayer;
        Terrain newTerrain = EmptyObj.AddComponent<Terrain>();
        newTerrain.Parameters = parameters;
        newTerrain.TerrainPos = new Vector2Int(0, 0);
        newTerrain.GenerateMesh();
        TerrainList[new Vector2Int(0,0)] = newTerrain;
        FindTerrainExtremesAndGenerateUVs(newTerrain.TerrainPos, true);
        freeIDs.Clear();

        mask = new Texture2D(maskDimensions.x, maskDimensions.y, TextureFormat.RGBA32, true);
        SaveNReloadMaskAsset(mask);

        CreateMaterial();
        EmptyObj.GetComponent<Renderer>().material = material;
    }

    public void DeleteTerrain(Vector2Int pos)
    {
        if (pos == Vector2Int.zero) return;

        Terrain terrain = TerrainList[pos];
        string objectName  = terrain.gameObject.name.Remove(0, 7);
        //Debug.Log("object name: " + objectName);
        freeIDs.Add(int.Parse(objectName));
        TerrainList.Remove(pos);
        DestroyImmediate(terrain.gameObject);

        FindTerrainExtremesAndGenerateUVs(Vector2Int.zero, false);
        DecreaseMask(pos);
        ApplyMaterialParams();
    }

    public void RerenderAll()
    {
        foreach(KeyValuePair<Vector2Int,Terrain> tile in TerrainList)
        {
            tile.Value.Parameters = parameters;
            tile.Value.GenerateMesh();
        }

        foreach (KeyValuePair<Vector2Int, Terrain> tile in TerrainList)
        {
            tile.Value.GenerateUVs(minTile, maxTile);
        }
    }

    private void FindTerrainExtremesAndGenerateUVs(Vector2Int pos, bool create)
    {
        Vector2Int min = Vector2Int.zero;
        Vector2Int max = Vector2Int.zero;

        foreach (KeyValuePair<Vector2Int,Terrain> tile in TerrainList)
        {
            Vector2Int terrainPos = tile.Key;
            if (terrainPos.x < min.x) min.x = terrainPos.x;
            if (terrainPos.x > max.x) max.x = terrainPos.x;
            if (terrainPos.y < min.y) min.y = terrainPos.y;
            if (terrainPos.y > max.y) max.y = terrainPos.y;
        }

        if(minTile == min && maxTile == max) //same extremes
        {
            if (create) TerrainList[pos].GenerateUVs(min, max);  //adding new tile, with same extremes
            else return; //deleting tile
        }
        else //extremes have changed, re generate all uvs
        {
            minTile = min;
            maxTile = max;
            foreach (KeyValuePair<Vector2Int, Terrain> tile in TerrainList)
            {
                tile.Value.GenerateUVs(minTile, maxTile);
            }
            Vector2Int newDimension = (maxTile - minTile + Vector2Int.one);
            if (newDimension.x > newDimension.y) tilingMultiplier = new Vector2(newDimension.x / newDimension.y, 1);
            else if(newDimension.x < newDimension.y) tilingMultiplier = new Vector2(1, newDimension.y / newDimension.x);
        }
    }

    public void Paint(RaycastHit hit)
    {
        Vector2Int pixelUV = new Vector2Int((int)(hit.textureCoord.x * mask.width), (int)(hit.textureCoord.y * mask.height));
        //Debug.Log("calculated texture coord: " + pixelUV);

        int minx = (int)(pixelUV.x - brushSize);
        int miny = (int)(pixelUV.y - brushSize);
        int diff = pixelUV.x - minx;
        diff = (diff * 2) + 1;


        Vector2Int[] affPixels = new Vector2Int[diff * diff];
        for (int j = 0; j < diff; j++)
        {
            for (int i = 0; i < diff; i++)
            {
                affPixels[(j * diff) + i] = new Vector2Int(minx + i, miny + j);
                //Debug.Log("uv: " + affPixels[(j * diff) + i]);
            }
        }

        //Color col = new Color(Red / 255f, Green / 255f, Blue / 255f);
        foreach (Vector2Int uv in affPixels)
        {
            float dist = Vector2Int.Distance(uv, pixelUV);
            //float pow = Mathf.InverseLerp(Size, 0, dist);
            float pow = 1 - Mathf.InverseLerp(0, brushSize, dist);
            Color newcol = Color.Lerp(mask.GetPixel(uv.x, uv.y), layerColors[layerColorIndex], pow * opacity);
            //Color newcol = mask.GetPixel(uv.x, uv.y) + (layerColors[layerColorIndex] * pow * opacity);
            mask.SetPixel(uv.x, uv.y, newcol);
        }


        //Tex.SetPixel(pixelUV.x, pixelUV.y, new Color((float)Red/255, (float)Green/255, (float)Blue/255));
        mask.Apply();
    }

    public void ApplyMaterialParams()
    {
        for (int i = 0; i < textureLayers.Count; i++)
        {
            InjectShaderParams(i, textureLayers[i]);
        }

        TerrainTextureLayer ttl = new TerrainTextureLayer();
        for (int i = textureLayers.Count; i < 3; i++)
        {
            InjectShaderParams(i, ttl);
        }

        material.SetTexture("_Mask", mask);
    }

    public void InjectShaderParams(int i, TerrainTextureLayer ttl)
    {
        material.SetVector("_Tiling" + (i + 1).ToString(), ttl.Tiling * tilingMultiplier);
        material.SetColor("_Color" + (i + 1).ToString(), ttl.Color);
        material.SetTexture("_MainText" + (i + 1).ToString(), ttl.MainTexture);
        material.SetTexture("_NormalText" + (i + 1).ToString(), ttl.NormalTexture);
        material.SetFloat("_NormalStrength" + (i + 1).ToString(), ttl.NormalStrength);
        material.SetFloat("_Metallic" + (i + 1).ToString(), ttl.Metallic);
        material.SetFloat("_Smoothness" + (i + 1).ToString(), ttl.Smoothness);
    }

    #if UNITY_EDITOR
    public void CreateOrResizeMask()
    {
        if(mask == null)
        {
            mask = new Texture2D(maskDimensions.x, maskDimensions.y, TextureFormat.RGBA32, true);
            SaveNReloadMaskAsset(mask);
        }
    }

    //private void SaveNReloadMask(Texture2D mask)
    //{
    //    byte[] bytes = mask.EncodeToPNG();
    //    if (maskPath == "") maskPath = "mask.png";
    //    if (!maskPath.EndsWith(".png")) maskPath += ".png";

    //    string fullpath = Application.dataPath + "/" + maskPath;
    //    System.IO.File.WriteAllBytes(fullpath, bytes);
    //    //AssetDatabase.CreateAsset(mask, "Assets/" + maskPath);

    //    string assetpath = "Assets/" + maskPath;
    //    AssetDatabase.Refresh();

    //    TextureImporter tImporter = AssetImporter.GetAtPath(assetpath) as TextureImporter;
    //    if (tImporter != null)
    //    {
    //        tImporter.textureType = TextureImporterType.Default;
    //        //tImporter.textureFormat = TextureImporterFormat.RGBA32;
    //        tImporter.textureCompression = TextureImporterCompression.Uncompressed;
    //        tImporter.isReadable = true;

    //        AssetDatabase.ImportAsset(assetpath);
    //        this.mask = AssetDatabase.LoadAssetAtPath<Texture2D>(assetpath);
    //    }
    //}

    private void SaveNReloadMaskAsset(Texture2D mask)
    {
        byte[] bytes = mask.EncodeToPNG();
        if (maskPath == "") maskPath = "mask.asset";
        if (!maskPath.EndsWith(".asset")) maskPath += ".asset";

        string assetpath = "Assets/" + maskPath;
        AssetDatabase.CreateAsset(mask, assetpath);
        this.mask = AssetDatabase.LoadAssetAtPath<Texture2D>(assetpath);
    }

    /// <summary>
    /// called before findExtremeUVs
    /// </summary>
    /// <param name="pos"></param>
    public void IncreaseMask(Vector2Int pos)
    {
        // get the new min position
        Vector2Int newMin = new Vector2Int((pos.x < minTile.x)? pos.x: minTile.x, (pos.y < minTile.y)? pos.y: minTile.y);
        Vector2Int newMax = new Vector2Int((pos.x > maxTile.x)? pos.x: maxTile.x, (pos.y > maxTile.y)? pos.y: maxTile.y);
        if(newMin == minTile && newMax == maxTile) return;  // no change in mask size

        Vector2Int newDimension = (newMax - newMin + Vector2Int.one) * maskDimensions;
        Texture2D newMask = new Texture2D(newDimension.x, newDimension.y, TextureFormat.RGBA32, true);
        Debug.Log("new Dimension: " + newDimension + "  newMask.width: " + newMask.width);
        int dstX = (minTile.x + Mathf.Abs(newMin.x)) * maskDimensions.x;
        int dstY = (minTile.y + Mathf.Abs(newMin.y)) * maskDimensions.y;

        Graphics.CopyTexture(mask, 0, 0, 0, 0, mask.width, mask.height, newMask, 0, 0, dstX, dstY);
        //TextureHelper.CopyTexture(mask, 0, 0, mask.width, mask.height, newMask, dstX, dstY);
        SaveNReloadMaskAsset(newMask); //
    }

    /// <summary>
    /// called after findExtremeUVs
    /// </summary>
    /// <param name="pos"></param>
    public void DecreaseMask(Vector2Int pos)
    {
        // get the old min position
        Vector2Int oldMin = new Vector2Int((pos.x < minTile.x) ? pos.x : minTile.x, (pos.y < minTile.y) ? pos.y : minTile.y);
        Vector2Int oldMax = new Vector2Int((pos.x > maxTile.x) ? pos.x : maxTile.x, (pos.y > maxTile.y) ? pos.y : maxTile.y);
        if (oldMin == minTile && oldMax == maxTile) return;  // no change in mask size

        //Vector2Int newDimension = (oldMax - oldMin + Vector2Int.one) * maskDimensions;
        Vector2Int newDimension = (maxTile - minTile + Vector2Int.one) * maskDimensions;
        Texture2D newMask = new Texture2D(newDimension.x, newDimension.y, TextureFormat.RGBA32, true);
        Debug.Log("new Dimension: " + newDimension + "  newMask.width: " + newMask.width);
        int dstX = (minTile.x + Mathf.Abs(oldMin.x)) * maskDimensions.x;
        int dstY = (minTile.y + Mathf.Abs(oldMin.y)) * maskDimensions.y;

        Graphics.CopyTexture(mask, 0, 0, dstX, dstY, newMask.width, newMask.height, newMask, 0, 0, 0, 0);
        //TextureHelper.CopyTexture(mask, 0, 0, mask.width, mask.height, newMask, dstX, dstY);
        SaveNReloadMaskAsset(newMask); 
    }
    #endif
}

[System.Serializable]
public class BrushSettings
{
    public List<Texture2D> Brushes;
    public int SelectedBrush;
    public float Size;
    public float Opacity;

    public BrushSettings()
    {
        Brushes = new List<Texture2D>();
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
    public float Height;

    [Header("Noise Parameters")]
    public float NoiseXOrigin;
    public float NoiseYOrigin;
    public float NoiseScale;
    public int NoiseOctaves;
    public float NoiseLacunarity;
    [Range(0f, 1f)]
    public float NoisePersistance;
}

[System.Serializable]
public class TerrainTextureLayer
{
    public Vector2 Tiling;
    public Color Color;
    public Texture2D MainTexture;
    public Texture2D NormalTexture;
    [Range(0f, 2f)] public float NormalStrength;
    [Range(0f, 1f)] public float Metallic;
    [Range(0f, 1f)] public float Smoothness;

    public TerrainTextureLayer()
    {
        Tiling = Vector2.one;
        Color = Color.white;
        NormalStrength = 1;
        Metallic = 0.3f;
        Smoothness = 0.2f;
    }
}


#if (UNITY_EDITOR)
[CustomEditor(typeof(TerrainParent))]
public class TerrainParentEditor : UnityEditor.Editor
{
    // General tool state
    enum state { Tiles, Brush, Curve, Spawn, max}

    TerrainParent terrainParent;

    // Tile menu params
    enum TileState { add, remove, none}
    private TileState tileState = TileState.none;
    private SerializedProperty TerrainParameters;

    // Brush menu params
    private SerializedProperty Shader;
    private SerializedProperty Material;
    private SerializedProperty Brush;
    private SerializedProperty BrushSize;
    private SerializedProperty Opacity;
    private SerializedProperty Mask;
    private SerializedProperty TextureLayers;
    private SerializedProperty MaskDimensions;
    private ReorderableList _reorderableList;
    private LayerMask terrainMask;
    private List<bool> _selected;
    private const float _fieldSpacing = 20;


    private List<Vector2Int> emptyTiles;
    private state editorState = state.Tiles;

    //bool keyPressed;
    private void OnEnable()
    {
        terrainParent = (TerrainParent)target;
        TerrainParent.TerrainTileCreated += RefreshEmptyTiles;

        terrainParent.SetupTerrainParent();
        terrainMask = LayerMask.GetMask("Terrain");

        // Tile Menu Params initilization
        TerrainParameters = serializedObject.FindProperty("parameters");

        // Brush Menu Params initilization
        Shader = serializedObject.FindProperty("shader");
        Material = serializedObject.FindProperty("material");
        Brush = serializedObject.FindProperty("brush");
        BrushSize = serializedObject.FindProperty("brushSize");
        Opacity = serializedObject.FindProperty("opacity");
        Mask = serializedObject.FindProperty("mask");
        MaskDimensions = serializedObject.FindProperty("maskDimensions");
        TextureLayers = serializedObject.FindProperty("textureLayers");
        _reorderableList = new ReorderableList(serializedObject, TextureLayers, true, true, true, true);
        _reorderableList.drawElementCallback = DrawListItems;
        _reorderableList.elementHeightCallback = HeightCallBack;
        _reorderableList.drawHeaderCallback = DrawHeader;
        _reorderableList.onAddCallback = AddCallBack;
        _reorderableList.onRemoveCallback = RemoveCallBack;
        _reorderableList.onChangedCallback = ListChanged;
        _reorderableList.onSelectCallback = SelectCallBack;
        _selected = new List<bool>();
        _selected.AddRange(new bool[TextureLayers.arraySize]);

    }

    #region TextureLayerList
    private void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
    {
        EditorGUI.BeginChangeCheck();
        EditorGUI.indentLevel++;
        _selected[index] = EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width - 20, EditorGUIUtility.singleLineHeight), TextureLayers.GetArrayElementAtIndex(index), false);
        if (_selected[index])
        {
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + _fieldSpacing * 1, rect.width, EditorGUIUtility.singleLineHeight), TextureLayers.GetArrayElementAtIndex(index).FindPropertyRelative("Tiling"), new GUIContent("Tiling"));
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + _fieldSpacing * 2, rect.width, EditorGUIUtility.singleLineHeight), TextureLayers.GetArrayElementAtIndex(index).FindPropertyRelative("Color"), new GUIContent("Color"));
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + _fieldSpacing * 3, rect.width, EditorGUIUtility.singleLineHeight), TextureLayers.GetArrayElementAtIndex(index).FindPropertyRelative("MainTexture"), new GUIContent("Main Texture"));
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + _fieldSpacing * 4, rect.width, EditorGUIUtility.singleLineHeight), TextureLayers.GetArrayElementAtIndex(index).FindPropertyRelative("NormalTexture"), new GUIContent("Normal Texture"));
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + _fieldSpacing * 5, rect.width, EditorGUIUtility.singleLineHeight), TextureLayers.GetArrayElementAtIndex(index).FindPropertyRelative("NormalStrength"), new GUIContent("Normal Strength"));
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + _fieldSpacing * 6, rect.width, EditorGUIUtility.singleLineHeight), TextureLayers.GetArrayElementAtIndex(index).FindPropertyRelative("Metallic"), new GUIContent("Metallic"));
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + _fieldSpacing * 7, rect.width, EditorGUIUtility.singleLineHeight), TextureLayers.GetArrayElementAtIndex(index).FindPropertyRelative("Smoothness"), new GUIContent("Smoothness"));
            EditorGUI.indentLevel--;
        }
        else if(terrainParent.textureLayers[index].MainTexture != null)
        {
            EditorGUI.indentLevel++;
            EditorGUI.DrawPreviewTexture(new Rect(rect.x + rect.width - (_fieldSpacing * 3), rect.y, _fieldSpacing * 3, _fieldSpacing * 3), terrainParent.textureLayers[index].MainTexture);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + _fieldSpacing * 1, rect.width - (_fieldSpacing * 4), EditorGUIUtility.singleLineHeight), TextureLayers.GetArrayElementAtIndex(index).FindPropertyRelative("Color"), new GUIContent(""));
            EditorGUI.indentLevel--;
        }
        EditorGUI.indentLevel--;
        if (EditorGUI.EndChangeCheck()) terrainParent.InjectShaderParams(index, terrainParent.textureLayers[index]);
    }

    private void AddCallBack(ReorderableList reorderableList)
    {
        if (TextureLayers.arraySize >= 5) return; // max number of textures
        
        if(TextureLayers.arraySize >= 1) TextureLayers.arraySize++;
        else terrainParent.textureLayers.Add(new TerrainTextureLayer());

        _selected.Add(new bool());
        serializedObject.ApplyModifiedProperties();
    }

    private void RemoveCallBack(ReorderableList reorderableList)
    {
        TextureLayers.arraySize--;
        _selected.RemoveAt(_selected.Count - 1);
        serializedObject.ApplyModifiedProperties();
    }

    private float HeightCallBack(int index)
    {
        if (_selected[index])
            return _fieldSpacing * 8; // when element is expanded, there are six lines
        else
            return _fieldSpacing * 3;  // when element is folded
    }

    private void DrawHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, "Texture Layers");
    }

    private void ListChanged(ReorderableList reorderableList)
    {
        serializedObject.ApplyModifiedProperties();
        terrainParent.ApplyMaterialParams();
    }

    private void SelectCallBack(ReorderableList reorderableList)
    {
        terrainParent.layerColorIndex = reorderableList.selectedIndices[0];
    }
    #endregion


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


        if (tileState == TileState.add)
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
        }

        else if (tileState == TileState.remove)
        {
            Vector3 rootPos = terrainParent.TerrainList[new Vector2Int(0, 0)].transform.position;
            float rootSize = terrainParent.parameters.Width / 2;
            // render empty edges
            foreach (KeyValuePair<Vector2Int, Terrain> tile in terrainParent.TerrainList)
            {
                Vector3 offset = new Vector3(tile.Key.x * terrainParent.parameters.Width, 0, tile.Key.y * terrainParent.parameters.Length);
                //Handles.DrawWireCube(rootPos + offset, rootSize);
                if (Handles.Button(rootPos + offset, Quaternion.Euler(90, 0, 0), rootSize, rootSize, Handles.RectangleHandleCap))
                {
                    terrainParent.DeleteTerrain(tile.Key);
                    return;
                }
            }
        }


        //.......brush.....................................................................................
        // We use hotControl to lock focus onto the editor (to prevent deselection)
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        if (Event.current.type == EventType.MouseDown && editorState == state.Brush && Event.current.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainMask))
            {
                GUIUtility.hotControl = controlID;
            }
        }

        if (Event.current.type == EventType.MouseDrag && editorState == state.Brush && Event.current.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainMask))
            {
                terrainParent.Paint(hit);
            }

            Event.current.Use();
        }
        else if (Event.current.type == EventType.MouseUp)
        {
            GUIUtility.hotControl = 0;
            Event.current.Use();
        }
        //.................................................................................................

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        RenderMenuBar(editorState);
        switch (editorState)
        {
            case state.Tiles:
                RenderTileMenu();
                break;
            case state.Brush:
                RenderBrushMenu();
                break;
        }
        serializedObject.ApplyModifiedProperties();
    }

    private void RenderTileMenu()
    {
        //base.OnInspectorGUI();
        EditorGUILayout.PropertyField(TerrainParameters);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Base Terrain"))
        {
            terrainParent.CreateBaseTerrain();
        }

        Color addColor = (tileState != TileState.add) ? new Color(0.6f, 1, 0.6f) : new Color(0.9f, 1, 0.9f);
        if (UIStyles.ColoredButton("Add Terrain Tile", addColor))
        {
            if(tileState != TileState.add) tileState = TileState.add;
            else tileState = TileState.none;

            if (tileState == TileState.add) RefreshEmptyTiles();
        }
        GUILayout.EndHorizontal();

        Color removeColor = (tileState != TileState.remove)? new Color(1, 0.6f, 0.6f) : new Color(1, 0.9f, 0.9f);
        if (UIStyles.ColoredButton("remove Terrain Tile", removeColor))
        {
            if (tileState != TileState.remove) tileState = TileState.remove;
            else tileState = TileState.none;
        }
        if (GUILayout.Button("Re Render"))
        {
            terrainParent.RerenderAll();
        }
    }

    private void RenderBrushMenu()
    {
        EditorGUILayout.PropertyField(Shader);
        EditorGUILayout.PropertyField(Material);
        EditorGUILayout.PropertyField(Brush);
        _reorderableList.DoLayoutList();

        UIStyles.BeginColoredVertical(new Color(0.8f,0.8f,0.8f));
        GUILayout.Label("Mask");
        UIStyles.BeginColoredHorizontal(new Color(0.95f, 0.95f, 0.95f));
        GUILayout.Box(terrainParent.mask, new GUILayoutOption[] { GUILayout.Width(50), GUILayout.Height(50) });
        GUILayout.Space(10);
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        if (terrainParent.mask == null)
        {
            GUILayout.Label("path: Assets/", new GUILayoutOption[] { GUILayout.Width(80) });
            terrainParent.maskPath = GUILayout.TextField(terrainParent.maskPath);
        }
        else { EditorGUILayout.PropertyField(Mask, new GUIContent("")); }
        GUILayout.EndHorizontal();
        EditorGUILayout.PropertyField(MaskDimensions, new GUIContent(""));
        if(UIStyles.HorizontalCenteredButton((terrainParent.mask == null)?"create" : "resize", 120))
        {
            terrainParent.CreateOrResizeMask();
        }
        GUILayout.EndVertical();
        UIStyles.EndColoredHorizontal();
        UIStyles.EndColoredVertical();

        EditorGUILayout.PropertyField(BrushSize);
        EditorGUILayout.PropertyField(Opacity);
    }

    private void RenderMenuBar(state eState)
    {
        GUILayout.BeginHorizontal();
        for(int i = 0; i < (int)state.max; i++)
        {
            Color btnColor = new Color(1, 1, 0.8f);
            if (i == (int)eState)
                btnColor = new Color(1, 0.8f, 0.4f);
            if (UIStyles.ColoredButton(((state)i).ToString(), btnColor))
            {
                editorState = (state)i;
            }
        }
        GUILayout.EndHorizontal();
    }

    private void RefreshEmptyTiles()
    {
        emptyTiles = terrainParent.GetEmptyTiles();
    }
}

public static class UIStyles
{
    public static bool ColoredButton(String str, Color color)
    {
        GUI.color = color;
        bool pressed = GUILayout.Button(str);
        GUI.color = Color.white;
        return pressed;
    }

    public static bool HorizontalCenteredButton(String str, float width)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        bool pressed = GUILayout.Button(str, new GUILayoutOption[] { GUILayout.Width(width) });
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        return pressed;
    }

    public static void BeginColoredVertical(Color color)
    {
        GUI.backgroundColor = color;
        GUILayout.BeginVertical(GUI.skin.box);
        GUI.backgroundColor = Color.white;
    }

    public static void EndColoredVertical()
    {
        GUILayout.EndVertical();
    }

    public static void BeginColoredHorizontal(Color color)
    {
        GUI.backgroundColor = color;
        GUILayout.BeginHorizontal(GUI.skin.box);
        GUI.backgroundColor = Color.white;
    }
    public static void EndColoredHorizontal()
    {
        GUILayout.EndHorizontal();
    }
}
#endif


public class TextureHelper {
    public static void CopyTexture(Texture2D src, int srcXpos, int srcYpos, int srcWidth, int srcHeight, Texture2D dst, int dstXpos, int dstYpos)
    {
        if(srcXpos + srcWidth > src.width)
        {
            Debug.LogError(" out of bounds on X axis");
            return;
        }
        if(srcYpos + srcHeight > src.height)
        {
            Debug.LogError(" out of bounds on Y axis");
            return ;
        }

        Color[] img = src.GetPixels(srcXpos, srcYpos, srcWidth, srcHeight);
        dst.SetPixels(dstXpos, dstYpos, srcWidth, srcHeight, img);
    }
}