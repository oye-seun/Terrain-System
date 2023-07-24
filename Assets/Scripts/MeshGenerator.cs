using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class MeshGenerator : MonoBehaviour
{
    [SerializeField] private Vector3 _dimensions;
    [SerializeField] private float _minHeight;
    [SerializeField] private float _maxHeight;
    [SerializeField] private float _gizmoSphereRadius;
    [SerializeField] private float _topScaleFactor = 0.5f;
    [SerializeField] private float _topEdgeFalloff = 3f;

    private Mesh _mesh;
    private Vector3[] _finalVerts;
    private int[] _finalTris;
    private Vector2[] _uvs;
    private List<Vector3> islandBoundary;
    private List<Vector3> verts;
    private Vector3[] verts2;

    // Start is called before the first frame update
    void Start()
    {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateMesh(float[,] noiseMap)
    {
        IslandShape islandShape = new IslandShape(30, 35, 5, _dimensions/2, 5, new Vector3(0,0,0));
        islandBoundary = islandShape.GenerateBoundary();
        //Debug.Log(islandBoundary.Count);


        _finalVerts = new Vector3[noiseMap.GetLength(0) * noiseMap.GetLength(1)];
        int noiseWidth = noiseMap.GetLength(0);
        int noiseHeight = noiseMap.GetLength(1);

        float planeWidthIncr = _dimensions.x / noiseMap.GetLength(0);
        float planeHeightIncr = _dimensions.z / noiseMap.GetLength(1);

        // position the vertices
        for (int y = 0; y < noiseHeight; y++)
        {
            for (int x = 0; x < noiseWidth; x++)
            {
                _finalVerts[y * noiseWidth + x] = new Vector3( x * planeWidthIncr, Mathf.Lerp(_minHeight,_maxHeight, noiseMap[x, y]), y * planeHeightIncr);
            }
        }

        verts = new List<Vector3>();
        // cut out island the vertices
        foreach (Vector3 vec in _finalVerts)
        {
            if (BoundaryTest.CollisionTest(islandBoundary, new Vector3(vec.x, 0, vec.z)))
            {
                verts.Add(vec);
            }
        }

        verts2 = verts.ToArray();
        //foreach (Vector3 vec in islandBoundary)
        //{
        //    for(int i = 0; i < verts2.Length; i++)
        //    {
        //        verts2[i] = ProportionalEdit(vec, verts2[i], 15, new Vector3(0, 1, 0), new Vector3(1,0,1));
        //    }
        //}

        // scale down map
        for (int i = 0; i < verts2.Length; i++)
        {
            Vector3 baseHeight = new Vector3(verts2[i].x, (_dimensions / 2).y, verts2[i].z);
            verts2[i] = Vector3.Lerp(baseHeight, verts2[i], _topScaleFactor);
        }

        // blend everything to baseHeight at the edges
        for (int i = 0; i < verts2.Length; i++)
        {
            verts2[i] = SmoothenEdges(islandBoundary, _dimensions / 2, verts2[i], _topEdgeFalloff);
        }


        // update the mesh
        _mesh.Clear();
        _mesh.vertices = _finalVerts;
        _mesh.triangles = _finalTris;
        _mesh.uv = _uvs;

    }

    private void OnDrawGizmos()
    {
        foreach(Vector3 vec in verts2)
        {
            Gizmos.DrawSphere(vec, _gizmoSphereRadius);
        }

        //Debug.Log(islandBoundary.Count);
        //for (int i = 0; i < islandBoundary.Count; i++)
        //{
        //    int j = (i + 1)%islandBoundary.Count;
        //    Gizmos.DrawLine(islandBoundary[i], islandBoundary[j]);
        //    //Gizmos.DrawLine(islandBoundary[i], _dimensions/2);
        //    //Gizmos.DrawCube(islandBoundary[i], new Vector3(_gizmoSphereRadius, _gizmoSphereRadius, _gizmoSphereRadius));
        //}
    }

    private Vector3 ProportionalEdit(Vector3 centrePos, Vector3 targetPos, float radius, Vector3 controlAxis, Vector3 consideredAxis)
    {
        Vector3 newPos = Vector3.zero;
        Vector3 newTargetPos = new Vector3(targetPos.x * consideredAxis.x, targetPos.y * consideredAxis.y, targetPos.z * consideredAxis.z);
        float dist = Vector3.Distance(centrePos, newTargetPos);
        //Debug.Log(dist);
        if (dist > radius) return targetPos;

        float weight = Mathf.InverseLerp(0, radius, dist);
        Vector3 lerpVal = Vector3.Lerp(centrePos, targetPos, weight);
        //lerpVal = new Vector3(lerpVal.x * controlAxis.x, lerpVal.y * controlAxis.y,  lerpVal.z * controlAxis.z);
        newPos.x = (controlAxis.x == 0)? targetPos.x : lerpVal.x;
        newPos.y = (controlAxis.y == 0)? targetPos.y : lerpVal.y;
        newPos.z = (controlAxis.z == 0)? targetPos.z : lerpVal.z;
        return newPos;
    }

    private Vector3 SmoothenEdges(List<Vector3> boundary, Vector3 centre, Vector3 vertPos, float falloff)
    {
        Vector3 zeroVertPos = new Vector3(vertPos.x, centre.y, vertPos.z);
        Vector3 dirA = (zeroVertPos - centre).normalized;
        float angle = 0;
        if (dirA.z >= 0) angle = Mathf.Acos(dirA.x) * Mathf.Rad2Deg;
        else if (dirA.z < 0) angle = 360 - (Mathf.Acos(dirA.x) * Mathf.Rad2Deg);
        int index = (angle != 0) ? (int)(angle / (360f / boundary.Count)) : 0;
        //Debug.Log(index);
        Vector3 originB = boundary[index];
        int j = (index + 1) % boundary.Count;
        Vector3 dirB = (originB - boundary[j]).normalized;
        
        Vector3 intersection = VectorIntersection.VectorIntersectionCheck(centre, dirA, originB, dirB);
        Vector3 lerpVal = Vector3.Lerp(zeroVertPos, vertPos, Vector3.Distance(zeroVertPos, intersection) / (Vector3.Distance(intersection, centre) / falloff));
        //Vector3 lerpVal = Vector3.Lerp(zeroVertPos, vertPos, 0.5f);
        return lerpVal;
    }
}
