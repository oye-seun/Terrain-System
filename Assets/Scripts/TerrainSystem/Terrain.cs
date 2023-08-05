using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class Terrain : MonoBehaviour
{
    public TerrainParameters Parameters;
    public Vector2Int TerrainPos;

    private List<Vector3> verts;
    private Mesh _mesh;
    private List<int> triangles;
    
    public void GenerateMesh()
    {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;


        // inrement on the x and z axis
        float xincr = Parameters.Width / Parameters.VertsWidth;
        float zincr = Parameters.Length / Parameters.VertsWidth;

        Vector2 extremeTopY = new Vector2(float.PositiveInfinity, float.NegativeInfinity);

        // generate simple square mesh (verts) using perlin noise
        verts = new List<Vector3>();
        for (int j = 0; j < Parameters.VertsLength; j++)
        {
            for (int i = 0; i < Parameters.VertsWidth; i++)
            {
                float Yval = Noise.GetNoiseVal(Parameters.NoiseXOrigin, Parameters.NoiseYOrigin, i, j, Parameters.NoiseScale, Parameters.NoiseOctaves, Parameters.NoisePersistance, Parameters.NoiseLacunarity);
                if (Yval > extremeTopY.y) extremeTopY.y = Yval;
                if (Yval < extremeTopY.x) extremeTopY.x = Yval;
                verts.Add(new Vector3((i * xincr) + transform.position.x, Yval, (j * zincr) + transform.position.z));
            }
        }

        // lerp mesh to height
        for (int i = 0; i < verts.Count; i++)
        {
            verts[i] = new Vector3(verts[i].x, Mathf.Lerp(Parameters.MinHeight, Parameters.MaxHeight, Mathf.InverseLerp(extremeTopY.x, extremeTopY.y, verts[i].y)), verts[i].z);
        }

        // generate triangles
        triangles = new List<int>();
        for (int j = 0; j < verts.Count - Parameters.VertsWidth; j += Parameters.VertsWidth)
        {
            for (int i = j; i < (j + Parameters.VertsWidth - 1); i++)
            {
                triangles.Add(i); triangles.Add(i + Parameters.VertsWidth + 1); triangles.Add(i + 1);
                triangles.Add(i); triangles.Add(i + Parameters.VertsWidth); triangles.Add(i + Parameters.VertsWidth + 1);
            }
        }

        // compile mesh
        _mesh.Clear();
        _mesh.vertices = verts.ToArray();
        _mesh.triangles = triangles.ToArray();
        _mesh.RecalculateNormals();
    }
}


#if (UNITY_EDITOR)
[CustomEditor(typeof(Terrain))]
public class TerrainEditor : UnityEditor.Editor
{
    Terrain terrain;
    //bool keyPressed;
    private void OnEnable()
    {
        terrain = (Terrain)target;
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

        if (GUILayout.Button("Generate Mesh"))
        {
            terrain.GenerateMesh();
        }
    }
}
#endif
