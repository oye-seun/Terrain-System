using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// this class converts to and from only .obj files with triangles per face
public class OBJConverter
{
    public Vector3[] Vertices;
    public Vector3[] Normals;
    public int[] Triangles;
    public Vector2[] UVs;


    public OBJConverter()
    {
        Vertices = new Vector3[0];
        Triangles = new int[0];
        Normals = new Vector3[0];
        UVs = new Vector2[0];
    }
    public OBJConverter(Vector3[] vertices, int[] triangles, Vector3[] normals, Vector2[] uVs)
    {
        Vertices = vertices;
        Triangles = triangles;
        Normals = normals;
        UVs = uVs;
    }

    public void SaveToOBJ(string path, bool smoothShading)
    {
        StreamWriter writer = new StreamWriter(path);
        writer.WriteLine("# OBJ created with OBJConverter developed by Seun");
        writer.WriteLine("# www.seun.games");

        // write Vertices
        foreach(Vector3 v in Vertices)
        {
            writer.WriteLine("v " + v.x + " " + v.y + " " + v.z);
        }

        // write Texture coordinates
        foreach(Vector2 uv in UVs)
        {
            writer.WriteLine("vt " + uv.x + " " + uv.y);
        }

        // write Normals (per verts)
        foreach(Vector3 n in Normals)
        {
            writer.WriteLine("vn " + n.x + " " + n.y + " " + n.z);
        }

        // wrtie  shading
        if(smoothShading) writer.WriteLine("s on");
        else writer.WriteLine("s off");

        // write faces
        for(int i = 0; i < Triangles.Length; i += 3)
        {
            if (Vertices.Length != 0 && UVs.Length == 0 && Normals.Length == 0)
            {
                writer.WriteLine($"f {Triangles[i] + 1} {Triangles[i + 1] + 1} {Triangles[i + 2] + 1}");
            }
            else if(Vertices.Length != 0 && UVs.Length != 0 && Normals.Length == 0)
            {
                writer.WriteLine($"f {Triangles[i] + 1}/{Triangles[i] + 1}   {Triangles[i + 1] + 1}/{Triangles[i + 1] + 1}  {Triangles[i + 2] + 1}/{Triangles[i + 2] + 1}");
            }
            else if(Vertices.Length != 0 && UVs.Length != 0 && Normals.Length != 0)
            {
                writer.WriteLine($"f {Triangles[i] + 1}/{Triangles[i] + 1}/{Triangles[i] + 1}   {Triangles[i + 1] + 1}/{Triangles[i + 1] + 1}/{Triangles[i + 1] + 1}  {Triangles[i + 2] + 1}/{Triangles[i + 2] + 1}/{Triangles[i + 2] + 1}");
            }
        }
        

        writer.Close();
    }
}
