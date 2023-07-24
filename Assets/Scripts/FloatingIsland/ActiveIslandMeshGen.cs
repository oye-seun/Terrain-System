using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.Image;

public class ActiveIslandMeshGen : MonoBehaviour
{
    [SerializeField] private IslandMapGen _islandMapGen;
    [SerializeField] private Vector3 _dimensions;
    [SerializeField] private int _noOfIslandBoundaryPoints = 30;
    [SerializeField] private float _aveIslandRadius = 35;
    [SerializeField] private float _islandRadiusOffsetScale = 5;
    [SerializeField] private float _islandSamplingScale = 5;
    [SerializeField] private Vector3 _sampleOrigin;
    [SerializeField] private float _gizmoSphereRadius;

    [Header("Top Mesh Values")]
    [SerializeField] private float _topMinHeight;
    [SerializeField] private float _topMaxHeight;
    [SerializeField] private float _topEdgeFalloff = 3f;

    [Header("Down Mesh Values")]
    [SerializeField] private float _downMinHeight;
    [SerializeField] private float _downMaxHeight;
    [SerializeField] private float _downEdgeFalloff = 3f;

    [Header("Extras")]
    public bool EliminateVerticeClusters;
    [Range(0f, 1f)]
    public float VertEliminationCheckRadius;
    public float Tolerance;
    public bool closeVertsAtEdges;
    public float CloseVertsTolerance;
    public float BoundaryDistnce;

    public bool AutoUpdate;
    public bool DisplayVerts;
    public bool DisplayTriangles;
    public bool DisplaySupraTris;
    public bool DisplayUVs;
    public int countermax;

    public int Progress;


    private Mesh _mesh;
    List<Vector2> uv1;
    List<int> triangles;
    List<int> triangles2;
    private Triangle SuperTriangle;

    private List<Vector3> islandBoundary;

