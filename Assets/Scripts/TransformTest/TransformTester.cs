using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformTester : MonoBehaviour
{
    public float turnAngle;
    public NormalTest normalTest;
    private Vector3[] verts;

    public void Rotate()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        int[] triangles = mesh.triangles;
        Vector3[] verts = mesh.vertices;
        


        float cos = Mathf.Cos(turnAngle * Mathf.Deg2Rad);
        float sin = Mathf.Sin(turnAngle * Mathf.Deg2Rad);

        Matrix3x3 rotationMat = new Matrix3x3(
            new Vector3(cos,  0,  sin), 
            new Vector3( 0,   1,   0 ), 
            new Vector3(-sin, 0,  cos)
            );

        for(int i = 0; i < verts.Length; i++)
        {
            verts[i] = rotationMat.MATxVEC(verts[i]);
        }
        mesh.vertices = verts;
        normalTest.Recalculate6();

        Vector3[] normals = mesh.normals;
        Vector3 normal = normals[1].normalized;
        //Vector3 nprime = new Vector3(-normal.z, 0, normal.x).normalized;
        nprime = Vector3.Cross(new Vector3(0, 1, 0), normal).normalized;
        float o = Mathf.Acos(normal.y) * Mathf.Rad2Deg;
        float a = Mathf.Acos(nprime.x) * Mathf.Rad2Deg;
        //o = (normal.z > 0) ? o : -o;
        a = (nprime.z > 0) ? a : -a;

        Debug.Log("alpha: " + a + " theta: " + o);
    }

    public void AlignXY()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        int[] triangles = mesh.triangles;
        Vector3[] verts = mesh.vertices;
        Vector3[] normals = mesh.normals;

        // angles first
        Vector3 normal = normals[1].normalized;
        //Vector3 nprime = new Vector3(normal.y, -normal.x, 0);
        Vector3 nprime = Vector3.Cross(normal, new Vector3(0, 0, 1)).normalized;
        float o = Mathf.Acos(normal.z) * Mathf.Rad2Deg;
        float a = Mathf.Acos(nprime.y) * Mathf.Rad2Deg;

        Debug.Log("alpha: " + a + " theta: " + o);

        float coso = Mathf.Cos(o * Mathf.Deg2Rad);      float cosa = Mathf.Cos(a * Mathf.Deg2Rad);
        float sino = Mathf.Sin(o * Mathf.Deg2Rad);      float sina = Mathf.Sin(a * Mathf.Deg2Rad);
        float tano = Mathf.Tan(o * Mathf.Deg2Rad);      float tana = Mathf.Tan(a * Mathf.Deg2Rad);
        float seco = 1 / coso;                          float seca = 1 / cosa;
        float tanSo = tano * tano;                      float tanSa = tana * tana;          

        Matrix3x3 rotationMat = new Matrix3x3(
            new Vector3((seca*seco) - (tanSa*cosa*seco) - (tano*coso*((seca*tano)-(tanSa*cosa*tano))), (-tana*cosa*seco) + (tana*cosa*tanSo*coso), -sino),
            new Vector3(                          cosa*tana,                                                                cosa,                     0 ),
            new Vector3(                ((seca*tano) - (tanSa*cosa*tano))/seco,                                    -tana*cosa*tano*coso,            coso)
            );

        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = rotationMat.MATxVEC(verts[i]);
        }
        mesh.vertices = verts;
        normalTest.Recalculate6();
    }

    private Vector3 nprime;
    public void Align2()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        int[] triangles = mesh.triangles;
        Vector3[] verts = mesh.vertices;
        Vector3[] normals = mesh.normals;

        // angles first
        Vector3 normal = normals[1].normalized;
        //Vector3 nprime = new Vector3(-normal.z, 0, normal.x).normalized;
        nprime = Vector3.Cross(new Vector3(0, 1, 0), normal).normalized;
        float o = Mathf.Acos(normal.y) * Mathf.Rad2Deg;
        float a = Mathf.Acos(nprime.x) * Mathf.Rad2Deg;
        //o = (normal.z > 0) ? o : -o;
        a = (nprime.z > 0) ? a : -a;

        Debug.Log("alpha: " + a + " theta: " + o);

        //reverse flip
        //o = -o;
        a = -a;

        float coso = Mathf.Cos(o * Mathf.Deg2Rad); float cosa = Mathf.Cos(a * Mathf.Deg2Rad);
        float sino = Mathf.Sin(o * Mathf.Deg2Rad); float sina = Mathf.Sin(a * Mathf.Deg2Rad);
        float tano = Mathf.Tan(o * Mathf.Deg2Rad); float tana = Mathf.Tan(a * Mathf.Deg2Rad);
        float seco = 1 / coso; float seca = 1 / cosa;
        float tanSo = tano * tano; float tanSa = tana * tana;

        Matrix3x3 XrotationMat = new Matrix3x3(
            new Vector3(1,    0,    0 ),
            new Vector3(0,  coso, sino),
            new Vector3(0, -sino, coso)
            );

        Matrix3x3 YrotationMat = new Matrix3x3(
            new Vector3(cosa, 0, -sina),
            new Vector3( 0,   1,   0  ),
            new Vector3(sina, 0,  cosa)
            );

        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = YrotationMat.MATxVEC(verts[i]);
            verts[i] = XrotationMat.MATxVEC(verts[i]);
        }
        mesh.vertices = verts;
        normalTest.Recalculate6();

        normals = mesh.normals;

        // check angles again
        normal = normals[1].normalized;
        nprime = Vector3.Cross(new Vector3(0, 1, 0), normal).normalized;
        o = Mathf.Acos(normal.y) * Mathf.Rad2Deg;
        a = Mathf.Acos(nprime.x) * Mathf.Rad2Deg;
        o = (normal.z > 0) ? o : -o;
        a = (nprime.z > 0) ? a : -a;

        Debug.Log("after rot, alpha: " + a + " theta: " + o);
    }


    public void RotateArb()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        int[] triangles = mesh.triangles;
        Vector3[] verts = mesh.vertices;
        Vector3[] normals = new Vector3[verts.Length];

        // angles first
        Vector3 normal = normals[1].normalized;
        //Vector3 nprime = new Vector3(normal.y, -normal.x, 0);
        Vector3 nprime = Vector3.Cross(normal, new Vector3(0, 0, 1)).normalized;
        //float o = Mathf.Acos(normal.z) * Mathf.Rad2Deg;
        //float a = Mathf.Acos(nprime.y) * Mathf.Rad2Deg;
        
        float o = 10;
        float a = 10;

        float coso = Mathf.Cos(o * Mathf.Deg2Rad); float cosa = Mathf.Cos(a * Mathf.Deg2Rad);
        float sino = Mathf.Sin(o * Mathf.Deg2Rad); float sina = Mathf.Sin(a * Mathf.Deg2Rad);
        float tano = Mathf.Tan(o * Mathf.Deg2Rad); float tana = Mathf.Tan(a * Mathf.Deg2Rad);
        float seco = 1 / coso; float seca = 1 / cosa;
        float tanSo = tano * tano; float tanSa = tana * tana;

        Matrix3x3 rotationMat = new Matrix3x3(
            new Vector3( cosa*coso, sina,  cosa*sino),
            new Vector3(-sina*coso, cosa, -sina*sino),
            new Vector3(-sino,       0,      coso   )
            );

        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = rotationMat.MATxVEC(verts[i]);
        }
        mesh.vertices = verts;
        normalTest.Recalculate6();
    }

    public void ResetPlane()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        List<int> triangles = new List<int>();
        verts = new Vector3[121];

        //Debug.Log("vert count: " + verts.Length);
        for (int i = 0; i < 11; i++)
        {
            for(int j = 0; j < 11; j++)
            {
                verts[(i*11)+j] = new Vector3(1f * i, 0, 1f * j) - new Vector3(5, 0, 5);
            }
        }

        int startNum = 0;
        for (int y = 1; y < 11; y++)
        {
            for(int x = startNum; x < startNum + 10; x++)
            {
                triangles.Add(x); triangles.Add(x + 1); triangles.Add(x + 12);
                triangles.Add(x); triangles.Add(x + 12); triangles.Add(x + 11); 
            }
            startNum += 11;
        }
        mesh.Clear();
        mesh.vertices = verts;
        mesh.triangles = triangles.ToArray();
        normalTest.Recalculate6();
    }

    private void OnDrawGizmos()
    {
        foreach(Vector3 vert in verts)
        {
            Gizmos.DrawSphere(vert + transform.position, 0.04f);
        }

        Debug.DrawLine(transform.position, transform.position + (nprime * 3), Color.red);
    }
}

public class Matrix3x3
{
    Vector3 Row1;
    Vector3 Row2;
    Vector3 Row3;
    
    public Matrix3x3(Vector3 row1, Vector3 row2, Vector3 row3)
    {
        Row1 = row1;
        Row2 = row2;
        Row3 = row3;
    }

    public Vector3 MATxVEC(Vector3 vec) 
    {
        Vector3 result = new Vector3(0, 0, 0);
        result.x = (Row1.x * vec.x) + (Row1.y * vec.y) + (Row1.z * vec.z);
        result.y = (Row2.x * vec.x) + (Row2.y * vec.y) + (Row2.z * vec.z);
        result.z = (Row3.x * vec.x) + (Row3.y * vec.y) + (Row3.z * vec.z);
        return result;
    }
}
