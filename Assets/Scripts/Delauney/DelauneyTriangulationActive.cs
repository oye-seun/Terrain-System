using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class DelauneyTriangulationActive : MonoBehaviour
{
    List<Triangle> goodTriangles = new List<Triangle> ();
    List<Triangle> badTriangles = new List<Triangle> ();
    List<Vector3> verts;
    Triangle SuperTriangle;
    public List<int> Triangles;

    public bool up;
    public bool ShowTris;
    public int progress;
    

    //public DelauneyTriangulationActive(List<Vector3> vert, Triangle superTriangle)
    //{ 
    //    SuperTriangle = superTriangle;
    //    goodTriangles.Add(superTriangle);
    //    verts = ProjectOnPlane(vert);
    //}

    public void SetDelauneyProperties(List<Vector3> vert, Triangle superTriangle)
    {
        SuperTriangle = new Triangle(
            new Vector3(superTriangle.A.x, 0, superTriangle.A.z),
            new Vector3(superTriangle.B.x, 0, superTriangle.B.z),
            new Vector3(superTriangle.C.x, 0, superTriangle.C.z)
            );
        goodTriangles.Clear ();
        goodTriangles.Add(SuperTriangle);
        verts = ProjectOnPlane(vert);
        progress = 0;
    }

    private void Update()
    {
        if (Inputs.GetKeyDown(KeyCode.N))
        {
            ProceduralTriangulate();
        }
    }

    private void OnDrawGizmos()
    {
        if (ShowTris)
        {
            foreach (Triangle triangle in goodTriangles)
            {
                Debug.DrawLine(triangle.A, triangle.B, Color.cyan);
                Debug.DrawLine(triangle.B, triangle.C, Color.cyan);
                Debug.DrawLine(triangle.C, triangle.A, Color.cyan);

                DrawCircle(triangle, 30);
            }
        }
    }

    private void DrawCircle(Triangle triangle, int sides)
    {
        for(int i = 0; i < sides; i++)
        {
            float angle = ((float)i / (float)sides) * 360f;
            float angleplus = (((float)i + 1 )/ (float)sides) * 360f;

            Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
            Vector3 dir2 = new Vector3(Mathf.Cos(angleplus * Mathf.Deg2Rad), 0, Mathf.Sin(angleplus * Mathf.Deg2Rad)).normalized;

            Debug.DrawLine(triangle.Circumcentre + (dir * triangle.Circumradius), triangle.Circumcentre + (dir2 * triangle.Circumradius), Color.yellow);
        }
    }

    public void ProceduralTriangulate()
    {
        Triangulate(progress);
    }

    public void Triangulate(int index)
    {
        if(index < verts.Count) { 
            // select the bad triangles
            for(int i = 0; i < goodTriangles.Count; i++)
            {
                //Debug.Log("circumcentre: " + goodTriangles[i].Circumcentre + " circumradius: " + goodTriangles[i].Circumradius + " A: " + goodTriangles[i].A + " B: " + goodTriangles[i].B + " C: " + goodTriangles[i].C);
                if (Vector3.Distance(verts[index], goodTriangles[i].Circumcentre) < goodTriangles[i].Circumradius)
                {
                    badTriangles.Add(goodTriangles[i]);
                }
            }

            // remove bad triangles
            for(int i = 0; i < badTriangles.Count; i++)
            {
                goodTriangles.Remove(badTriangles[i]);
            }

            // add new triangles
            List<Edge> uniqueEdges = UniqueEdges(badTriangles);
            for(int i = 0; i < uniqueEdges.Count; i++)
            {
                if ((verts[index] - uniqueEdges[i].B).normalized != (uniqueEdges[i].A - uniqueEdges[i].B).normalized)
                {
                    Triangle newTris = new Triangle(verts[index], uniqueEdges[i].A, uniqueEdges[i].B);
                    goodTriangles.Add(newTris);
                    //Debug.Log("centre: " + newTris.Circumcentre + " radius: " + newTris.Circumradius);
                    Debug.Log("vert: " + verts[index] + " uniqueEdges[i].A: " + uniqueEdges[i].A + " uniqueEdges[i].B: " + uniqueEdges[i].B);
                }
                else
                    Debug.Log("tried to add Unilinear triangle");
            }

            // clear bad triangle list
            badTriangles.Clear();
            progress++;
        }

        if (index >= verts.Count)
        {
            // make sure all face normals are pointing in one direction
            for (int i = goodTriangles.Count - 1; i >= 0; i--)
            {
                goodTriangles[i] = TestTrisNormalAndSwap(goodTriangles[i], up);

                // remove triangles touching the super triangle
                if (OverlappingTriangles(goodTriangles[i], SuperTriangle))
                {
                    //Debug.Log("removed super related tris");
                    goodTriangles.RemoveAt(i);
                }
            }

            List<int> triangles = new List<int>();
            triangles = TranslateToInt(verts, goodTriangles);
            Triangles = triangles;
            progress = 0;
        }
    }

    private bool OverlappingTriangles(Triangle A, Triangle B)
    {
        bool hasDupli = false;
        //Edge[] AEdge = new Edge[3];
        //AEdge[0] = new Edge(A.A, A.B); /**/  AEdge[1] = new Edge(A.B, A.C);  /**/  AEdge[2] = new Edge(A.C, A.A);

        //Edge[] BEdge = new Edge[3];
        //BEdge[0] = new Edge(B.A, B.B); /**/  BEdge[1] = new Edge(B.B, B.C);  /**/  BEdge[2] = new Edge(B.C, B.A);

        //for(int i = 0; i < 3; i++)
        //{
        //    for(int j = 0; j < 3; j++)
        //    {
        //        hasDupli = AEdge[i].Compare(BEdge[j]);
        //    }
        //}

        if (A.A == B.A || A.A == B.B || A.A == B.C)
            hasDupli = true;
        else if (A.B == B.A || A.B == B.B || A.B == B.C)
            hasDupli = true;
        else if (A.C == B.A || A.C == B.B || A.C == B.C)
            hasDupli = true;
        return hasDupli;
    }

    private List<int> TranslateToInt(List<Vector3> verts, List<Triangle> triangles)
    {
        List<int> result = new List<int>();
        for (int i = 0; i < triangles.Count; i++)
        {
            //result.Add(verts.IndexOf(triangles[i].A)); result.Add(verts.IndexOf(triangles[i].B)); result.Add(verts.IndexOf(triangles[i].C));
            result.Add(SearchIndex(verts, triangles[i].A)); result.Add(SearchIndex(verts, triangles[i].B)); result.Add(SearchIndex(verts, triangles[i].C));
        }
        return result;
    }

    private int SearchIndex(List<Vector3> vert, Vector3 vector)
    {
        int result = -1;
        //Debug.Log("searched vec: " + vector);
        for(int i = 0; i < vert.Count; i++)
        {
            if(vert[i] == vector)
                result = i;
        }
        return result;
    }

    private List<Vector3> ProjectOnPlane(List<Vector3> verts)
    {
        List<Vector3> result = new List<Vector3>();
        for(int i = 0; i < verts.Count; i++)
        {
            result.Add(new Vector3(verts[i].x, 0, verts[i].z));
        }
        return result;
    }

    private List<Edge> UniqueEdges(List<Triangle> triangles)
    {
        List<Edge> edges = new List<Edge>();
        foreach(Triangle tris in triangles)
        {
            edges.Add(new Edge(tris.A, tris.B)); edges.Add(new Edge(tris.B, tris.C)); edges.Add(new Edge(tris.C, tris.A));
        }

        List<Edge> sortedEdges = new List<Edge>();
        for(int i = 0; i < edges.Count; i++)
        {
            bool hasDuplicate = false;
            for(int j = 0; j < edges.Count; j++)
            {
                if(edges[i].Compare(edges[j]) && j != i)
                {
                    hasDuplicate = true;
                }
            }
            if (!hasDuplicate)
            {
                sortedEdges.Add(edges[i]);
            }
        }
        return sortedEdges;
    }

    private Triangle TestTrisNormalAndSwap(Triangle triangle, bool up)
    {
        Triangle result = new Triangle();
        Vector3 upDir = (up) ? Vector3.up : Vector3.down;
        Vector3 dir1 = (triangle.A - triangle.B).normalized;
        Vector3 dir2 = (triangle.B - triangle.C).normalized;
        Vector3 normal = Vector3.Cross(dir1, dir2).normalized;
        if (Vector3.Dot(normal, upDir) >= 0.99f) // right Dir
        {
            result.A = triangle.A;  result.B = triangle.B; result.C = triangle.C;
        }
        else
        {
            result.A = triangle.A; result.B = triangle.C; result.C = triangle.B;
        }
        result.Circumradius = triangle.Circumradius;
        result.Circumcentre = triangle.Circumcentre;
        return result;
    }
}


#if (UNITY_EDITOR)
[CustomEditor(typeof(DelauneyTriangulationActive))]
public class DelauneyTriangulationActiveEditor : UnityEditor.Editor
{
    DelauneyTriangulationActive delauney;
    bool keyPressed;
    private void OnEnable()
    {
        delauney = (DelauneyTriangulationActive)target;
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
            delauney.ProceduralTriangulate();
            //EditorUtility.SetDirty(dollyLookCamUpdater);
        }
    }
}
#endif