    private Vector3[] _topVerts;
    private List<Vector3> _cutoutTopVerts;
    private Vector3[] _downVerts;
    private List<Vector3> _cutoutDownVerts;
    List<Vector3> uvSeamVerts;
    Dictionary<int, int> vertMap;
    List<int> boundarySeam;
    int lastVert;
    int lastButtVert;




    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public void GenerateMesh(int progress)
    {
        switch (progress) 
        {
            case 0:
                {
                    //----------- Generate Mesh -------------------------------------------------------------------------------------------------
                    _mesh = new Mesh();
                    GetComponent<MeshFilter>().mesh = _mesh;

                    IslandShape islandShape = new IslandShape(_noOfIslandBoundaryPoints, _aveIslandRadius, _islandRadiusOffsetScale, _dimensions / 2, _islandSamplingScale, _sampleOrigin);
                    islandBoundary = islandShape.GenerateBoundary();
                    //Debug.Log(islandBoundary.Count);

                    // top process
                    _topVerts = To1DArray(_islandMapGen.GenerateTopMap(), _dimensions.x, _dimensions.z, _topMinHeight, _topMaxHeight);
                    _cutoutTopVerts = CutoutIsland(islandBoundary, _topVerts);

                    // blend everything to baseHeight at the edges
                    for (int i = 0; i < _cutoutTopVerts.Count; i++)
                    {
                        _cutoutTopVerts[i] = SmoothenEdges(islandBoundary, _dimensions / 2, _cutoutTopVerts[i], _topEdgeFalloff);
                    }

                    // bottom process
                    _downVerts = To1DArray(_islandMapGen.GenerateDownMap(), _dimensions.x, _dimensions.z, _downMinHeight, _downMaxHeight);
                    _cutoutDownVerts = CutoutIsland(islandBoundary, _downVerts);

                    // blend everything to baseHeight at the edges
                    for (int i = 0; i < _cutoutDownVerts.Count; i++)
                    {
                        _cutoutDownVerts[i] = SmoothenEdges(islandBoundary, _dimensions / 2, _cutoutDownVerts[i], _downEdgeFalloff);
                    }


                    //_cutoutTopVerts.AddRange(AddEdgeVertices(min, countX, countZ, distIncr));
                    //_cutoutDownVerts.AddRange(AddEdgeVertices(min, countX, countZ, distIncr));
                    _cutoutTopVerts.AddRange(islandBoundary);
                    //int downVertMargin = _cutoutDownVerts.Count;
                    _cutoutDownVerts.AddRange(islandBoundary);


                    if (closeVertsAtEdges)
                    {
                        CloseVertsAtEdges(_cutoutTopVerts, _dimensions / 2, _aveIslandRadius - _islandRadiusOffsetScale, CloseVertsTolerance);
                        CloseVertsAtEdges(_cutoutDownVerts, _dimensions / 2, _aveIslandRadius - _islandRadiusOffsetScale, CloseVertsTolerance);
                    }
                    //---------------------------------------------------------------------------------------------------------------------------
                    break;
                }

            case 1:
                {
                    //------------- Generate Triangles ------------------------------------------------------------------------------------------
                    //GenerateTriangles3(_cutoutTopVerts, _islandMapGen.TopMapHeight);
                    SuperTriangle = GenerateSuperTriangle();



                    //if (EliminateVerticeClusters)
                    //{
                    //    EliminateRedundantVerts(_cutoutTopVerts, _dimensions / 2, distIncr * VertEliminationCheckRadius);
                    //    EliminateRedundantVerts(_cutoutDownVerts, _dimensions / 2, distIncr * VertEliminationCheckRadius);
                    //}

                    DelauneyTriangulation delauney = new DelauneyTriangulation(_cutoutTopVerts, SuperTriangle);
                    triangles = delauney.Triangulate(true);
                    RemoveTrianglesAtCorners(_cutoutTopVerts, triangles, _dimensions / 2, _aveIslandRadius - _islandRadiusOffsetScale);


                    DelauneyTriangulation delauney2 = new DelauneyTriangulation(_cutoutDownVerts, SuperTriangle);
                    triangles2 = delauney2.Triangulate(false);
                    RemoveTrianglesAtCorners(_cutoutDownVerts, triangles2, _dimensions / 2, _aveIslandRadius - _islandRadiusOffsetScale);

                    // shift verts to the centre pos
                    _cutoutTopVerts = ShiftVertsToCentre(_cutoutTopVerts, _dimensions / 2);
                    _cutoutDownVerts = ShiftVertsToCentre(_cutoutDownVerts, new Vector3((_dimensions / 2).x, BoundaryDistnce, (_dimensions / 2).z));
                    //--------------------------------------------------------------------------------------------------------------------------
                    break;
                }
            case 2:
                {
                    //------------ generate UVs -------------------------------------------------------------------------------------------------
                    UVGenerator UVGen = new UVGenerator(triangles, _cutoutTopVerts, countermax);
                    uv1 = new List<Vector2>();
                    uv1.AddRange(UVGen.GenerateUV4());
                    int uvSplit = uv1.Count;
                    UVGenerator UVGen2 = new UVGenerator(triangles2, _cutoutDownVerts, countermax);
                    uv1.AddRange(UVGen2.GenerateUV4());
                    Vector2 uvAdd = new Vector2(0.5f, 0);
                    for (int i = 0; i < uv1.Count; i++)
                    {
                        if (i < uvSplit)
                        {
                            uv1[i] = new Vector2(uv1[i].x / 2, uv1[i].y / 2);
                        }
                        else
                        {
                            uv1[i] = new Vector2(uv1[i].x / 2, uv1[i].y / 2) + uvAdd;
                        }
                    }
                    
                    //--------------------------------------------------------------------------------------------------------------------------
                    break;
                }
            case 3:
                {
                    //------------- Generate UVs for seam --------------------------------------------------------------------------------------
                    // join verts and triangles
                    lastVert = _cutoutTopVerts.Count;
                    //Debug.Log("lastvert: " + lastVert);
                    lastButtVert = _cutoutDownVerts.Count;

                    // join both boundaries
                    
                    boundarySeam = JoinMeshes(triangles, triangles2, lastVert, out uvSeamVerts, out vertMap);
                    // generate UVs for the seam
                    Vector2 estimatedCentre = new Vector2(0.75f, 0.25f);
                    // sample 3 uv triangles and find ave length of side
                    float uvDist = 0;
                    for (int i = 0; i < 9; i += 3)
                    {
                        uvDist += (uv1[triangles[i]] - uv1[triangles[i + 1]]).magnitude / 9;
                        uvDist += (uv1[triangles[i + 1]] - uv1[triangles[i + 2]]).magnitude / 9;
                        uvDist += (uv1[triangles[i + 2]] - uv1[triangles[i]]).magnitude / 9;
                    }

                    Vector2[] tempArray = new Vector2[uvSeamVerts.Count];
                    for (int i = 0; i < tempArray.Length; i++)
                        tempArray[i] = new Vector2(0, 0);
                    uv1.AddRange(tempArray);
                    //Debug.Log("uvDist: " + uvDist);
                    foreach (int index in vertMap.Keys)
                    {
                        uv1[vertMap[index]] = uv1[index + lastVert] + ((uv1[index + lastVert] - estimatedCentre).normalized * uvDist);
                    }
                    //------------------------------------------------------------------------------------------------------------------
                    break;
                }
            case 4:
                {
                    //-------------- finish off ----------------------------------------------------------------------------------------
                    _cutoutTopVerts.AddRange(_cutoutDownVerts);
                    _cutoutTopVerts.AddRange(uvSeamVerts);

                    triangles.AddRange(boundarySeam);
                    for (int i = 0; i < triangles2.Count; i++)
                    {
                        triangles2[i] = triangles2[i] + lastVert;
                    }
                    triangles.AddRange(triangles2);


                    // update the mesh
                    _mesh.Clear();
                    _mesh.vertices = _cutoutTopVerts.ToArray();
                    _mesh.triangles = triangles.ToArray();
                    _mesh.RecalculateNormals();
                    _mesh.uv = uv1.ToArray();
                    //------------------------------------------------------------------------------------------------------------------

                    Debug.Log("Finished Generating Mesh");
                    Progress = 0;
                    break;
                }
        }
        
        
    }

    


