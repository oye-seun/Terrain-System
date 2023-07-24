using System.Collections.Generic;
using UnityEngine;
# if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

public class WaterMeshGen : MonoBehaviour
{
    private Mesh mesh;
    //public Vector3 pos;
    //public SimpleTransform[] Points;
    public BezierPath[] BezierPaths;
    public float HalfWidth;
    public int VertsPerLine;
    public bool ShowTris;
    public bool ShowVerts;
    public bool ShowBounds;

    private List<Vector3> verts;

    //public DelauneyTriangulationActive Delauney;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateWaterMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        verts = new List<Vector3> ();

        // Generate the vertices
        foreach(BezierPath path in BezierPaths)
        {
            path.GenerateVerts(verts, HalfWidth, VertsPerLine);
        }

        // Generate the Global bounding Box in order to generate Supra Triangle
        BoundingBox globalBoundingBox = new BoundingBox(Vector3.positiveInfinity, Vector3.negativeInfinity);
        foreach (BezierPath path in BezierPaths)
        {
            BoundingBox.CompareBoundingBoxAndSetExtreme(globalBoundingBox, path.GenerateBoundingBox(HalfWidth));
        }

        // Generate Supra Triangle
        Vector3 globalBoxCentre = globalBoundingBox.CalculateCentre();
        float radius = Vector3.Distance(globalBoxCentre, globalBoundingBox.max);
        Triangle supraTriangle = new Triangle
            (
                globalBoxCentre + (new Vector3(Mathf.Cos(0 * Mathf.Deg2Rad), 0, Mathf.Sin(0 * Mathf.Deg2Rad)) * 2 * radius),
                globalBoxCentre + (new Vector3(Mathf.Cos(120 * Mathf.Deg2Rad), 0, Mathf.Sin(120 * Mathf.Deg2Rad)) * 2 * radius),
                globalBoxCentre + (new Vector3(Mathf.Cos(240 * Mathf.Deg2Rad), 0, Mathf.Sin(240 * Mathf.Deg2Rad)) * 2 * radius)
            );


        //Delauney.SetDelauneyProperties(verts, supraTriangle);


        // Generate Triangles using Delauney Triangulation
        DelauneyTriangulation triangulator = new DelauneyTriangulation(verts, supraTriangle);
        List<int> triangles = triangulator.Triangulate(true);
        Debug.Log("triangle count: " + triangles.Count);

        // clean the triangles
        for (int i = triangles.Count - 3; i >= 0; i -= 3)
        {
            Vector3 centroid = (verts[triangles[i]] + verts[triangles[i + 1]] + verts[triangles[i + 2]]) / 3;

            bool withinCurve = false;
            for(int j = 0; j < BezierPaths.Length && !withinCurve; j++)
            {
                withinCurve = BezierPaths[j].CheckPosWithinWidthX_Z(centroid, HalfWidth);
            }
            if (!withinCurve)
            {
                triangles.RemoveAt(i + 2); triangles.RemoveAt(i + 1); triangles.RemoveAt(i);
            }
        }

        // Generate UV
        Vector2[] uv = new Vector2[verts.Count];
        for(int i = 0; i < verts.Count; i++)
        {
            uv[i] = new Vector2(
                Mathf.InverseLerp(globalBoundingBox.min.x, globalBoundingBox.max.x, verts[i].x),
                Mathf.InverseLerp(globalBoundingBox.min.z, globalBoundingBox.max.z, verts[i].z));
        }

        //verts = ShiftVertsToCentre(verts, transform.position);
        List<Vector3> centredVerts = new List<Vector3>();
        centredVerts.AddRange(verts);
        centredVerts = ShiftVertsToCentre(centredVerts, transform.position);

        mesh.Clear();
        mesh.vertices = centredVerts.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uv;
        mesh.RecalculateNormals();
        //_mesh.uv = uv1.ToArray();

