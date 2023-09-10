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
    public bool ShowVerts;

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

        Parameters.VertsWidth++;
        Parameters.VertsLength++;

        // generate simple square mesh (verts) using perlin noise
        verts = new List<Vector3>();
        for (int j = 0; j < Parameters.VertsLength; j++)
        {
            for (int i = 0; i < Parameters.VertsWidth; i++)
            {
                float Yval = Noise.GetNoiseVal(Parameters.NoiseXOrigin, Parameters.NoiseYOrigin, i + (TerrainPos.x * (Parameters.VertsWidth-1)), j + (TerrainPos.y * (Parameters.VertsLength-1)), Parameters.NoiseScale, Parameters.NoiseOctaves, Parameters.NoisePersistance, Parameters.NoiseLacunarity);
                verts.Add(new Vector3((i * xincr)  - (Parameters.Width/2), Yval, (j * zincr) - (Parameters.Length / 2)));
            }
        }

        for (int i = 0; i < verts.Count; i++)
        {
            verts[i] = new Vector3(verts[i].x, verts[i].y * Parameters.Height, verts[i].z);
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

    private void OnDrawGizmos()
    {
        if (ShowVerts)
        {
            foreach (Vector3 v in verts)
            {
                Gizmos.DrawCube(v + transform.position, Vector3.one * 0.02f);
            }
        }
        
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