    private Triangle GenerateSuperTriangle()
    {
        Vector3[] directions = new Vector3[3];
        Vector3[] positions = new Vector3[3];
        float startAngle = 30;
        for(int i = 0; i < 3; i++)
        {
            directions[i] = new Vector3(Mathf.Cos((startAngle + (i * 120)) * Mathf.Deg2Rad), 0, Mathf.Sin((startAngle + (i * 120)) * Mathf.Deg2Rad)).normalized;
            positions[i] = (_dimensions / 2) + (directions[i] * (_aveIslandRadius + _islandRadiusOffsetScale));
            Vector2 newDir = VectorIntersection.RotateDirVectorBy(new Vector2(directions[i].x, directions[i].z), 90);
            directions[i] = new Vector3(newDir.x, (_dimensions / 2).y, newDir.y);
        }
        Triangle superTriangle = new Triangle(
        VectorIntersection.VectorIntersectionCheck(positions[0], directions[0], positions[1], directions[1]),
        VectorIntersection.VectorIntersectionCheck(positions[1], directions[1], positions[2], directions[2]),
        VectorIntersection.VectorIntersectionCheck(positions[2], directions[2], positions[0], directions[0]));

        return superTriangle;
    }

    private void OnDrawGizmos()
    {
        if (DisplayVerts)
        {
            foreach (Vector3 vec in _cutoutTopVerts)
            {
                Gizmos.DrawSphere(vec + transform.position, _gizmoSphereRadius);
            }

            foreach (Vector3 vec in _cutoutDownVerts)
            {
                Gizmos.DrawSphere(vec + transform.position, _gizmoSphereRadius);
            }
        }

        if (DisplaySupraTris)
        {
            Debug.DrawLine(SuperTriangle.A + transform.position, SuperTriangle.B + transform.position, Color.red);
            Debug.DrawLine(SuperTriangle.B + transform.position, SuperTriangle.C + transform.position, Color.red);
            Debug.DrawLine(SuperTriangle.C + transform.position, SuperTriangle.A + transform.position, Color.red);
        }

        for (int i = 0; i < triangles.Count /*triangleCount*/ && DisplayUVs; i += 3)
        {
            Vector3 a = new Vector3(uv1[triangles[i]].x, 5, uv1[triangles[i]].y);
            Vector3 b = new Vector3(uv1[triangles[i + 1]].x, 5, uv1[triangles[i + 1]].y);
            Vector3 c = new Vector3(uv1[triangles[i + 2]].x, 5, uv1[triangles[i + 2]].y);

            bool hasUvZero = triangles[i] == 0 || triangles[i + 2] == 0 || triangles[i + 2] == 0;
            bool allZero = a.x != 0 && b.x != 0 && c.x != 0 && a.y != 0 && b.y != 0 && c.y != 0;
            if (allZero)
            {
                Color color = (hasUvZero)? Color.yellow : Color.blue;
                Debug.DrawLine(a + transform.position, b + transform.position, color);
                Debug.DrawLine(b + transform.position, c + transform.position, color);
                Debug.DrawLine(c + transform.position, a + transform.position, color);
            }
        }


        for (int i = 0; i < triangles.Count && DisplayTriangles; i += 3)
        {
            //if (triangles[i] >= _cutoutTopVerts.Count || triangles[i] < 0)
            //{
            //    Debug.Log("out of bound tris: " + triangles[i]);
            //}
            Debug.DrawLine(_cutoutTopVerts[triangles[i]] + transform.position, _cutoutTopVerts[triangles[i + 1]] + transform.position, Color.red);
            Debug.DrawLine(_cutoutTopVerts[triangles[i + 1]] + transform.position, _cutoutTopVerts[triangles[i + 2]] + transform.position, Color.red);
            Debug.DrawLine(_cutoutTopVerts[triangles[i + 2]] + transform.position, _cutoutTopVerts[triangles[i]] + transform.position, Color.red);
        }


        for (int i = 0; i < islandBoundary.Count && DisplayVerts; i++)
        {
            int j = (i + 1) % islandBoundary.Count;
            Debug.DrawLine(islandBoundary[i] - (_dimensions/2) + transform.position, islandBoundary[j] - (_dimensions / 2) + transform.position, Color.green);
        }
    }