        //--------- Preview Purposes -----------------------------------------------------------------------------------------------------
        bound = globalBoundingBox;
        sTriangle = supraTriangle;
        pTriangles = triangles;
        Debug.Log("vertices: " + verts.Count);
        //bound = GeneratePathBoundingBox(BezierPaths[2], HalfWidth);
        //Debug.Log("minVal: " + bound.min);
        //Debug.Log("maxVal: " + bound.max);
        //--------------------------------------------------------------------------------------------------------------------------------
    }

    private List<Vector3> ShiftVertsToCentre(List<Vector3> verts, Vector3 centre)
    {
        for (int i = 0; i < verts.Count; i++)
        {
            Vector3 disp = verts[i] - centre;
            //disp.y = verts[i].y;
            verts[i] = disp;
        }
        return verts;
    }




    // Test Variables
    private BoundingBox bound;
    Triangle sTriangle;
    List<int> pTriangles;
    private void OnDrawGizmos()
    {
        //---------- Test if BezierPosition Works --------------------------------------------------------------------------------------------
        //int div = 30;
        //DrawBezierCurve(BezierPaths[0].Start, BezierPaths[0].FirstHandle, BezierPaths[0].Path[0].A, BezierPaths[0].Path[0].Centre, div);
        //for (int i = 1; i < BezierPaths[0].Path.Count; i++)
        //{
        //    DrawBezierCurve(BezierPaths[0].Path[i - 1].Centre, BezierPaths[0].Path[i - 1].B, BezierPaths[0].Path[i].A, BezierPaths[0].Path[i].Centre, div);
        //}
        //int lInd = BezierPaths[0].Path.Count - 1;
        //DrawBezierCurve(BezierPaths[0].Path[lInd].Centre, BezierPaths[0].Path[lInd].B, BezierPaths[0].LastHandle, BezierPaths[0].End, div);
        //------------------------------------------------------------------------------------------------------------------------------------

        //------------ Preview Generated vertices -------------------------------------------------------------------------------------------
        for (int i = 0; ShowVerts && i < verts.Count ; i++)
        {
            Gizmos.DrawCube(verts[i], Vector3.one * 0.05f);
        }
        //----------------------------------------------------------------------------------------------------------------------------------

        //------------ Preview Bounding Box ------------------------------------------------------------------------------------------------
        if (ShowBounds)
        {
            Vector3 b1 = new Vector3(bound.min.x, bound.min.y, bound.min.z);
            Vector3 b2 = new Vector3(bound.min.x, bound.min.y, bound.max.z);
            Vector3 b3 = new Vector3(bound.max.x, bound.min.y, bound.max.z);
            Vector3 b4 = new Vector3(bound.max.x, bound.min.y, bound.min.z);

            Vector3 b5 = new Vector3(bound.min.x, bound.max.y, bound.min.z);
            Vector3 b6 = new Vector3(bound.min.x, bound.max.y, bound.max.z);
            Vector3 b7 = new Vector3(bound.max.x, bound.max.y, bound.max.z);
            Vector3 b8 = new Vector3(bound.max.x, bound.max.y, bound.min.z);

            Debug.DrawLine(b1, b2, Color.green);
            Debug.DrawLine(b2, b3, Color.green);
            Debug.DrawLine(b3, b4, Color.green);
            Debug.DrawLine(b4, b1, Color.green);

            Debug.DrawLine(b5, b6, Color.green);
            Debug.DrawLine(b6, b7, Color.green);
            Debug.DrawLine(b7, b8, Color.green);
            Debug.DrawLine(b8, b5, Color.green);

            Debug.DrawLine(b1, b5, Color.green);
            Debug.DrawLine(b2, b6, Color.green);
            Debug.DrawLine(b3, b7, Color.green);
            Debug.DrawLine(b4, b8, Color.green);

            for (int i = 0; i < BezierPaths.Length; i++)
            {
                foreach (BoundingBox bound in BezierPaths[i].BoundingBoxes)
                {
                    Vector3 c1 = new Vector3(bound.min.x, bound.min.y, bound.min.z);
                    Vector3 c2 = new Vector3(bound.min.x, bound.min.y, bound.max.z);
                    Vector3 c3 = new Vector3(bound.max.x, bound.min.y, bound.max.z);
                    Vector3 c4 = new Vector3(bound.max.x, bound.min.y, bound.min.z);

                    Vector3 c5 = new Vector3(bound.min.x, bound.max.y, bound.min.z);
                    Vector3 c6 = new Vector3(bound.min.x, bound.max.y, bound.max.z);
                    Vector3 c7 = new Vector3(bound.max.x, bound.max.y, bound.max.z);
                    Vector3 c8 = new Vector3(bound.max.x, bound.max.y, bound.min.z);

                    Debug.DrawLine(c1, c2, Color.green);
                    Debug.DrawLine(c2, c3, Color.green);
                    Debug.DrawLine(c3, c4, Color.green);
                    Debug.DrawLine(c4, c1, Color.green);

                    Debug.DrawLine(c5, c6, Color.green);
                    Debug.DrawLine(c6, c7, Color.green);
                    Debug.DrawLine(c7, c8, Color.green);
                    Debug.DrawLine(c8, c5, Color.green);

                    Debug.DrawLine(c1, c5, Color.green);
                    Debug.DrawLine(c2, c6, Color.green);
                    Debug.DrawLine(c3, c7, Color.green);
                    Debug.DrawLine(c4, c8, Color.green);
                }
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------------

        //------------- Preview Supra Triangle -------------------------------------------------------------------------------------------------
        if (ShowBounds)
        {
            Debug.DrawLine(sTriangle.A, sTriangle.B, Color.cyan);
            Debug.DrawLine(sTriangle.B, sTriangle.C, Color.cyan);
            Debug.DrawLine(sTriangle.C, sTriangle.A, Color.cyan);
        }
        //-------------------------------------------------------------------------------------------------------------------------------------

        //------------- Preview Triangles------------------------------------------------------------------------------------------------------
        for (int i = 0; ShowTris && i < pTriangles.Count; i += 3)
        {
            //if (triangles[i] >= _cutoutTopVerts.Count || triangles[i] < 0)
            //{
            //    Debug.Log("out of bound tris: " + triangles[i]);
            //}
            Debug.DrawLine(verts[pTriangles[i]], verts[pTriangles[i + 1]], Color.red);
            Debug.DrawLine(verts[pTriangles[i + 1]], verts[pTriangles[i + 2]], Color.red);
            Debug.DrawLine(verts[pTriangles[i + 2]], verts[pTriangles[i]], Color.red);
        }
        //-------------------------------------------------------------------------------------------------------------------------------------
    }

    // ------- Testing Purposes ----------------------------------------------------------------------------------------------------------------
    private void DrawBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int div)
    {
        for (int i = 0; i < div; i++)
        {
            Vector3 pos1 = BezierPath.BezierPosition(p0, p1, p2, p3, (float)i / div);
            Vector3 pos2 = BezierPath.BezierPosition(p0, p1, p2, p3, (float)(i + 1) / div);
            Debug.DrawLine(pos1, pos2, Color.cyan);
        }
    }
    //-----------------------------------------------------------------------------------------------------------------------------------------

}




