using System.Collections.Generic;
using UnityEngine;

public class UVGenerator
{
    List<int> triangles;
    List<Vector3> verts;
    Vector2[] uv;
    int maxcount;

    public UVGenerator(List<int> triangle, List<Vector3> vert, int maxcount)
    {
        triangles = new List<int>();
        triangles.AddRange(triangle);
        verts = new List<Vector3>();
        verts.AddRange(vert);
        uv = new Vector2[vert.Count];
        this.maxcount = maxcount;
    }

    public Vector2[] GenerateUV()
    {
        // add base triangle
        int[] baseTriangle = ReturnAttachedTriangle(0);
        if (baseTriangle[0] == -1) return uv;

        Vector3[] baseTrisXY = AlignToXY(verts[baseTriangle[0]], verts[baseTriangle[1]], verts[baseTriangle[2]]);
        //baseTrisXY = Rotate(baseTrisXY, 30);
        uv[baseTriangle[0]] = new Vector2(baseTrisXY[0].x, baseTrisXY[0].z);
        uv[baseTriangle[1]] = new Vector2(baseTrisXY[1].x, baseTrisXY[1].z);
        uv[baseTriangle[2]] = new Vector2(baseTrisXY[2].x, baseTrisXY[2].z);

        // for testing only
        //uv[0] = new Vector2(baseTrisXY[0].x, baseTrisXY[0].z);
        //uv[1] = new Vector2(baseTrisXY[1].x, baseTrisXY[1].z);
        //uv[2] = new Vector2(baseTrisXY[2].x, baseTrisXY[2].z);


        // call recursive loop to generate remaining triangles
        CompleteTriangle(baseTriangle[0], baseTriangle[1]);
        CompleteTriangle(baseTriangle[1], baseTriangle[2]);
        CompleteTriangle(baseTriangle[2], baseTriangle[0]);

        return uv;
    }
    

    private void CompleteTriangle(int A, int B)
    {
        // get the last point of the triangle
        int C = ReturnLastTris(A, B);

        // ex
        if (C == -1) return;

        // get the vertices and transform them to xy plane
        Vector3[] xyTris = AlignToXY(verts[A], verts[B], verts[C]);

        // rotate tris to match AB
        Vector2 disp1 = (uv[A] - uv[B]).normalized; Vector2 disp2 = new Vector2((xyTris[0] - xyTris[1]).x, (xyTris[0] - xyTris[1]).z).normalized;
        float angleDiff = Vector2.SignedAngle(disp1, disp2);
        //angleDiff = (disp2.y < disp1.y)? -angleDiff : angleDiff;
        //Debug.Log("anglediff: " + angleDiff);
        xyTris = Rotate(xyTris, -angleDiff);

        // Assign C to the UV map
        Vector3 Cdisp = xyTris[2] - xyTris[0];
        uv[C] = uv[A] + new Vector2(Cdisp.x, Cdisp.z);

        // for testing only
        //uv[3] = new Vector2(xyTris[0].x, xyTris[0].z);
        //uv[4] = new Vector2(xyTris[1].x, xyTris[1].z);
        //uv[5] = new Vector2(xyTris[2].x, xyTris[2].z);

        // recursive call for all the edges
        CompleteTriangle(A, C);
        CompleteTriangle(B, C);
    }