    private Vector3 SmoothenEdges(List<Vector3> boundary, Vector3 centre, Vector3 vertPos, float falloff)
    {
        Vector3 zeroVertPos = new Vector3(vertPos.x, centre.y, vertPos.z);
        Vector3 dirA = (zeroVertPos - centre).normalized;
        if(dirA.magnitude == 0)
        {
            //Debug.Log("zeroVert");
            return vertPos;
        }
        float angle = 0;
        if (dirA.z >= 0) angle = Mathf.Acos(dirA.x) * Mathf.Rad2Deg;
        else if (dirA.z < 0) angle = 360 - (Mathf.Acos(dirA.x) * Mathf.Rad2Deg);
        int index = (angle != 0) ? (int)(angle / (360f / boundary.Count)) : 0;
        //Debug.Log(index);
        Vector3 originB = boundary[index];
        int j = (index + 1) % boundary.Count;
        Vector3 dirB = (originB - boundary[j]).normalized;

        Vector3 intersection = VectorIntersection.VectorIntersectionCheck(centre, dirA, originB, dirB);
        float dist = Vector3.Distance(zeroVertPos, intersection) / Vector3.Distance(intersection, centre);
        Vector3 lerpVal = Vector3.Lerp(zeroVertPos, vertPos, Mathf.Pow(dist, falloff));
        //Vector3 lerpVal = Vector3.Lerp(zeroVertPos, vertPos, 0.5f);
        return lerpVal;
    }

    private Vector3[] To1DArray(float[,] noiseMap, float xDimen, float yDimen, float minHeight, float maxHeight)
    {
        Vector3[] verts = new Vector3[noiseMap.GetLength(0) * noiseMap.GetLength(1)];
        int noiseWidth = noiseMap.GetLength(0);
        int noiseHeight = noiseMap.GetLength(1);

        float planeWidthIncr = xDimen / noiseMap.GetLength(0);
        float planeHeightIncr = yDimen / noiseMap.GetLength(1);

        // position the vertices
        for (int y = 0; y < noiseHeight; y++)
        {
            for (int x = 0; x < noiseWidth; x++)
            {
                verts[y * noiseWidth + x] = new Vector3(x * planeWidthIncr, Mathf.Lerp(minHeight, maxHeight, noiseMap[x, y]), y * planeHeightIncr);
            }
        }
        return verts;
    }

    // cut out island the vertices
    private List<Vector3> CutoutIsland(List<Vector3> boundary, Vector3[] verts)
    {
        List<Vector3> cutout = new List<Vector3>();
        foreach (Vector3 vec in verts)
        {
            if (BoundaryTest.CollisionTest(boundary, new Vector3(vec.x, 0, vec.z)))
            {
                cutout.Add(vec);
            }
        }
        return cutout;
    }


    //private int floatCompareTo(float a, float b)
    //{
    //    if (a < b) return -1;
    //    else if (a == b) return 0;
    //    else return 1;
    //}

    //private int floatCompareTo(float a, float b)
    //{
    //    return (int)(a - b);
    //}