[CustomEditor(typeof(WaterMeshGen))]
public class WaterMeshGenEditor : Editor
{
    private float _size = 0.1f;
    private Vector3 _shape = new Vector3(0.3f, 1f, 0.3f);
    private float _lineDist = 1;

    private SerializedProperty _halfWidth;
    private SerializedProperty _vertsPerLine;
    private SerializedProperty _showTris;
    private SerializedProperty _showVerts;
    private SerializedProperty _showBounds;
    //private SerializedProperty _delauney;

    private WaterMeshGen WMG;
    private SerializedProperty _list;
    private ReorderableList _reorderableList;
    private List<bool> _selected;
    private const float _fieldSpacing = 20;

    private void OnEnable()
    {
        WMG = (WaterMeshGen)target;
        //_list = serializedObject.FindProperty("Points");
        _halfWidth = serializedObject.FindProperty("HalfWidth");
        _vertsPerLine = serializedObject.FindProperty("VertsPerLine");
        _showTris = serializedObject.FindProperty("ShowTris");
        _showVerts = serializedObject.FindProperty("ShowVerts");
        _showBounds = serializedObject.FindProperty("ShowBounds");
        //_delauney = serializedObject.FindProperty("Delauney");

        _list = serializedObject.FindProperty("BezierPaths");
        _reorderableList = new ReorderableList(serializedObject, _list, true, true, true, true);
        _reorderableList.drawElementCallback = DrawListItems;
        _reorderableList.elementHeightCallback = HeightCallBack;
        _reorderableList.drawHeaderCallback = DrawHeader;
        _reorderableList.onAddCallback = AddCallBack;
        _reorderableList.onRemoveCallback = RemoveCallBack;
        _selected = new List<bool>();
        _selected.AddRange(new bool[_list.arraySize]);
    }

