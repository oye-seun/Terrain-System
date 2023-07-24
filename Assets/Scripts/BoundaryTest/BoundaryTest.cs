//using System.Collections;
//using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//using static UnityEngine.UI.Image;
//using UnityEngine.UIElements;

public class BoundaryTest : MonoBehaviour
{
    [SerializeField] private GameObject _boundaryPrefab;
    [SerializeField] private int _noOfBoundaryPoints;
    [SerializeField] private Vector2 _boundaryOrigin;
    [SerializeField] private float _boundaryRadius;
    [SerializeField] private Material _testCubeMat;
    [SerializeField] private Material _boundaryMat;
    private GameObject[] _boundaries;
    private GameObject _testCube;
    private Triangle triangle;
    //private GameObject _midpointCube;

    // Start is called before the first frame update
    void Start()
    {
        _boundaries = new GameObject[_noOfBoundaryPoints];
        float angleDiv = 360f / _noOfBoundaryPoints;
        
        //CartesianPlane(new Vector2(0, 0), 15);
        for(int i = 0; i < _noOfBoundaryPoints; i++)
        {
            Vector3 dir = new Vector3(Mathf.Cos(i * angleDiv * Mathf.Deg2Rad), 0, Mathf.Sin(i * angleDiv * Mathf.Deg2Rad)).normalized;
            _boundaries[i] = Instantiate(_boundaryPrefab, new Vector3(_boundaryOrigin.x + (_boundaryRadius * dir.x), 0, _boundaryOrigin.y + (_boundaryRadius * dir.z)), Quaternion.identity);
        }

        triangle = new Triangle();

        _testCube = Instantiate(_boundaryPrefab, new Vector3(_boundaryOrigin.x, 0, _boundaryOrigin.y), Quaternion.identity);
        //_midpointCube = Instantiate(_boundaryPrefab, new Vector3(_boundaryOrigin.x, 0, _boundaryOrigin.y), Quaternion.identity);
        _testCube.GetComponent<Renderer>().material = _testCubeMat;
        //_midpointCube.GetComponent<Renderer>().material = _testCubeMat;
    }

    // Update is called once per frame
    void Update()
    {
        CartesianPlane(new Vector2(0, 0), 15);
        DrawBoundary();
        DoCollisionTest();
        //GetMidpoint();
    }

    // set up cartesian plane
    private void CartesianPlane(Vector2 origin, int length)
    {
        // z axis
        for(int i = 0; i < length; i++) 
        {
            Debug.DrawLine(new Vector3(origin.x, 0, origin.y + i), new Vector3(origin.x + length, 0, origin.y + i), Color.green);
        }

        // x axis
        for (int i = 0; i < length; i++)
        {
            Debug.DrawLine(new Vector3(origin.x + i, 0, origin.y), new Vector3(origin.x + i, 0, origin.y + length), Color.green);
        }
    }

