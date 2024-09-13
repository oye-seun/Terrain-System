using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using HelperClasses;


public class PlanarIslandMeshGen : MonoBehaviour
{
    // values for generating vert noise
    [Header("Top Settings")]
    [SerializeField] private int _topMapWidth;
    [SerializeField] private int _topMapHeight;
    [SerializeField] private float _topMapXOrigin;
    [SerializeField] private float _topMapYOrigin;
    [SerializeField] private float _topMapNoiseScale;
    [SerializeField] private int _topMapOctaves;
    [Range(0f, 1f)]
    [SerializeField] private float _topMapPersistance;
    [SerializeField] private float _topMapLacunarity;


    [Header("Bottom Settings")]
    [SerializeField] private int _downMapWidth;
    [SerializeField] private int _downMapHeight;
    [SerializeField] private float _downMapXOrigin;
    [SerializeField] private float _downMapYOrigin;
    [SerializeField] private float _downMapNoiseScale;
    [SerializeField] private int _downMapOctaves;
    [Range(0f, 1f)]
    [SerializeField] private float _downMapPersistance;
    [SerializeField] private float _downMapLacunarity;

    [Header("Boundary Settings")]
    // values for generating the island boundary
    [SerializeField] private Vector3 _dimensions;
    [SerializeField] private int _noOfIslandBoundaryPoints = 30;
    [SerializeField] private float _aveIslandRadius = 35;
    [SerializeField] private float _islandRadiusOffsetScale = 5;
    [SerializeField] private float _islandSamplingScale = 5;
    [SerializeField] private Vector3 _sampleOrigin;

    // values for adjusting the height of top mesh
    [Header("Top Mesh Values")]
    [SerializeField] private float _topMinHeight;
    [SerializeField] private float _topMaxHeight;
    [SerializeField] private float _topEdgeFalloff = 3f;

    // values for adjusting the height of bottom mesh
    [Header("Down Mesh Values")]
    [SerializeField] private float _downMinHeight;
    [SerializeField] private float _downMaxHeight;
    [SerializeField] private float _downEdgeFalloff = 3f;

    [Header("OBJ Path")]
    [SerializeField] private string _objPath;

    [Space]

    public bool DisplayVerts;
    public bool DisplayTriangles;
    public bool DisplayTriangles2;

    private Mesh _mesh;
    private List<Vector3> islandBoundary;
    private List<Vector3> topVerts;
    private List<int> topTriangles;
    int[] tris;
    TrianglePurifier purifier;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void GeneratePlaneMesh()
    {
        // create new mesh component
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;

        // generate boundary
        IslandShape islandShape = new IslandShape(_noOfIslandBoundaryPoints, _aveIslandRadius, _islandRadiusOffsetScale, _dimensions / 2, _islandSamplingScale, _sampleOrigin);
        islandBoundary = islandShape.GenerateBoundary();

        // create bounding box using the min and max of the island boundary
        BoundingBox boundBox = GetExtremeVerticePos(islandBoundary);

        // inrement on the x and z axis
        float xincr = (boundBox.max.x - boundBox.min.x)/_topMapWidth;
        float zincr = (boundBox.max.z - boundBox.min.z)/_topMapHeight;

        Vector2 extremeTopY = new Vector2(float.PositiveInfinity, float.NegativeInfinity);

        // generate simple square mesh (verts) using perlin noise
        topVerts = new List<Vector3>();
        for (int j = 0; j < _topMapHeight; j++)
        {
            for (int i = 0; i < _topMapWidth; i++)
            {
                float Yval = Noise.GetNoiseVal(_topMapXOrigin, _topMapYOrigin, i, j, _topMapNoiseScale, _topMapOctaves, _topMapPersistance, _topMapLacunarity);
                if (Yval > extremeTopY.y) extremeTopY.y = Yval;
                if (Yval < extremeTopY.x) extremeTopY.x = Yval;
                topVerts.Add(new Vector3((i*xincr)+boundBox.min.x, Yval, (j*zincr)+boundBox.min.z));
            }
        }

        // lerp mesh to height
        for(int i = 0; i < topVerts.Count; i++)
        {
            topVerts[i] = new Vector3(topVerts[i].x, Mathf.Lerp(_topMinHeight, _topMaxHeight, Mathf.InverseLerp(extremeTopY.x, extremeTopY.y, topVerts[i].y)), topVerts[i].z);
        }

        // scale mesh at the bottom


        // generate triangles
        topTriangles = new List<int>();
        for(int j = 0; j < topVerts.Count - _topMapWidth; j += _topMapWidth)
        {
            for (int i = j; i < (j + _topMapWidth - 1); i++)
            {
                topTriangles.Add(i); topTriangles.Add(i + _topMapWidth + 1); topTriangles.Add(i + 1);
                topTriangles.Add(i); topTriangles.Add(i + _topMapWidth); topTriangles.Add(i + _topMapWidth + 1);
            }
        }

        // use intersection to delete irrelevant triangles and adjust triangles at the boundary
        TrianglePurifier purifier = new TrianglePurifier();
        purifier.SetTrianglePurifier(islandBoundary, topTriangles, topVerts);
        tris = purifier.PurifyTriangles().ToArray();


        // reduce height to zero at the edges
        for (int i = 0; i < topVerts.Count; i++)
        {
            topVerts[i] = SmoothenEdges(islandBoundary, _dimensions / 2, topVerts[i], _topEdgeFalloff);
        }

        // find boundary

        // join both meshes


        OBJConverter objConverter = new OBJConverter();
        objConverter.Vertices = topVerts.ToArray();
        //objConverter.Triangles = topTriangles.ToArray();
        objConverter.Triangles = tris;
        objConverter.SaveToOBJ(_objPath, true);

        _mesh.Clear();
        _mesh.vertices = topVerts.ToArray();
        //_mesh.triangles = topTriangles.ToArray();
        _mesh.triangles = tris;
        //_mesh.triangles = purifier.Triangles.ToArray();
        _mesh.RecalculateNormals();
        //_mesh.uv = uv1.ToArray();
    }
    