    //private void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
    //{
    //    EditorGUI.indentLevel++;
    //    _selected[index] = EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width - 20, EditorGUIUtility.singleLineHeight), _list.GetArrayElementAtIndex(index), false);
    //    if (_selected[index])
    //    {
    //        EditorGUI.indentLevel++;
    //        EditorGUI.PropertyField(new Rect(rect.x, rect.y + _fieldSpacing * 1, rect.width, EditorGUIUtility.singleLineHeight), _list.GetArrayElementAtIndex(index).FindPropertyRelative("Position"), new GUIContent("Position"));
    //        EditorGUI.PropertyField(new Rect(rect.x, rect.y + _fieldSpacing * 2, rect.width, EditorGUIUtility.singleLineHeight), _list.GetArrayElementAtIndex(index).FindPropertyRelative("Rotation"), new GUIContent("Rotation"));
    //        //EditorGUI.PropertyField(new Rect(rect.x, rect.y + _fieldSpacing * 3, rect.width, EditorGUIUtility.singleLineHeight), _list.GetArrayElementAtIndex(index).FindPropertyRelative("_movementType"), new GUIContent("MovementType"));
    //        //EditorGUI.PropertyField(new Rect(rect.x, rect.y + _fieldSpacing * 4, rect.width, EditorGUIUtility.singleLineHeight), _list.GetArrayElementAtIndex(index).FindPropertyRelative("_duration"), new GUIContent("Duration"));
    //        if (GUI.Button(new Rect(rect.x, rect.y + _fieldSpacing * 3, rect.width / 2, EditorGUIUtility.singleLineHeight), new GUIContent("Show Pos Handle")))
    //        {
    //            _list.GetArrayElementAtIndex(index).FindPropertyRelative("PosHandle").boolValue = !_list.GetArrayElementAtIndex(index).FindPropertyRelative("PosHandle").boolValue;
    //        }

    //        if (GUI.Button(new Rect(rect.x + (rect.width / 2), rect.y + _fieldSpacing * 3, rect.width / 2, EditorGUIUtility.singleLineHeight), new GUIContent("Show Rot Handle")))
    //        {
    //            _list.GetArrayElementAtIndex(index).FindPropertyRelative("RotHandle").boolValue = !_list.GetArrayElementAtIndex(index).FindPropertyRelative("RotHandle").boolValue;
    //        }
    //        EditorGUI.indentLevel--;
    //    }
    //    EditorGUI.indentLevel--;
    //}