    private void DrawBoundary()
    {
        for (int i = 0; i < _boundaries.Length; i++)
        {
            int j = (i + 1) % _boundaries.Length;
            i %= _boundaries.Length;
            Debug.DrawLine(_boundaries[i].transform.position, _boundaries[j].transform.position, Color.red);
        }

        triangle.A = _boundaries[0].transform.position;
        triangle.B = _boundaries[1].transform.position;
        triangle.C = _boundaries[2].transform.position;
        Vector3 centre = triangle.CircumCentre();
        Debug.Log("centre: " + centre);
        float radius = (centre - triangle.A).magnitude;
        for (int i = 0; i < 360; i++)
        {
            Vector3 dir = new Vector3(Mathf.Cos(i * Mathf.Deg2Rad), 0, Mathf.Sin(i * Mathf.Deg2Rad)).normalized;
            int j = (i + 1) % 360;
            Vector3 dirPlus1 = new Vector3(Mathf.Cos(j * Mathf.Deg2Rad), 0, Mathf.Sin(j * Mathf.Deg2Rad)).normalized;
            Debug.DrawLine(centre + (dir * radius), centre + (dirPlus1 * radius), Color.blue);
        }

        //triangle.A = _boundaries[0].transform.position;
        //triangle.B = _boundaries[1].transform.position;
        //triangle.C = _boundaries[2].transform.position;
        //Vector2 centre2D = triangle.CircumCentre2D(new Vector2(triangle.A.x, triangle.A.z), new Vector2(triangle.B.x, triangle.B.z), new Vector2(triangle.C.x, triangle.C.z));
        //Vector3 centre = new Vector3(centre2D.x, 0, centre2D.y);
        //Debug.Log("centre: " + centre);
        //float radius = (centre - triangle.A).magnitude;
        //for(int i = 0; i < 360; i++)
        //{
        //    Vector3 dir = new Vector3(Mathf.Cos(i * Mathf.Deg2Rad), 0, Mathf.Sin(i* Mathf.Deg2Rad)).normalized;
        //    int j = (i + 1) % 360;
        //    Vector3 dirPlus1 = new Vector3(Mathf.Cos(j * Mathf.Deg2Rad), 0, Mathf.Sin(j * Mathf.Deg2Rad)).normalized;
        //    Debug.DrawLine(centre + (dir*radius), centre + (dirPlus1*radius), Color.blue);
        //}

    }

    private void DoCollisionTest()
    {
        List<Vector3> boundaryArray = new List<Vector3>();

        // update boundary array
        for (int i = 0; i < _boundaries.Length; i++)
        {
            boundaryArray.Add(_boundaries[i].transform.position);
        }

        //if (CollisionTest(boundaryArray, _testCube.transform.position)) _testCubeMat.color = Color.red;
        if (WindingCountBoundaryTest.CheckWithinBoundary(_testCube.transform.position, boundaryArray, WindingCountBoundaryTest.Plane.X_Z))
        {
            _testCubeMat.color = Color.red;
        }
        else _testCubeMat.color = Color.cyan;
    }


    public static bool CollisionTest(List<Vector3> boundaryArr, Vector3 testPos, float tolerance = 0.5f)
    {
        List<Vector3> boundaryArray = new List<Vector3>();
        boundaryArray.AddRange(boundaryArr);


        //// update boundary array
        //for (int i = 0; i < _boundaries.Length; i++)
        //{
        //    boundaryArray.Add(_boundaries[i].transform.position);
        //}

        bool isWithinBoundary = false;
        bool hasReflex = true;
        // check for internal reflex angles
        while (hasReflex)
        {
            int i = 0;
            int count = 0;
            for (i = 0; i < boundaryArray.Count; i++)
            {
                int preIndex = (i - 1 < 0) ? boundaryArray.Count - 1 : i - 1;
                int postIndex = (i + 1) % boundaryArray.Count;
                Vector3 normal = Vector3.Cross(boundaryArray[preIndex] - boundaryArray[i], boundaryArray[i] - boundaryArray[postIndex]).normalized;
                if (normal.y > 0)  // reflex angle
                {
                    count++;
                    break;
                }
            }
            if(i == boundaryArray.Count)
            {
                isWithinBoundary = PolygonCollisionTest(testPos, boundaryArray.ToArray(), tolerance);
                return isWithinBoundary;
            }

            // check forward Dir for chain reflex angles
            int forwd = GetChainReflexBoundaries(i, 1, boundaryArray);

            // check reverse Dir for chain reflex angles
            int backd = GetChainReflexBoundaries(i, -1, boundaryArray);

            // generate list of chained reflex boundaries
            if(forwd > 0 && backd > 0)
            {
                //Debug.Log($"current: {i}, baackward: {backd}, forward: {forwd}");
                List<Vector3> chainReflBound = new List<Vector3>();
                List<int> reflexIndexes = new List<int>();

                //int lastObtuseVert = SubtractModulo(i, backd, boundaryArray.Count);
                //chainReflBound.Add(boundaryArray[lastObtuseVert]);
                for (int j = 0; j < backd; j++)
                {
                    int vert = CyclicSummation(i, -(backd - j), boundaryArray.Count);
                    chainReflBound.Add(boundaryArray[vert]);
                    reflexIndexes.Add(vert);
                }
                for(int j = 0; j < forwd + 1; j++)
                {
                    int vert = (i + j) % boundaryArray.Count;
                    chainReflBound.Add(boundaryArray[vert]);
                    reflexIndexes.Add(vert);
                }

                bool outsideTris = PolygonCollisionTest(testPos, chainReflBound.ToArray(), tolerance);
                if(outsideTris) return false;

                reflexIndexes.RemoveAt(reflexIndexes.Count -1);
                reflexIndexes.RemoveAt(0);

                reflexIndexes.Sort();

                for (int k = reflexIndexes.Count - 1; k >= 0; k--)
                {
                    boundaryArray.RemoveAt(reflexIndexes[k]);
                }

                //for (int z = 0; z < boundaryArray.Count; z++)
                //{
                //    int j = (z + 1) % boundaryArray.Count;
                //    z %= boundaryArray.Count;
                //    Debug.DrawLine(boundaryArray[z], boundaryArray[j], Color.blue);
                //}

            }

            //if (forwd > 0 && backd > 0) _boundaries[i].GetComponent<Renderer>().material = _testCubeMat;
            //else _boundaries[i].GetComponent<Renderer>().material = _boundaryMat;

            if (count == 0) hasReflex = false;
        }

        // final polygon test
        isWithinBoundary = PolygonCollisionTest(testPos, boundaryArray.ToArray(), tolerance);

        //Debug.Log(angle);
        return isWithinBoundary;
    }