    List<int> Edges;
    public Vector2[] GenerateUV2()
    {
        // add base triangle
        int[] baseTriangle = ReturnAttachedTriangle(0);
        if (baseTriangle[0] == -1) return uv;

        Vector3[] baseTrisXY = AlignToXY(verts[baseTriangle[0]], verts[baseTriangle[1]], verts[baseTriangle[2]]);
        //baseTrisXY = Rotate(baseTrisXY, 30);
        uv[baseTriangle[0]] = new Vector2(baseTrisXY[0].x, baseTrisXY[0].z);
        uv[baseTriangle[1]] = new Vector2(baseTrisXY[1].x, baseTrisXY[1].z);
        uv[baseTriangle[2]] = new Vector2(baseTrisXY[2].x, baseTrisXY[2].z);

        Debug.Log($"base tris: {baseTriangle[0]}, {baseTriangle[1]}, {baseTriangle[2]}");

        // for testing only
        //uv[0] = new Vector2(baseTrisXY[0].x, baseTrisXY[0].z);
        //uv[1] = new Vector2(baseTrisXY[1].x, baseTrisXY[1].z);
        //uv[2] = new Vector2(baseTrisXY[2].x, baseTrisXY[2].z);

        // list of available edges
        Edges = new List<int>();
        Edges.Add(baseTriangle[0]); Edges.Add(baseTriangle[1]); Edges.Add(baseTriangle[2]); Edges.Add(baseTriangle[0]);
        int A; int B; int C; int count; int counter = 0;
        while(Edges.Count > 1 && counter < maxcount)
        {
            count = Edges.Count;
            A = Edges[count - 2];
            B = Edges[count - 1];
            C = CompleteTriangleNonRec(A, B);
            //Debug.Log("C: " + C);
            if( C == -1)
            {
                Edges.RemoveAt(count - 1);  // remove last vert
            }
            else
            {
                Edges.RemoveAt(count - 1); Edges.RemoveAt(count - 2);
                Edges.Add(A); Edges.Add(C); Edges.Add(B);
            }

            if (count > 3)
            {
                // second labourer
                int M = Edges[0];
                int N = Edges[1];
                int O = CompleteTriangleNonRec(M, N);
                //Debug.Log("C: " + C);
                if (O == -1)
                {
                    Edges.RemoveAt(0);  // remove last vert
                }
                else
                {
                    Edges.RemoveAt(1); Edges.RemoveAt(0);
                    Edges.Insert(0, N); Edges.Insert(0, O); Edges.Insert(0, M);
                }
            }

            //string E = Edges[0].ToString();
            //for (int i = 1; i < Edges.Count; i++)
            //{
            //    E += ", ";
            //    E += Edges[i].ToString();
            //}
            //Debug.Log("edges: " + E);
            counter++;
        }


        // call recursive loop to generate remaining triangles
        //CompleteTriangleNonRec(baseTriangle[0], baseTriangle[1]);
        //CompleteTriangleNonRec(baseTriangle[1], baseTriangle[2]);
        //CompleteTriangleNonRec(baseTriangle[2], baseTriangle[0]);

        return uv;
    }


    private int CompleteTriangleNonRec(int A, int B)
    {
        // get the last point of the triangle
        int C = ReturnLastTris(A, B);

        // exit
        if (Edges.Contains(C)) return C;
        else if (C == -1) return -1;

        // get the vertices and transform them to xy plane
        Vector3[] xyTris = AlignToXY(verts[A], verts[B], verts[C]);

        // rotate tris to match AB
        Vector2 disp1 = (uv[A] - uv[B]).normalized; Vector2 disp2 = new Vector2((xyTris[0] - xyTris[1]).x, (xyTris[0] - xyTris[1]).z).normalized;
        float angleDiff = Vector2.SignedAngle(disp1, disp2);
        xyTris = Rotate(xyTris, -angleDiff);

        // Assign C to the UV map
        Vector3 Cdisp = xyTris[2] - xyTris[0];
        uv[C] = uv[A] + new Vector2(Cdisp.x, Cdisp.z);
        return C;
    }


    public Vector2[] GenerateUV3()
    {
        // add base triangle
        int[] baseTriangle = ReturnAttachedTriangle(0);
        if (baseTriangle[0] == -1) return uv;

        Vector3[] baseTrisXY = AlignToXY(verts[baseTriangle[0]], verts[baseTriangle[1]], verts[baseTriangle[2]]);
        //baseTrisXY = Rotate(baseTrisXY, 30);
        uv[baseTriangle[0]] = new Vector2(baseTrisXY[0].x, baseTrisXY[0].z);
        uv[baseTriangle[1]] = new Vector2(baseTrisXY[1].x, baseTrisXY[1].z);
        uv[baseTriangle[2]] = new Vector2(baseTrisXY[2].x, baseTrisXY[2].z);

        Debug.Log($"base tris: {baseTriangle[0]}, {baseTriangle[1]}, {baseTriangle[2]}");

        // for testing only
        //uv[0] = new Vector2(baseTrisXY[0].x, baseTrisXY[0].z);
        //uv[1] = new Vector2(baseTrisXY[1].x, baseTrisXY[1].z);
        //uv[2] = new Vector2(baseTrisXY[2].x, baseTrisXY[2].z);

        // list of available edges
        List<int> todoEdges = new List<int>();
        List<int> ondoEdges = new List<int>();
        ondoEdges.Add(baseTriangle[0]); ondoEdges.Add(baseTriangle[1]); 
        ondoEdges.Add(baseTriangle[1]); ondoEdges.Add(baseTriangle[2]);
        ondoEdges.Add(baseTriangle[2]); ondoEdges.Add(baseTriangle[0]);

        int A; int B; int C;
        while (ondoEdges.Count > 1)
        {
            for(int i = ondoEdges.Count - 1; i >= 0; i -= 2)
            {
                A = ondoEdges[i - 1];
                B = ondoEdges[i];
                C = CompleteTriangle3(A, B);
                //Debug.Log("C: " + C);
                if (C == -1)
                {
                    ondoEdges.RemoveAt(i); ondoEdges.RemoveAt(i - 1); // remove last vert
                }
                else
                {
                    ondoEdges.RemoveAt(i); ondoEdges.RemoveAt(i - 1);
                    todoEdges.Add(A); todoEdges.Add(C); todoEdges.Add(B); todoEdges.Add(C);
                }
            }

            ondoEdges.Clear();
            ondoEdges.AddRange(todoEdges);
            todoEdges.Clear();
        }

        return uv;
    }