    private void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
    {
        EditorGUI.indentLevel++;
        _selected[index] = EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width - 20, EditorGUIUtility.singleLineHeight), _list.GetArrayElementAtIndex(index), false);
        if (_selected[index])
        {
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + _fieldSpacing * 1, rect.width, EditorGUIUtility.singleLineHeight), _list.GetArrayElementAtIndex(index).FindPropertyRelative("Start"), new GUIContent("Start Pos"));
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + _fieldSpacing * 2, rect.width, EditorGUIUtility.singleLineHeight), _list.GetArrayElementAtIndex(index).FindPropertyRelative("FirstHandle"), new GUIContent("FirstHandle"));
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + _fieldSpacing * 3, rect.width, EditorGUIUtility.singleLineHeight), _list.GetArrayElementAtIndex(index).FindPropertyRelative("LastHandle"), new GUIContent("LastHandle"));
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + _fieldSpacing * 4, rect.width, EditorGUIUtility.singleLineHeight), _list.GetArrayElementAtIndex(index).FindPropertyRelative("End"), new GUIContent("End"));
            EditorGUI.PropertyField(new Rect(rect.x, rect.y + _fieldSpacing * 5, rect.width, EditorGUIUtility.singleLineHeight), _list.GetArrayElementAtIndex(index).FindPropertyRelative("Segments"), new GUIContent("Segments"));

            EditorGUI.PropertyField(new Rect(rect.x, rect.y + _fieldSpacing * 6, rect.width / 2, EditorGUIUtility.singleLineHeight), _list.GetArrayElementAtIndex(index).FindPropertyRelative("ShowHandles"), new GUIContent("ShowHandles"));
            EditorGUI.PropertyField(new Rect(rect.x + (rect.width / 2), rect.y + _fieldSpacing * 6, rect.width, EditorGUIUtility.singleLineHeight), _list.GetArrayElementAtIndex(index).FindPropertyRelative("ShowNormals"), new GUIContent("ShowNormals"));
            if (GUI.Button(new Rect(rect.x, rect.y + _fieldSpacing * 7, rect.width / 2, EditorGUIUtility.singleLineHeight), new GUIContent("Add Control point")))
            {
                //_list.GetArrayElementAtIndex(index).FindPropertyRelative("PosHandle").boolValue = !_list.GetArrayElementAtIndex(index).FindPropertyRelative("PosHandle").boolValue;
                int lastPathindex = WMG.BezierPaths[index].Path.Count - 1;
                Vector3 dir = Vector3.zero;
                Vector3 centre = Vector3.zero;
                if (lastPathindex >= 0)
                {
                    centre = (WMG.BezierPaths[index].End + WMG.BezierPaths[index].Path[lastPathindex].Centre) / 2;
                    dir = GetCentreDir(WMG.BezierPaths[index].End - WMG.BezierPaths[index].LastHandle, WMG.BezierPaths[index].Path[lastPathindex].Centre - WMG.BezierPaths[index].Path[lastPathindex].A);
                }
                else
                {
                    centre = (WMG.BezierPaths[index].End + WMG.BezierPaths[index].Start) / 2;
                    dir = GetCentreDir(WMG.BezierPaths[index].End - WMG.BezierPaths[index].LastHandle, WMG.BezierPaths[index].Start - WMG.BezierPaths[index].FirstHandle);
                }

                WMG.BezierPaths[index].AddIntermediate(new ControlPoint(centre, centre + (dir * 1f), centre - (dir * 1f)));
            }

            if (GUI.Button(new Rect(rect.x + (rect.width / 2), rect.y + _fieldSpacing * 7, rect.width / 2, EditorGUIUtility.singleLineHeight), new GUIContent("Remove Control Point")))
            {
                int lastPathindex = WMG.BezierPaths[index].Path.Count - 1;
                if (lastPathindex >= 0)
                {
                    WMG.BezierPaths[index].Path.RemoveAt(lastPathindex);
                }
            }
            EditorGUI.indentLevel--;
        }
        EditorGUI.indentLevel--;
    }

    private void AddCallBack(ReorderableList reorderableList)
    {
        _list.arraySize++;
        //_list.GetArrayElementAtIndex(_list.arraySize - 1).FindPropertyRelative("Rotation").quaternionValue = Quaternion.identity;
        _selected.Add(new bool());
        serializedObject.ApplyModifiedProperties();
    }

    private void RemoveCallBack(ReorderableList reorderableList)
    {
        _list.arraySize--;
        _selected.RemoveAt(_selected.Count - 1);
        serializedObject.ApplyModifiedProperties();
    }

    private float HeightCallBack(int index)
    {
        if (_selected[index])
            return _fieldSpacing * 8; // when element is expanded, there are six lines
        else
            return _fieldSpacing;  // when element is folded
    }

    private void DrawHeader(Rect rect)
    {
        //EditorGUI.LabelField(rect, "Points");
        EditorGUI.LabelField(rect, "Water Paths");
    }


    private void OnSceneGUI()
    {
        Handles.color = Color.red;
        //drawHandles(WMG.Points);
        drawCurves(WMG.BezierPaths);
        drawNormals(WMG.BezierPaths);
    }

    private void drawCurves(BezierPath[] paths)
    {
        foreach(BezierPath path in paths)
        {
            if(path.Path.Count == 0)
                Handles.DrawBezier(path.Start, path.End, path.FirstHandle, path.LastHandle, Color.yellow, Texture2D.whiteTexture, 2);
            else
            {
                //start tp first control point
                Handles.DrawBezier(path.Start, path.Path[0].Centre, path.FirstHandle, path.Path[0].A, Color.yellow, Texture2D.whiteTexture, 2);
                if(path.ShowHandles)
                    TrackControlPoint(path.Path[0]);

                for (int i = 1; i < path.Path.Count; i++)
                {
                    Handles.DrawBezier(path.Path[i-1].Centre, path.Path[i].Centre, path.Path[i-1].B, path.Path[i].A, Color.yellow, Texture2D.whiteTexture, 2);
                    if(path.ShowHandles)
                        TrackControlPoint(path.Path[i]);
                }

                int lastPathIndex = path.Path.Count - 1;
                Handles.DrawBezier(path.Path[lastPathIndex].Centre, path.End, path.Path[lastPathIndex].B, path.LastHandle, Color.yellow, Texture2D.whiteTexture, 2);
            }

            if (path.ShowHandles)
            {
                Vector3 startDiff = TrackBezierPoint(path.Start) - path.Start;
                path.Start += startDiff;

                path.FirstHandle = TrackBezierPoint(path.FirstHandle);
                path.FirstHandle += startDiff;
                Handles.DrawLine(path.Start, path.FirstHandle);

                Vector3 endDiff = TrackBezierPoint(path.End) - path.End;
                path.End += endDiff;
                path.LastHandle = TrackBezierPoint(path.LastHandle);
                path.LastHandle += endDiff;
                Handles.DrawLine(path.LastHandle, path.End);
            }
        }
    }


    private void drawNormals(BezierPath[] paths)
    {
        foreach(BezierPath path in paths)
        {
            if (path.ShowNormals)
            {
                int lInd = path.Path.Count - 1;
                if (lInd >= 0) 
                {
                    DrawBezierNormals(path.Start, path.FirstHandle, path.Path[0].A, path.Path[0].Centre, path.Segments);
                    for (int i = 1; i < path.Path.Count; i++)
                    {
                        DrawBezierNormals(path.Path[i - 1].Centre, path.Path[i - 1].B, path.Path[i].A, path.Path[i].Centre, path.Segments);
                    }
                    DrawBezierNormals(path.Path[lInd].Centre, path.Path[lInd].B, path.LastHandle, path.End, path.Segments);
                }
                else
                {
                    DrawBezierNormals(path.Start, path.FirstHandle, path.LastHandle, path.End, path.Segments);
                }
            }
        }
        
    }

    private void DrawBezierNormals(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int div)
    {
        for (int i = 0; i < div + 1; i++)
        {
            Vector3 pos = BezierPath.BezierPosition(p0, p1, p2, p3, (float)i / div);
            Vector3 norm = BezierPath.BezierNormal(p0, p1, p2, p3, (float)i / div);
            Handles.DrawLine(pos - (norm * WMG.HalfWidth), pos + (norm * WMG.HalfWidth));
        }
    }

    //private void drawHandles(SimpleTransform[] points)
    //{
    //    foreach (SimpleTransform point in points)
    //    {
    //        //Handles.DrawWireCube(point.Position, _shape * _size);
    //        Handles.DrawLine(point.Position + (point.Rotation * Vector3.right * _lineDist), point.Position - (point.Rotation * Vector3.right * _lineDist));

    //        // draw button
    //        if(Handles.Button(point.Position, Quaternion.identity, _size, _size, Handles.CubeHandleCap))
    //        {
    //            if(!point.PosHandle || !point.RotHandle)
    //            {
    //                point.PosHandle = true;
    //                point.RotHandle = true;
    //            }
    //            else if(point.PosHandle && point.RotHandle)
    //            {
    //                point.PosHandle = false;
    //                point.RotHandle = false;
    //            }
    //        }

    //        // draw handle to adjust position
    //        if (point.PosHandle)
    //        {
    //            Vector3 moved = Handles.PositionHandle(point.Position, point.Rotation);
    //            point.Position = moved;
    //        }

    //        // draw handle to adjust rotation
    //        if (point.RotHandle)
    //        {
    //            Quaternion rotated = Handles.RotationHandle(point.Rotation, point.Position);
    //            //point.Rotation.y = rotated.y;
    //            Vector3 euler = rotated.eulerAngles;
    //            euler.x = 0; euler.z = 0;
    //            point.Rotation = Quaternion.Euler(euler);
    //        }
    //    }
    //}

    private void ShowOtherFields()
    {
        EditorGUILayout.PropertyField(_halfWidth);
        EditorGUILayout.PropertyField(_vertsPerLine);
        EditorGUILayout.PropertyField(_showTris);
        EditorGUILayout.PropertyField(_showVerts);
        EditorGUILayout.PropertyField(_showBounds);
        //EditorGUILayout.PropertyField(_delauney);
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        serializedObject.Update();
        ShowOtherFields();
        if (GUILayout.Button("Generate Water Mesh"))
        {
            WMG.GenerateWaterMesh();
        }
        _reorderableList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }

    private Vector3 GetCentreDir(Vector3 dirA, Vector3 dirB)
    {
        if(dirA.normalized == -dirB.normalized)
            return Vector3.forward;
        else
            return (dirA.normalized + dirB.normalized)/2;
    }

    private Vector3 TrackBezierPoint(Vector3 point)
    {
        Handles.Button(point, Quaternion.identity, _size, _size, Handles.CubeHandleCap);
        //Handles.DrawWireCube(point, _shape * _size);
        Vector3 newPos = Handles.PositionHandle(point, Quaternion.identity);
        return newPos;
    }

    private void TrackControlPoint(ControlPoint point)
    {
        // track centre
        Handles.Button(point.Centre, Quaternion.identity, _size, _size, Handles.CubeHandleCap);
        Vector3 diff = Handles.PositionHandle(point.Centre, Quaternion.identity) - point.Centre;
        point.Centre += diff;

        // track A
        Handles.Button(point.A, Quaternion.identity, _size, _size, Handles.CubeHandleCap);
        point.A = Handles.PositionHandle(point.A, Quaternion.identity);
        point.B = point.Centre + (point.Centre - point.A);
        point.A += diff;

        // track B
        Handles.DrawWireCube(point.B, _shape * _size);
        //Handles.Button(point.B, Quaternion.identity, _size, _size, Handles.CubeHandleCap);
        //point.B = Handles.PositionHandle(point.B, Quaternion.identity);
        point.B += diff;

        // DrawLines
        Handles.DrawLine(point.Centre, point.A);
        Handles.DrawLine(point.Centre, point.B);
    }
}
#endif