    private void EliminateRedundantVerts(List<Vector3> verts, Vector3 centre, float checkRadius)
    {
        List<int> badVerts = new List<int>();
        //List<int> nearVerts = new List<int>();
        for (int i = 0; i < verts.Count; i++)
        {
            if (badVerts.Contains(i))
                continue;
            Vector3 position = new Vector3(verts[i].x, centre.y, verts[i].z);
            for (int j = 0; j < verts.Count; j++)
            {
                Vector3 planarPos = new Vector3(verts[j].x, centre.y, verts[j].z);
                if (i != j && Vector3.Distance(position, planarPos) < checkRadius)
                {
                    verts[i] = new Vector3(verts[i].x, centre.y, verts[i].z); 
                    //nearVerts.Add(j);
                    badVerts.Add(j);
                }
            }
        }

        badVerts.Sort();
        for (int i = badVerts.Count - 1; i >= 0; i--)
        {
            verts.RemoveAt(badVerts[i]);
        }
    }

    private void RemoveTrianglesAtCorners(List<Vector3> verts, List<int> triangles, Vector3 centre, float minRad)
    {
        for(int i = triangles.Count - 3; i >= 0; i -= 3)
        {
            Vector3 halfA = (verts[triangles[i]] + verts[triangles[i + 1]]) / 2;
            halfA.y = islandBoundary[0].y;
            Vector3 halfB = (verts[triangles[i + 1]] + verts[triangles[i + 2]]) / 2;
            halfB.y = islandBoundary[0].y;
            Vector3 halfC = (verts[triangles[i + 2]] + verts[triangles[i]]) / 2;
            halfC.y = islandBoundary[0].y;

            if ((centre - halfA).magnitude < minRad && ((centre - halfB).magnitude < minRad) && (centre - halfC).magnitude < minRad)
                continue;

            float tolerance = Tolerance;
            //bool halfAOut = BoundaryTest.CollisionTest(islandBoundary, halfA, tolerance);
            //bool halfBOut = BoundaryTest.CollisionTest(islandBoundary, halfB, tolerance);
            //bool halfCOut = BoundaryTest.CollisionTest(islandBoundary, halfC, tolerance);

            Vector3 Aintersect = VectorIntersection.BoundarySingleIntersectionCheck(islandBoundary, centre, (halfA - centre).normalized);
            float AintersectLen = (Aintersect - centre).magnitude;    float halfALen = (halfA - centre).magnitude;
            bool halfAIn = (halfALen < AintersectLen + tolerance) ? true : false;

            Vector3 Bintersect = VectorIntersection.BoundarySingleIntersectionCheck(islandBoundary, centre, (halfB - centre).normalized);
            float BintersectLen = (Bintersect - centre).magnitude; float halfBLen = (halfB - centre).magnitude;
            bool halfBIn = (halfBLen < BintersectLen + tolerance) ? true : false;

            Vector3 Cintersect = VectorIntersection.BoundarySingleIntersectionCheck(islandBoundary, centre, (halfC - centre).normalized);
            float CintersectLen = (Cintersect - centre).magnitude; float halfCLen = (halfC - centre).magnitude;
            bool halfCIn = (halfCLen < CintersectLen + tolerance) ? true : false;

            if (!halfAIn || !halfBIn || !halfCIn)
            {
                triangles.RemoveAt(i + 2); triangles.RemoveAt(i + 1); triangles.RemoveAt(i);
            }
        }
    }
    

    private void CloseVertsAtEdges(List<Vector3> verts, Vector3 centre, float minRad, float tolerance)
    {
        for(int i = 0; i < verts.Count; i++)
        {
            Vector3 vert = verts[i];
            vert.y = centre.y;

            if ((centre - vert).magnitude < minRad)
                continue;

            Vector3 Aintersect = VectorIntersection.BoundarySingleIntersectionCheck(islandBoundary, centre, (vert - centre).normalized);
            float AintersectLen = (Aintersect - centre).magnitude;    float vertLen = (vert - centre).magnitude;
            bool vertIn = (vertLen < AintersectLen + tolerance) ? true : false;

            if (!vertIn)
            {
                verts[i] = vert;
            }
        }
    }

    private List<Vector3> ShiftVertsToCentre(List<Vector3> verts, Vector3 centre)
    {
        for(int i = 0; i < verts.Count; i++)
        {
            Vector3 disp = verts[i] - centre;
            //disp.y = verts[i].y;
            verts[i] = disp;
        }
        return verts;
    }


