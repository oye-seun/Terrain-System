using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.Image;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class VectorIntersection : MonoBehaviour
{

    [SerializeField] private GameObject _boundaryPrefab;
    [SerializeField] private int _noOfBoundaryPoints;
    [SerializeField] private Vector3 _boundaryOrigin;
    [SerializeField] private float _boundaryRadius;
    [SerializeField] private Material _testCubeMat;
    //[SerializeField] private Material _boundaryMat;
    private GameObject[] _boundaries;
    private GameObject _testCube;
    private GameObject _intersectCube;
    private GameObject[] _intersectCubes;
    //private GameObject _midpointCube;

    // Start is called before the first frame update
    void Start()
    {
        _boundaries = new GameObject[_noOfBoundaryPoints];
        float angleDiv = 360f / _noOfBoundaryPoints;

        //CartesianPlane(new Vector2(0, 0), 15);
        for (int i = 0; i < _noOfBoundaryPoints; i++)
        {
            Vector3 dir = new Vector3(Mathf.Cos(i * angleDiv * Mathf.Deg2Rad), 0, Mathf.Sin(i * angleDiv * Mathf.Deg2Rad)).normalized;
            _boundaries[i] = Instantiate(_boundaryPrefab, new Vector3(_boundaryOrigin.x + (_boundaryRadius * dir.x), 0, _boundaryOrigin.z + (_boundaryRadius * dir.z)), Quaternion.identity);
        }

        _testCube = Instantiate(_boundaryPrefab, new Vector3(_boundaryOrigin.x, 0, _boundaryOrigin.y), Quaternion.identity);
        _intersectCube = Instantiate(_boundaryPrefab, new Vector3(_boundaryOrigin.x, 0, _boundaryOrigin.y), Quaternion.identity);
        //_midpointCube = Instantiate(_boundaryPrefab, new Vector3(_boundaryOrigin.x, 0, _boundaryOrigin.y), Quaternion.identity);
        _testCube.GetComponent<Renderer>().material = _testCubeMat;
        _intersectCube.GetComponent<Renderer>().material = _testCubeMat;
        _intersectCube.layer = 0;
        //_midpointCube.GetComponent<Renderer>().material = _testCubeMat;

        _intersectCubes = new GameObject[3];
        for(int i = 0; i < 3; i++)
        {
            _intersectCubes[i] = Instantiate(_boundaryPrefab, new Vector3(_boundaryOrigin.x, 0, _boundaryOrigin.y), Quaternion.identity);
            _intersectCubes[i].GetComponent<Renderer>().material = _testCubeMat;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dirA = (_testCube.transform.position - _boundaryOrigin).normalized;
        float angle = 0;
        if (dirA.z > 0) angle = Mathf.Acos(dirA.x)* Mathf.Rad2Deg;
        else if(dirA.z < 0) angle = 360-(Mathf.Acos(dirA.x) * Mathf.Rad2Deg);
        int index = (angle!= 0)? (int)(angle / (360f / _noOfBoundaryPoints)) : 0;
        //Debug.Log(index);
        Vector3 originB = _boundaries[index].transform.position;
        int j = (index + 1) % _noOfBoundaryPoints;
        Vector3 dirB = (originB - _boundaries[j].transform.position).normalized;

        _intersectCube.transform.position = VectorIntersectionCheck(_boundaryOrigin, dirA, originB, dirB);
        DrawLines();

        Vector3[] boundaries = new Vector3[_boundaries.Length];
        for(int i = 0; i < _boundaries.Length; i++) boundaries[i] = _boundaries[i].transform.position;

        //List<Vector3> intersections = BoundaryIntersectionCheck(boundaries, _testCube.transform.position, new Vector3(0, 0, -1));
        //for(int i = 0; i < intersections.Count; i++)
        //{
        //    _intersectCubes[i].transform.position = intersections[i];
        //}
        
        Vector3 intersection = BoundarySingleIntersectionCheck(boundaries, _testCube.transform.position, new Vector3(0, 0, -1));
            _intersectCubes[0].transform.position = intersection;


        //// test line distance to point
        //float dist = DistOfPointToLine2D(
        //    new Vector2(_boundaries[0].transform.position.x, _boundaries[0].transform.position.z),
        //    new Vector2(_boundaries[1].transform.position.x, _boundaries[1].transform.position.z),
        //    new Vector2(_testCube.transform.position.x, _testCube.transform.position.z));
        //Debug.Log("Point Dist to Line: " + dist);
    }

    private void DrawLines()
    {
        Debug.DrawLine(_boundaryOrigin, _testCube.transform.position, Color.blue);
        for(int i = 0; i < _noOfBoundaryPoints; i++)
        {
            int j = (i + 1) % _noOfBoundaryPoints;
            Debug.DrawLine(_boundaries[i].transform.position, _boundaries[j].transform.position, Color.red);
        }
    }

    public static Vector3 VectorIntersectionCheck(Vector3 originA, Vector3 dirA, Vector3 originB, Vector3 dirB)
    {
        Vector3 intersectPos = Vector3.zero;
        float intersectLen = ((originA.x * dirA.z) - (originA.z * dirA.x) - (originB.x * dirA.z) + (originB.z * dirA.x)) / ((dirB.x * dirA.z) - (dirB.z * dirA.x));
        //Debug.Log(intersectLen);
        intersectPos = originB + (dirB * intersectLen);
        return intersectPos;
    }

    public static Vector3 VectorIntersectionCheck(Vector2 originA, Vector2 dirA, Vector2 originB, Vector2 dirB)
    {
        Vector2 intersectPos = Vector2.negativeInfinity;
        float intersectLen = ((originA.x * dirA.y) - (originA.y * dirA.x) - (originB.x * dirA.y) + (originB.y * dirA.x)) / ((dirB.x * dirA.y) - (dirB.y * dirA.x));
        //Debug.Log(intersectLen);
        intersectPos = originB + (dirB * intersectLen);
        return intersectPos;
    }

    public static List<Vector3> BoundaryIntersectionCheck(IEnumerable<Vector3> boundary, Vector3 originA, Vector3 dirA)
    {
        List<Vector3> intersections = new List<Vector3>();
        for(int i = 0; i < boundary.Count(); i++)
        {
            int j = (i+1) % boundary.Count();
            Vector3 dir = (boundary.ElementAt(i) - boundary.ElementAt(j)).normalized;
            Vector3 intersection = VectorIntersectionCheck(originA, dirA, boundary.ElementAt(i), dir);
            Vector3 dir2 = (boundary.ElementAt(i) - intersection).normalized;
            if (Vector3.Dot(dir, dir2) > 0.99f && (boundary.ElementAt(i) - intersection).sqrMagnitude < (boundary.ElementAt(i) - boundary.ElementAt(j)).sqrMagnitude)
            {
                intersections.Add(intersection);
            }
        }
        return intersections;
    }

    public static Vector3 BoundarySingleIntersectionCheck(IEnumerable<Vector3> boundary, Vector3 originA, Vector3 dirA)
    {
        Vector3 intersection = Vector3.negativeInfinity;
        for (int i = 0; i < boundary.Count(); i++)
        {
            int j = (i + 1) % boundary.Count();
            Vector3 dir = (boundary.ElementAt(i) - boundary.ElementAt(j)).normalized;
            intersection = VectorIntersectionCheck(originA, dirA, boundary.ElementAt(i), dir);

            Vector3 dir2 = (boundary.ElementAt(i) - intersection).normalized;
            //bool withinRange = (originA - intersection).sqrMagnitude < (length * length);
            Vector3 dir3 = (intersection - originA).normalized;
            bool rightDir = (Vector3.Dot(dirA, dir3) > 0.99f) ? true : false;
            bool withinRange = (boundary.ElementAt(i) - boundary.ElementAt(j)).sqrMagnitude > (boundary.ElementAt(i) - intersection).sqrMagnitude;

            if (Vector3.Dot(dir, dir2) > 0.99f && (boundary.ElementAt(i) - intersection).sqrMagnitude < (boundary.ElementAt(i) - boundary.ElementAt(j)).sqrMagnitude && withinRange && rightDir)
            {
                return intersection;
            }
        }
        return intersection;
    }

    // do not use normalized dirA
    // try to use with uniform Y values of originA, dirA and boundary
    public static Vector3 BoundarySingleIntersectionCheckNew(IEnumerable<Vector3> boundary, Vector3 originA, Vector3 dirA)
    {
        originA.y = dirA.y = boundary.ElementAt(0).y;
        Vector3 intersection = Vector3.negativeInfinity;
        for (int i = 0; i < boundary.Count(); i++)
        {
            int j = (i + 1) % boundary.Count();
            Vector3 dir = (boundary.ElementAt(i) - boundary.ElementAt(j)).normalized;
            intersection = VectorIntersectionCheck(boundary.ElementAt(i), dir, originA, dirA.normalized);

            Vector3 dir3 = (originA - intersection);
            bool rightDir = (Vector3.Dot(dirA.normalized, dir3.normalized) > 0.99f) ? true : false;
            bool withinRange = dirA.sqrMagnitude >= (dir3.sqrMagnitude - 0.001f);
            bool withinRange2 = (boundary.ElementAt(i) - boundary.ElementAt(j)).sqrMagnitude >= ((boundary.ElementAt(i) - intersection).sqrMagnitude - 0.001f);

            if (rightDir && withinRange && withinRange2)
            {
                //Debug.Log("within");
                return intersection;
            }
        }
        //Debug.Log("not within");
        return intersection;
    }

    /// <summary>
    /// Gets shortest distance between a point and a line. note that the Line extends to infinity
    /// </summary>
    /// <param name="lineP1"></param>
    /// <param name="lineP2"></param>
    /// <param name="testPos"></param>
    /// <returns></returns>
    public static float DistOfPointToLine2D(Vector2 lineP1, Vector2 lineP2, Vector2 testPos)
    {
        return (((lineP2.x - lineP1.x) * (lineP1.y - testPos.y)) - ((lineP1.x - testPos.x) * (lineP2.y - lineP1.y))) / (lineP2 - lineP1).magnitude;
    }

    public static Vector2 RotateDirVectorBy(Vector2 dir, float degree)
    {
        float angle = 0;
        if (dir.y >= 0) angle = Mathf.Acos(dir.x) * Mathf.Rad2Deg;
        else if (dir.y < 0) angle = 360 - (Mathf.Acos(dir.x) * Mathf.Rad2Deg);
        angle += degree;
        angle = (angle < 0)?  360 - angle : angle %= 360;
        Vector2 newDir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
        return newDir;
    }
}