//[System.Serializable]
//public class SimpleTransform
//{
//    public Vector3 Position;
//    public Quaternion Rotation;
//    public bool PosHandle;
//    public bool RotHandle;

//    SimpleTransform(Vector3 position, Quaternion rotation)
//    {
//        Position = position;
//        Rotation = rotation;
//    }
//}

public class BoundingBox
{
    public Vector3 min;
    public Vector3 max;

    public BoundingBox(Vector3 minn, Vector3 maxx)
    {
        min = minn;
        max = maxx;
    }

    public Vector3 CalculateCentre()
    {
        return new Vector3((min.x + max.x) / 2, (min.y + max.y) / 2, (min.z + max.z) / 2);
    }

    public static BoundingBox CompareBoundingBoxAndSetExtreme(BoundingBox extreme, BoundingBox compare)
    {
        if (compare.min.x < extreme.min.x) extreme.min.x = compare.min.x;
        if (compare.min.y < extreme.min.y) extreme.min.y = compare.min.y;
        if (compare.min.z < extreme.min.z) extreme.min.z = compare.min.z;

        if (compare.max.x > extreme.max.x) extreme.max.x = compare.max.x;
        if (compare.max.y > extreme.max.y) extreme.max.y = compare.max.y;
        if (compare.max.z > extreme.max.z) extreme.max.z = compare.max.z;
        return extreme;
    }
}