    public List<int> JoinMeshes(List<int> upTris, List<int> downTris, int diff, out List<Vector3> uvSeam, out Dictionary<int, int> vertMap)
    {
        List<int> seam = new List<int>();
        List<IntEdge> upborder = FindBorder(upTris);
        List<IntEdge> downborder = FindBorder(downTris);

        foreach(IntEdge edge in upborder)
        {
            for(int i = downborder.Count - 1; i >= 0; i--)
            {
                if(edge.A == downborder[i].A && edge.B == downborder[i].B)
                {
                    seam.Add(edge.A); seam.Add(downborder[i].B + diff); seam.Add(edge.B);
                    seam.Add(downborder[i].B + diff); seam.Add(edge.A); seam.Add(downborder[i].A + diff); 
                    downborder.RemoveAt(i);
                }
                else if (edge.A == downborder[i].B && edge.B == downborder[i].A)
                {
                    seam.Add(edge.A); seam.Add(downborder[i].A + diff); seam.Add(edge.B);
                    seam.Add(downborder[i].A + diff); seam.Add(edge.A); seam.Add(downborder[i].B + diff);
                    downborder.RemoveAt(i);
                }
            }
        }

        uvSeam = new List<Vector3>();
        vertMap = new Dictionary<int, int>();
        CreateUVSeamVerts(upborder, seam, _cutoutTopVerts, uvSeam, diff * 2, vertMap);

        return seam;
    }

    public void CreateUVSeamVerts(List<IntEdge> edges, List<int> tris, List<Vector3> verts, List<Vector3> uvSeam, int counterStart, Dictionary<int, int> doneVerts)
    {
        int counter = counterStart;

        // generate the mapping and duplicate vertices
        for(int i = 0; i < edges.Count; i++)
        {
            if (!doneVerts.ContainsKey(edges[i].A))
            {
                doneVerts.Add(edges[i].A, counter);    // add key mapping to the dictionary
                uvSeam.Add(new Vector3(verts[edges[i].A].x, verts[edges[i].A].y, verts[edges[i].A].z));   // add new vertex copying the position
                counter++;
            }

            if (!doneVerts.ContainsKey(edges[i].B))
            {
                doneVerts.Add(edges[i].B, counter);    // add key mapping to the dictionary
                uvSeam.Add(new Vector3(verts[edges[i].B].x, verts[edges[i].B].y, verts[edges[i].B].z));   // add new vertex copying the position
                counter++;
            }
        }

        // loop through triangle and replace indexes
        for(int i = 0; i < tris.Count; i++)
        {
            if (doneVerts.ContainsKey(tris[i]))
                tris[i] = doneVerts[tris[i]];
        }
    }

    public List<IntEdge> FindBorder(List<int> triangles)
    {
        List<IntEdge> edges = new List<IntEdge>();
        //edges.Add(new IntEdge(-1,-1));
        for(int i = 0; i < triangles.Count; i += 3)
        {
            // check and add AB
            CheckAddRemove(edges, triangles[i], triangles[i + 1]);
            // check and add BC
            CheckAddRemove(edges, triangles[i + 1], triangles[i + 2]);
            // check and add CA
            CheckAddRemove(edges, triangles[i + 2], triangles[i]);
        }
        //edges.RemoveAt(0);
        //Debug.Log("edges count: " + edges.Count);
        return edges;
    }

    public void CheckAddRemove(List<IntEdge> edges, int a, int b)
    {
        // search for edge
        for(int j = edges.Count - 1; j >= 0; j--)
        {
            if (edges[j].A == a && edges[j].B == b)
            {
                edges.RemoveAt(j);
                return;
            }
            else if (edges[j].A == b && edges[j].B == a)
            {
                edges.RemoveAt(j);
                return;
            }
        }

        // add edge if it is not present
        edges.Add(new IntEdge(a, b));
    }

    public class IntEdge
    {
        public int A;
        public int B;
        //public int Score;

        public IntEdge(int a, int b)
        {
            A = a;
            B = b;
            //Score = 0;
        }
    }
}




#if (UNITY_EDITOR)

[CustomEditor(typeof(ActiveIslandMeshGen))]
public class ActiveIslandMeshGenEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        ActiveIslandMeshGen meshGen = (ActiveIslandMeshGen)target;
        //if (DrawDefaultInspector())
        //{
        //    if (meshGen.AutoUpdate) meshGen.GenerateMesh();
        //}
        DrawDefaultInspector();

        if (GUILayout.Button("Generate Map"))
        {
            meshGen.GenerateMesh(meshGen.Progress);
            meshGen.Progress += 1;
        }
    }
}
#endif