    // make subtracting and adding cyclic in a range of numbers
    public static int CyclicSummation(int val, int sum, int arrayCount)
    {
        int newVal = (val + sum < 0) ? arrayCount - (Mathf.Abs(sum + val) % arrayCount) : val + sum;
        return newVal;
    }

    public static int GetChainReflexBoundaries(int index, int dir, List<Vector3> boundaries)
    {
        int count = 0;
        int preIndex = (index - 1 < 0)? boundaries.Count -1 : index - 1;
        int postIndex = (index + 1) % boundaries.Count;

        int nextIndex = (index + dir < 0)? boundaries.Count - 1 : index + dir;
        nextIndex %= boundaries.Count;

        Vector3 normal = Vector3.Cross(boundaries[preIndex] - boundaries[index], boundaries[index] - boundaries[postIndex]).normalized;
        if (normal.y > 0)  // reflex angle
        {
            count++;
            count += GetChainReflexBoundaries(nextIndex, dir, boundaries);
        }
        return count;
    }


    public static float AngleBtw(Vector3 pos1, Vector3 pos2, Vector3 pos3)
    {
        Vector3 dir1 = pos2 - pos1;
        Vector3 dir2 = pos2 - pos3;
        return Vector3.Angle(dir1, dir2);
    }

    public static bool PolygonCollisionTest(Vector3 pos, Vector3[] boundary, float tolerance = 0.5f)
    {
        bool isWithin = false;
        float angle = 0;
        for (int i = 0; i < boundary.Length; i++)
        {
            int j = (i + 1) % boundary.Length;
            i %= boundary.Length;
            angle += AngleBtw(boundary[i], pos, boundary[j]);
        }

        //Debug.Log(angle);
        if (angle > (360 -tolerance) && angle < (360 + tolerance)) isWithin = true;
        return isWithin;
    }


    //private void GetMidpoint()
    //{
    //    Vector3 midpoint = Vector3.zero;
    //    for(int i = 0; i < _boundaries.Length; i++)
    //    {
    //        midpoint.x += _boundaries[i].transform.position.x;
    //        midpoint.z += _boundaries[i].transform.position.z;
    //    }
    //    midpoint /= _boundaries.Length;
    //    _midpointCube.transform.position = midpoint;
    //}

    
}
