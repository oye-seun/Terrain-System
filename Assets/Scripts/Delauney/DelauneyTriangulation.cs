using System;
using System.Collections.Generic;
using UnityEngine;

public class DelauneyTriangulation
{
    List<Triangle> goodTriangles = new List<Triangle> ();
    List<Triangle> badTriangles = new List<Triangle> ();
    List<Vector3> verts;
    Triangle SuperTriangle;

    public DelauneyTriangulation(List<Vector3> vert, Triangle superTriangle)
    {
        SuperTriangle = new Triangle(
            new Vector3(superTriangle.A.x, 0, superTriangle.A.z),
            new Vector3(superTriangle.B.x, 0, superTriangle.B.z),
            new Vector3(superTriangle.C.x, 0, superTriangle.C.z)
            );
        goodTriangles.Add(SuperTriangle);
        verts = ProjectOnPlane(vert);
    }

    public List<int> Triangulate(bool up)
    {
        foreach(Vector3 vert in verts)
        {
            // select the bad triangles
            for(int i = 0; i < goodTriangles.Count; i++)
            {
                //Debug.Log("circumcentre: " + goodTriangles[i].Circumcentre + " circumradius: " + goodTriangles[i].Circumradius + " A: " + goodTriangles[i].A + " B: " + goodTriangles[i].B + " C: " + goodTriangles[i].C);
                if (Vector3.Distance(vert, goodTriangles[i].Circumcentre) < goodTriangles[i].Circumradius)
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
                if ((vert - uniqueEdges[i].B).normalized != (uniqueEdges[i].A - uniqueEdges[i].B).normalized)
                    goodTriangles.Add(new Triangle(vert, uniqueEdges[i].A, uniqueEdges[i].B));

                //if ((vert - uniqueEdges[i].B).normalized != (uniqueEdges[i].A - uniqueEdges[i].B).normalized)
                //{
                //    Triangle newTris = new Triangle(vert, uniqueEdges[i].A, uniqueEdges[i].B);
                //    goodTriangles.Add(newTris);
                //    Debug.Log("centre: " + newTris.Circumcentre + " radius: " + newTris.Circumradius);
                //    //Debug.Log("vert: " + vert + " uniqueEdges[i].A: " + uniqueEdges[i].A + " uniqueEdges[i].B: " + uniqueEdges[i].B);
                //}
                //else
                //    Debug.Log("tried to add Unilinear triangle");
            }

            // clear bad triangle list
            badTriangles.Clear();
        }

        // make sure all face normals are pointing in one direction
        for(int i = goodTriangles.Count - 1; i >= 0; i--)
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

        return triangles;
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


public class Edge
{
    public Vector3 A;
    public Vector3 B;

    public Edge(Vector3 a, Vector3 b)
    {
        A = a;
        B = b;
    }

    public bool Compare(Edge edge)
    {
        if (A == edge.A && B == edge.B)
            return true;
        else if (A == edge.B && B == edge.A)
            return true;
        else 
            return false;
    }
}

public class Triangle
{
    public Vector3 A;
    public Vector3 B;
    public Vector3 C;
    public Vector3 Circumcentre = Vector3.zero;
    public float Circumradius = 0;


    public Triangle() { }

    public Triangle(Vector3 a, Vector3 b, Vector3 c)
    {
        this.A = a; this.B = b; this.C = c;
        Circumcentre = CircumCentre();
        Circumradius = CircumRadius();
    }

    public float CircumRadius()
    {
        return Vector3.Distance(Circumcentre, A);
    }

    public Vector3 CircumCentre()
    { 
        if(A.y == B.y && A.y == C.y)
        {
            Vector2 centre2D = CircumCentre2D(new Vector2(A.x, A.z), new Vector2(B.x, B.z), new Vector2(C.x, C.z));
            return new Vector3(centre2D.x, A.y, centre2D.y);
        }
        else if(A.x == B.x && A.x == C.x)
        {
            Vector2 centre2D = CircumCentre2D(new Vector2(A.y, A.z), new Vector2(B.y, B.z), new Vector2(C.y, C.z));
            return new Vector3(A.x, centre2D.x, centre2D.y);
        }
        else if(A.z == B.z && A.z == C.z)
        {
            Vector2 centre2D = CircumCentre2D(new Vector2(A.x, A.y), new Vector2(B.x, B.y), new Vector2(C.x, C.y));
            return new Vector3(centre2D.x, centre2D.y, A.z);
        }

        Vector3 centre = Vector3.zero;
        float O = ((A.y - B.y) * (A.z - C.z)) - ((A.y - C.y) * (A.z - B.z));
        float N = ((A.x - C.x) * (A.z - B.z)) - ((A.x - B.x) * (A.z - C.z));
        float M = ((A.x - B.x) * (A.y - C.y)) - ((A.x - C.x) * (A.y - B.y));
        float L = (O * (B.z - A.z)) - (M * (B.x - A.x));
        float K = (N * (B.z - A.z)) - (M * (B.y - A.y));
        float J = (O * (C.z - A.z)) - (M * (C.x - A.x));
        float I = (N * (C.z - A.z)) - (M * (C.y - A.y));
        float H = 2 * (B.z - A.z) * ((A.x * O) + (A.y * N) + (A.z * M));
        float G = M * ((B.x * B.x) - (A.x * A.x) + (B.y * B.y) - (A.y * A.y) + (B.z * B.z) - (A.z * A.z));
        float F = 2 * (C.z - A.z) * ((A.x * O) + (A.y * N) + (A.z * M));
        float E = M * ((C.x * C.x) - (A.x * A.x) + (C.y * C.y) - (A.y * A.y) + (C.z * C.z) - (A.z * A.z));

        //Debug.Log($"I: {I} J: {J} K: {K} L: {L}");

        centre.x = (J == 0 && L == 0) ? 0 : ((I * (H - G)) - (K * (F - E))) / (2 * ((L * I) - (J * K)));
        centre.y = (H - G - (2 * centre.x * L)) / (2 * K);
        centre.z = (M == 0)? 0 : ((A.x * O) + (A.y * N) + (A.z * M) - (centre.x * O) - (centre.y * N)) / M;
        return centre;
    }

    public Vector2 CircumCentre2D(Vector2 A, Vector2 B, Vector2 C)
    {
        Vector2 centre = new Vector2();
        float O = (C.y - A.y) * ((A.x * A.x) + (A.y * A.y) - (B.x * B.x) - (B.y * B.y));
        //Debug.Log("O: " + O);
        float N = (A.y - B.y) * ((C.x * C.x) + (C.y * C.y) - (A.x * A.x) - (A.y * A.y));
        //Debug.Log("N: " + N);
        float M = (2 * (A.x - B.x) * (C.y - A.y)) - (2 * (C.x - A.x) * (A.y - B.y));
        //Debug.Log("M: " + M);

        centre.x = (O - N) / M;
        if(A.y - B.y != 0)
            centre.y = ((A.x * A.x) + (A.y * A.y) - (B.x * B.x) - (B.y * B.y) - (2 * centre.x * (A.x - B.x))) / (2 * (A.y - B.y));
        else
            centre.y = ((C.x * C.x) + (C.y * C.y) - (A.x * A.x) - (A.y * A.y) - (2 * centre.x * (C.x - A.x))) / (2 * (C.y - A.y));
        return centre;
    }
}