    private int CompleteTriangle3(int A, int B)
    {
        // get the last point of the triangle
        int C = ReturnLastTris(A, B);

        // exit
        if (C == -1) return -1;

        // get the vertices and transform them to xy plane
        Vector3[] xyTris = AlignToXY(verts[A], verts[B], verts[C]);

        // rotate tris to match AB
        Vector2 disp1 = (uv[A] - uv[B]).normalized; Vector2 disp2 = new Vector2((xyTris[0] - xyTris[1]).x, (xyTris[0] - xyTris[1]).z).normalized;
        float angleDiff = Vector2.SignedAngle(disp1, disp2);
        xyTris = Rotate(xyTris, -angleDiff);

        // Assign C to the UV map
        Vector3 Cdisp = xyTris[2] - xyTris[0];
        uv[C] = uv[A] + new Vector2(Cdisp.x, Cdisp.z);
        return C;
    }


    public Vector2[] GenerateUV4()
    {
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);
        for(int i = 0; i < verts.Count; i++)
        {
            uv[i] = new Vector2(verts[i].x, verts[i].z);
            if (uv[i].x < min.x) min.x = uv[i].x;
            if (uv[i].y < min.y) min.y = uv[i].y;
            if (uv[i].x > max.x) max.x = uv[i].x;
            if (uv[i].y > max.y) max.y = uv[i].y;
        }

        // set UV to 0 to 1
        for(int i = 0; i < uv.Length; i++)
        {
            uv[i].x = Mathf.InverseLerp(min.x, max.x, uv[i].x);
            uv[i].y = Mathf.InverseLerp(min.y, max.y, uv[i].y);
        }
        

