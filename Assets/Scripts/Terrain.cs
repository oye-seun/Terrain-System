using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Terrain : MonoBehaviour
{
    [SerializeField] private float _width;
    [SerializeField] private float _height;
    [SerializeField] private int _vertsWidth;
    [SerializeField] private int _vertsHeight;

    private Mesh _mesh;
    
    private void GenerateMesh()
    {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;
    }
}


#if (UNITY_EDITOR)
[CustomEditor(typeof(Terrain))]
public class TerrainEditor : UnityEditor.Editor
{
    Terrain terrain;
    bool keyPressed;
    private void OnEnable()
    {
        terrain = (Terrain)target;
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
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        if (GUILayout.Button("Next Triangulation"))
        {
            //terrain.ProceduralTriangulate();
            //EditorUtility.SetDirty(dollyLookCamUpdater);
        }
    }
}
#endif
