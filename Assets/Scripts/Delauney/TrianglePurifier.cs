using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TrianglePurifier/*: MonoBehaviour*/
{
    IEnumerable<Vector3> Boundary;
    public List<Vector3> Verts;
    List<Vector3> VertsMain;
    public List<int> Triangles;
    public List<Vector3> Intersections;
    List<int> badVerts;
    List<int> goodVerts;
    public int i = 0;

    //public TrianglePurifier(IEnumerable<Vector3> boundary, List<int>triangles, List<Vector3> verts)
    //{
    //    Boundary = boundary;
    //    Verts = verts;
    //    Triangles = triangles;
    //    Intersections = new List<Vector3>();
    //}

    public void SetTrianglePurifier(IEnumerable<Vector3> boundary, List<int> triangles, List<Vector3> verts)
    {
        Boundary = boundary;
        VertsMain = new List<Vector3>();
        VertsMain.AddRange(verts);
        Verts = verts;
        Triangles = triangles;
        Intersections = new List<Vector3>();
        i = Triangles.Count - 3;
        badVerts = new List<int>();
        goodVerts = new List<int>();
    }

    private void Start()
    {
        
    }

    public List<int> PurifyTriangles()
    {
        PurifyNextXTriangles(Triangles.Count/3);
        return Triangles;
    }

    public void PurifyNextXTriangles(int no)
    {
        for (int j = 0; j < no; j++)
        {
            PurifyNextTriangle();
            i -= 3;
        }
    }

    public void PurifyNextTriangle()
    {
        if (i >= 0)
        {
            // clear verts
            badVerts.Clear();
            goodVerts.Clear();

            // put three verts into bad triangle
            badVerts.Add(Triangles[i]); badVerts.Add(Triangles[i + 1]); badVerts.Add(Triangles[i + 2]);

            // check vertices in bad triangle if they are good, and move good ones
            for (int j = 2; j >= 0; j--)
            {
                if (WindingCountBoundaryTest.Check3d(Verts.ElementAt(badVerts[j]), Boundary))
                {
                    goodVerts.Add(badVerts[j]);
                    badVerts.RemoveAt(j);
                }
            }

            // if bad verts == 3, delete triangle
            if (badVerts.Count == 3)
            {
                Triangles.RemoveAt(i + 2); Triangles.RemoveAt(i + 1); Triangles.RemoveAt(i);
            }

            // if good verts == 1, check intersections and move bad verts
            else if (goodVerts.Count == 1)
            {
                // inside vert is on the boundary with other verts outside, hence delete
                if (Verts[goodVerts[0]] != VertsMain[goodVerts[0]])
                {
                    Triangles.RemoveAt(i + 2); Triangles.RemoveAt(i + 1); Triangles.RemoveAt(i);
                    return;
                }
                Vector3 intersection = VectorIntersection.BoundarySingleIntersectionCheckNew(Boundary, Verts[goodVerts[0]], Verts[goodVerts[0]] - Verts[badVerts[0]]);
                Verts[badVerts[0]] = new Vector3(intersection.x, Verts[badVerts[0]].y, intersection.z);
                Intersections.Add(intersection);

                Vector3 intersextion = VectorIntersection.BoundarySingleIntersectionCheckNew(Boundary, Verts[goodVerts[0]], Verts[goodVerts[0]] - Verts[badVerts[1]]);
                Verts[badVerts[1]] = new Vector3(intersextion.x, Verts[badVerts[1]].y, intersextion.z);
                Intersections.Add(intersextion);
            }

            // if good verts == 2, move bad vert to one intersection
            else if (goodVerts.Count == 2)
            {
                Vector3 posA = Vector3.zero;
                // check if verts has been moved before
                if (Verts[goodVerts[0]] != VertsMain[goodVerts[0]] && Verts[goodVerts[1]] != VertsMain[goodVerts[1]]) 
                {
                    Triangles.RemoveAt(i + 2); Triangles.RemoveAt(i + 1); Triangles.RemoveAt(i);
                    return; 
                }
                else if (Verts[goodVerts[0]] == VertsMain[goodVerts[0]]) posA = new Vector3(Verts[goodVerts[0]].x, 0, Verts[goodVerts[0]].z);
                else posA = new Vector3(Verts[goodVerts[1]].x, 0, Verts[goodVerts[1]].z);

                Vector3 posB = new Vector3(Verts[badVerts[0]].x, 0, Verts[badVerts[0]].z);
                Vector3 intersection = VectorIntersection.BoundarySingleIntersectionCheckNew(Boundary, posA, (posA - posB));
                //Debug.Log("intersection: " + intersection);
                Verts[badVerts[0]] = new Vector3(intersection.x, Verts[badVerts[0]].y, intersection.z);
                //Intersections.Add(intersection);
            }
        }
    }


    //public List<int> PurifyTriangles()
    //{
    //    List<int> badVerts =  new List<int>();
    //    List<int> goodVerts = new List<int>();

    //    for(int i = Triangles.Count - 3; i >= 0; i -= 3)
    //    {
    //        // clear verts
    //        badVerts.Clear();
    //        goodVerts.Clear();

    //        // put three verts into bad triangle
    //        badVerts.Add(Triangles[i]); badVerts.Add(Triangles[i + 1]); badVerts.Add(Triangles[i + 2]);

    //        // check vertices in bad triangle if they are good, and move good ones
    //        for(int j = 2; j >= 0 ; j--)
    //        {
    //            if (WindingCountBoundaryTest.Check3d(Verts.ElementAt(badVerts[j]), Boundary))
    //            {
    //                goodVerts.Add(badVerts[j]);
    //                badVerts.RemoveAt(j);
    //            }
    //        }

    //        // if good verts == 3, keep triangle and skip to the next triangle
    //        if (goodVerts.Count == 3) continue;

    //        // if bad verts == 3, delete triangle
    //        else if (badVerts.Count == 3)
    //        {
    //            Triangles.RemoveAt(i + 2); Triangles.RemoveAt(i + 1); Triangles.RemoveAt(i);
    //        }

    //        //// if good verts == 1, check intersections and move bad verts
    //        //else if (goodVerts.Count == 1)
    //        //{
    //        //    Debug.Log("good ind: " + goodVerts[0]);
    //        //    Vector3 intersection = VectorIntersection.BoundarySingleIntersectionCheckNew(Boundary, Verts[goodVerts[0]], Verts[goodVerts[0]] - Verts[badVerts[0]]);
    //        //    Verts[badVerts[0]] = new Vector3(intersection.x, Verts[badVerts[0]].y, intersection.z);

    //        //    Vector3 intersextion = VectorIntersection.BoundarySingleIntersectionCheckNew(Boundary, Verts[goodVerts[0]], Verts[goodVerts[0]] - Verts[badVerts[1]]);
    //        //    Verts[badVerts[1]] = new Vector3(intersextion.x, Verts[badVerts[1]].y, intersextion.z);
    //        //}

    //        // if good verts == 2, move bad vert to one intersection
    //        else if (goodVerts.Count == 2)
    //        {
    //            Vector3 posA = new Vector3(Verts[goodVerts[0]].x, 0, Verts[goodVerts[0]].z);
    //            Vector3 posB = new Vector3(Verts[badVerts[0]].x, 0, Verts[badVerts[0]].z);
    //            Vector3 intersection = VectorIntersection.BoundarySingleIntersectionCheckNew(Boundary, posA, (posA-posB));
    //            //Debug.Log("intersection: " + intersection);
    //            Verts[badVerts[0]] = new Vector3(intersection.x, Verts[badVerts[0]].y, intersection.z);
    //            Intersections.Add(intersection);
    //        }
    //    }

    //    return Triangles;
    //}
}

//#if (UNITY_EDITOR)

//[CustomEditor(typeof(TrianglePurifier))]
//public class TrianglePurifierEditor : UnityEditor.Editor
//{
//    public override void OnInspectorGUI()
//    {
//        TrianglePurifier purEdit = (TrianglePurifier)target;
//        //if (DrawDefaultInspector())
//        //{
//        //    if (meshGen.AutoUpdate) meshGen.GenerateMesh();
//        //}
//        DrawDefaultInspector();

//        if (GUILayout.Button("next"))
//        {
//            purEdit.PurifyNextXTriangles(1);
//        }

//        if (GUILayout.Button("next 10"))
//        {
//            purEdit.PurifyNextXTriangles(10);
//        }
//    }
//}
//#endif
