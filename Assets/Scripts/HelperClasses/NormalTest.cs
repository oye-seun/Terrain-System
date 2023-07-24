using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalTest : MonoBehaviour
{
    public bool showNormals;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Recalculate()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        mesh.RecalculateNormals();
    }

    public void Recalculate2()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        int[] triangles = mesh.triangles;
        verts = mesh.vertices;
        normals = new Vector3[verts.Length];

        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = (verts[i] - Vector3.zero).normalized;
        }
        
        mesh.normals = normals;
        Debug.Log("vert count: " + verts.Length);
        Debug.Log("normals count: " + normals.Length);
    }

    public void Recalculate3()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        int[] triangles = mesh.triangles;
        verts = mesh.vertices;
        normals = new Vector3[verts.Length];

        for (int i = triangles.Length - 3; i >= 0; i -= 3)
        {
            Vector3 dirA = (verts[triangles[i]] - verts[triangles[i + 1]]).normalized;
            Vector3 dirB = (verts[triangles[i + 1]] - verts[triangles[i + 2]]).normalized;

            Vector3 normal = Vector3.Cross(dirA, dirB).normalized;

            if (normals[triangles[i]] == Vector3.zero)
                normals[triangles[i]] = normal;
            else
                normals[triangles[i]] = (normals[triangles[i]] + normal) / 2;

            if (normals[triangles[i + 1]] == Vector3.zero)
                normals[triangles[i + 1]] = normal;
            else
                normals[triangles[i + 1]] = (normals[triangles[i + 1]] + normal) / 2;

            if (normals[triangles[i + 2]] == Vector3.zero)
                normals[triangles[i + 2]] = normal;
            else
                normals[triangles[i + 2]] = (normals[triangles[i + 2]] + normal) / 2;

            if(verts[triangles[i]].y == 0)
                normals[triangles[i]] = (verts[triangles[i]] - Vector3.zero).normalized;
            if (verts[triangles[i + 1]].y == 0)
                normals[triangles[i + 1]] = (verts[triangles[i + 1]] - Vector3.zero).normalized;
            if (verts[triangles[i + 2]].y == 0)
                normals[triangles[i + 2]] = (verts[triangles[i + 2]] - Vector3.zero).normalized;
        }

        mesh.normals = normals;
        Debug.Log("vert count: " + verts.Length);
        Debug.Log("normals count: " + normals.Length);
    }

    public void Recalculate4()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        int[] triangles = mesh.triangles;
        verts = mesh.vertices;
        normals = new Vector3[verts.Length];

        for (int i = triangles.Length - 3; i >= 0; i -= 3)
        {
            Vector3 dirA = (verts[triangles[i]] - verts[triangles[i + 1]]).normalized;
            Vector3 dirB = (verts[triangles[i + 1]] - verts[triangles[i + 2]]).normalized;

            Vector3 normal = Vector3.Cross(dirA, dirB).normalized;

            
                normals[triangles[i]] = normal;

                normals[triangles[i + 1]] = normal;

                normals[triangles[i + 2]] = normal;
        }

        mesh.normals = normals;
        Debug.Log("vert count: " + verts.Length);
        Debug.Log("normals count: " + normals.Length);
    }


    public void Recalculate5()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        int[] triangles = mesh.triangles;
        verts = mesh.vertices;
        normals = new Vector3[verts.Length];

        for (int i = triangles.Length - 3; i >= 0; i -= 3)
        {
            Vector3 dirA = (verts[triangles[i]] - verts[triangles[i + 1]]).normalized;
            Vector3 dirB = (verts[triangles[i + 1]] - verts[triangles[i + 2]]).normalized;

            Vector3 normal = Vector3.Cross(dirA, dirB).normalized;
            if(Vector3.Dot(normal, (verts[triangles[i + 1]] - Vector3.zero).normalized) < 0f)
                normal = -normal;

            if (normals[triangles[i]] == Vector3.zero)
                normals[triangles[i]] = normal;
            else
                normals[triangles[i]] = (normals[triangles[i]] + normal) / 2;

            if (normals[triangles[i + 1]] == Vector3.zero)
                normals[triangles[i + 1]] = normal;
            else
                normals[triangles[i + 1]] = (normals[triangles[i + 1]] + normal) / 2;

            if (normals[triangles[i + 2]] == Vector3.zero)
                normals[triangles[i + 2]] = normal;
            else
                normals[triangles[i + 2]] = (normals[triangles[i + 2]] + normal) / 2;
        }

        mesh.normals = normals;
        Debug.Log("vert count: " + verts.Length);
        Debug.Log("normals count: " + normals.Length);
    }

    Vector3[] verts;
    Vector3[] normals;

    public void Recalculate6()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        int[] triangles = mesh.triangles;
        verts = mesh.vertices;
        normals = new Vector3[verts.Length];

        for (int i = triangles.Length - 3; i >= 0; i -= 3)
        {
            Vector3 dirA = (verts[triangles[i]] - verts[triangles[i + 1]]);
            Vector3 dirB = (verts[triangles[i + 1]] - verts[triangles[i + 2]]);

            Vector3 normal = Vector3.Cross(dirA, dirB);

            normals[triangles[i]] += normal;
            normals[triangles[i + 1]] += normal;
            normals[triangles[i + 2]] += normal;

        }

        for (int i = 0; i < normals.Length; i++)
            normals[i] = normals[i].normalized;

        mesh.normals = normals;
        Debug.Log("vert count: " + verts.Length);
        Debug.Log("normals count: " + normals.Length);
    }

    public void Recalculate7()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        int[] triangles = mesh.triangles;
        verts = mesh.vertices;
        normals = new Vector3[verts.Length];

        for (int i = triangles.Length - 3; i >= 0; i -= 3)
        {
            Vector3 dirA = (verts[triangles[i]] - verts[triangles[i + 1]]);
            Vector3 dirB = (verts[triangles[i + 1]] - verts[triangles[i + 2]]);

            Vector3 normal = Vector3.Cross(dirA, dirB);

            normals[triangles[i]] += normal;
            normals[triangles[i + 1]] += normal;
            normals[triangles[i + 2]] += normal;


            if (verts[triangles[i]].y == 0)
                normals[triangles[i]] = (verts[triangles[i]] - Vector3.zero).normalized;
            if (verts[triangles[i + 1]].y == 0)
                normals[triangles[i + 1]] = (verts[triangles[i + 1]] - Vector3.zero).normalized;
            if (verts[triangles[i + 2]].y == 0)
                normals[triangles[i + 2]] = (verts[triangles[i + 2]] - Vector3.zero).normalized;
        }

        for (int i = 0; i < normals.Length; i++)
            normals[i] = normals[i].normalized;

        mesh.normals = normals;
        Debug.Log("vert count: " + verts.Length);
        Debug.Log("normals count: " + normals.Length);
    }

    //private void OnDrawGizmos()
    //{
    //    for(int i = 0; i < normals.Length && showNormals; i++)
    //    {
    //        Debug.DrawLine(verts[i] + transform.position, verts[i] + transform.position + (normals[i] * 1.5f), Color.cyan);
    //    }
    //}
}