        return uv;
    }


    private int[] ReturnAttachedTriangle(int index)
    {
        int[] tris = new int[3] { -1, -1, -1 };
        for (int i = triangles.Count - 3; i >= 0; i -= 3)
        {
            if (triangles[i] == index || triangles[i + 1] == index || triangles[i + 2] == index)
            {
                tris[0] = triangles[i]; tris[1] = triangles[i + 1]; tris[2] = triangles[i + 2];
                triangles.RemoveAt(i + 2); triangles.RemoveAt(i + 1); triangles.RemoveAt(i);
                return tris;
            }
        }
        return tris;
    }

    private int ReturnLastTris(int index, int index2)
    {
        int tris = -1;
        void RemoveTriangle(int i)
        {
            triangles.RemoveAt(i + 2); triangles.RemoveAt(i + 1); triangles.RemoveAt(i);
            //Debug.Log("triangleCount: " + triangles.Count);
        }
        for (int i = triangles.Count - 3; i >= 0; i -= 3)
        {
            if ((triangles[i] == index && triangles[i + 1] == index2) || (triangles[i] == index2 && triangles[i + 1] == index))
            {
                tris = triangles[i + 2];
                RemoveTriangle(i);
                return tris;
            }
            else if ((triangles[i + 1] == index && triangles[i + 2] == index2) || (triangles[i + 1] == index2 && triangles[i + 2] == index))
            {
                tris = triangles[i];
                RemoveTriangle(i);
                return tris;
            }
            else if ((triangles[i + 2] == index && triangles[i] == index2) || (triangles[i + 2] == index2 && triangles[i] == index))
            {
                tris = triangles[i + 1];
                RemoveTriangle(i);
                return tris;
            }
        }
        return tris;
    }

    public Vector3[] AlignToXY(Vector3 A, Vector3 B, Vector3 C)
    {
        // move the triangle into local space
        Vector3 centre = (A + B + C) / 3;
        A -= centre; B -= centre; C -= centre;

        // declare Output
        Vector3[] vertices = new Vector3[3] { A, B, C };

        Vector3 disp1 = A - B;
        Vector3 disp2 = B - C;

        // angles first
        Vector3 normal = Vector3.Cross(disp1, disp2).normalized;
        if(normal.y < 0) normal = Vector3.Cross(disp2, disp1).normalized;
        Vector3 nprime = Vector3.Cross(new Vector3(0, 1, 0), normal).normalized;
        float o = Mathf.Acos(normal.y) * Mathf.Rad2Deg;
        float a = Mathf.Acos(nprime.x) * Mathf.Rad2Deg;
        //o = (normal.z > 0) ? o : -o;
        a = (nprime.z > 0) ? a : -a;

        //reverse flip
        //o = -o;
        a = -a;

        float coso = Mathf.Cos(o * Mathf.Deg2Rad); float cosa = Mathf.Cos(a * Mathf.Deg2Rad);
        float sino = Mathf.Sin(o * Mathf.Deg2Rad); float sina = Mathf.Sin(a * Mathf.Deg2Rad);
        //float tano = Mathf.Tan(o * Mathf.Deg2Rad); float tana = Mathf.Tan(a * Mathf.Deg2Rad);

        Matrix3x3 XrotationMat = new Matrix3x3(
            new Vector3(1, 0, 0),
            new Vector3(0, coso, sino),
            new Vector3(0, -sino, coso)
            );

        Matrix3x3 YrotationMat = new Matrix3x3(
            new Vector3(cosa, 0, -sina),
            new Vector3(0, 1, 0),
            new Vector3(sina, 0, cosa)
            );

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = YrotationMat.MATxVEC(vertices[i]);
            vertices[i] = XrotationMat.MATxVEC(vertices[i]);
        }

        return vertices;
    }


    public Vector3[] Rotate(Vector3[] verts, float turnAngle)
    {
        float cos = Mathf.Cos(turnAngle * Mathf.Deg2Rad);
        float sin = Mathf.Sin(turnAngle * Mathf.Deg2Rad);

        Matrix3x3 rotationMat = new Matrix3x3(
            new Vector3(cos, 0, -sin),
            new Vector3( 0,  1,   0 ),
            new Vector3(sin, 0,  cos)
            );

        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = rotationMat.MATxVEC(verts[i]);
        }

        return verts;
    }

    //private int[] ReturnLastTris(int index, int index2)
    //{
    //    int[] tris = new int[] { -1, -1, -1 };
    //    void RemoveTriangle(int i)
    //    {
    //        triangles.RemoveAt(i + 2); triangles.RemoveAt(i + 1); triangles.RemoveAt(i);
    //    }
    //    for (int i = triangles.Count - 3; i >= 0; i -= 3)
    //    {
    //        if ((triangles[i] == index && triangles[i + 1] == index2) || (triangles[i] == index2 && triangles[i + 1] == index))
    //        {
    //            tris[0] = index;
    //            tris[1] = index2;
    //            tris[2] = triangles[i + 2];
    //            RemoveTriangle(i);
    //            return tris;
    //        }
    //        else if ((triangles[i + 1] == index && triangles[i + 2] == index2) || (triangles[i + 1] == index2 && triangles[i + 2] == index))
    //        {
    //            tris[0] = index;
    //            tris[1] = index2;
    //            tris[2] = triangles[i];
    //            RemoveTriangle(i);
    //            return tris;
    //        }
    //        else if ((triangles[i + 2] == index && triangles[i] == index2) || (triangles[i + 2] == index2 && triangles[i] == index))
    //        {
    //            tris[0] = index;
    //            tris[1] = index2;
    //            tris[2] = triangles[i + 1];
    //            RemoveTriangle(i);
    //            return tris;
    //        }
    //    }
    //    return tris;
    //}
}