    private void OnDrawGizmos()
    {
        if (DisplayVerts)
        {
            foreach (Vector3 vert in topVerts)
            {
                Gizmos.DrawCube(vert + transform.position, Vector3.one * 0.1f);
            }
        }

        for (int i = 0; i < topTriangles.Count && DisplayTriangles; i += 3)
        {
            Debug.DrawLine(topVerts[topTriangles[i]] + transform.position, topVerts[topTriangles[i + 1]] + transform.position, Color.red);
            Debug.DrawLine(topVerts[topTriangles[i + 1]] + transform.position, topVerts[topTriangles[i + 2]] + transform.position, Color.red);
            Debug.DrawLine(topVerts[topTriangles[i + 2]] + transform.position, topVerts[topTriangles[i]] + transform.position, Color.red);
        }

        for (int i = 0; i < tris.Length && DisplayTriangles2; i += 3)
        {
            Debug.DrawLine(topVerts[tris[i]] + transform.position, topVerts[tris[i + 1]] + transform.position, Color.red);
            Debug.DrawLine(topVerts[tris[i + 1]] + transform.position, topVerts[tris[i + 2]] + transform.position, Color.red);
            Debug.DrawLine(topVerts[tris[i + 2]] + transform.position, topVerts[tris[i]] + transform.position, Color.red);
        }

        //for (int i = 0; i < purifier.Triangles.Count && DisplayTriangles2; i += 3)
        //{
        //    Debug.DrawLine(topVerts[purifier.Triangles[i]] + transform.position, topVerts[purifier.Triangles[i + 1]] + transform.position, Color.red);
        //    Debug.DrawLine(topVerts[purifier.Triangles[i + 1]] + transform.position, topVerts[purifier.Triangles[i + 2]] + transform.position, Color.red);
        //    Debug.DrawLine(topVerts[purifier.Triangles[i + 2]] + transform.position, topVerts[purifier.Triangles[i]] + transform.position, Color.red);
        //}

        for (int i = 0; purifier != null && i < purifier.Intersections.Count; i++)
        {
            Gizmos.DrawCube(purifier.Intersections[i] + transform.position, Vector3.one * 0.1f);
        }

        for(int i = 0; i < islandBoundary.Count; i++)
        {
            int j = (i+1) % islandBoundary.Count;
            Debug.DrawLine(islandBoundary[i] + transform.position, islandBoundary[j] + transform.position, Color.cyan);
        }
    }


    // Get the minimum or maximum vertices
    private BoundingBox GetExtremeVerticePos(IEnumerable<Vector3> boundary)
    {
        BoundingBox extreme = new BoundingBox(Vector3.positiveInfinity, Vector3.negativeInfinity);
        foreach (Vector3 vec in boundary)
        {
            if (vec.x < extreme.min.x) extreme.min.x = vec.x;
            if (vec.y < extreme.min.y) extreme.min.y = vec.y;
            if (vec.z < extreme.min.z) extreme.min.z = vec.z;

            if (vec.x > extreme.max.x) extreme.max.x = vec.x;
            if (vec.y > extreme.max.y) extreme.max.y = vec.y;
            if (vec.z > extreme.max.z) extreme.max.z = vec.z;
        }
        return extreme;
    }

    private Vector3 SmoothenEdges(List<Vector3> boundary, Vector3 centre, Vector3 vertPos, float falloff)
    {
        Vector3 zeroVertPos = new Vector3(vertPos.x, centre.y, vertPos.z);
        Vector3 dirA = (zeroVertPos - centre).normalized;
        if (dirA.magnitude == 0)
        {
            //Debug.Log("zeroVert");
            return vertPos;
        }
        float angle = 0;
        if (dirA.z >= 0) angle = Mathf.Acos(dirA.x) * Mathf.Rad2Deg;
        else if (dirA.z < 0) angle = 360 - (Mathf.Acos(dirA.x) * Mathf.Rad2Deg);
        int index = (angle != 0 && angle != 360) ? (int)(angle / (360f / boundary.Count)) : 0;
        //Debug.Log(angle);
        Vector3 originB = boundary[index];
        int j = (index + 1) % boundary.Count;
        Vector3 dirB = (originB - boundary[j]).normalized;

        Vector3 intersection = VectorIntersection.VectorIntersectionCheck(centre, dirA, originB, dirB);
        float dist = Vector3.Distance(zeroVertPos, intersection) / Vector3.Distance(intersection, centre);
        Vector3 lerpVal = Vector3.Lerp(zeroVertPos, vertPos, Mathf.Pow(dist, falloff));
        //Vector3 lerpVal = Vector3.Lerp(zeroVertPos, vertPos, 0.5f);
        return lerpVal;
    }
}



#if (UNITY_EDITOR)

[CustomEditor(typeof(PlanarIslandMeshGen))]
public class PlanarIslandMeshGenEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        PlanarIslandMeshGen meshGen = (PlanarIslandMeshGen)target;
        //if (DrawDefaultInspector())
        //{
        //    if (meshGen.AutoUpdate) meshGen.GenerateMesh();
        //}
        DrawDefaultInspector();

        if (GUILayout.Button("Generate Map"))
        {
            meshGen.GeneratePlaneMesh();
        }
    }
}
#endif