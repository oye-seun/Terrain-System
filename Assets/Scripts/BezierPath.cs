using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BezierPath
{
    public Vector3 Start;
    public Vector3 FirstHandle;
    public List<ControlPoint> Path;
    public Vector3 LastHandle;
    public Vector3 End; 
    public bool ShowHandles;
    public bool ShowNormals;
    public int Segments;
    public BoundingBox[] BoundingBoxes;

    BezierPath(Vector3 start, Vector3 firstHandle, Vector3 lastHandle, Vector3 end)
    {
        Start = start;
        FirstHandle = firstHandle;
        LastHandle = lastHandle;
        End = end;

        Path = new List<ControlPoint>();
    }

    public void AddIntermediate(ControlPoint intermediate)
    {
        Path.Add(intermediate);
    }

    public void GenerateVerts(List<Vector3> verts, float halfWidth, int vertsPerSegment)
    {
        int lastIndex = Path.Count - 1;
        if (lastIndex >= 0)
        {
            GenerateVertsFromCurve(verts, Start, FirstHandle, Path[0].A, Path[0].Centre, halfWidth, vertsPerSegment);
            for (int i = 1; i < Path.Count; i++)
            {
                GenerateVertsFromCurve(verts, Path[i - 1].Centre, Path[i - 1].B, Path[i].A, Path[i].Centre, halfWidth, vertsPerSegment);
            }
            GenerateVertsFromCurve(verts, Path[lastIndex].Centre, Path[lastIndex].B, LastHandle, End, halfWidth, vertsPerSegment);
        }
        else
        {
            GenerateVertsFromCurve(verts, Start, FirstHandle, LastHandle, End, halfWidth, vertsPerSegment);
        }
    }

    private void GenerateVertsFromCurve(List<Vector3> verts, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float halfWidth, int vertsPerSegment)
    {
        for (int i = 0; i < Segments + 1; i++)
        {
            Vector3 pos = BezierPosition(p0, p1, p2, p3, (float)i / Segments);
            Vector3 norm = BezierNormal(p0, p1, p2, p3, (float)i / Segments);
            Vector3 normStart = pos + (norm * halfWidth);
            Vector3 normEnd = pos - (norm * halfWidth);

            for (int j = 0; j < vertsPerSegment; j++)
            {
                verts.Add(Vector3.Lerp(normStart, normEnd, (float)j / (vertsPerSegment - 1)));
            }
        }
    }

    public BoundingBox GenerateBoundingBox(float halfWidth)
    {
        int lastIndex = Path.Count - 1;
        if (lastIndex < 0)
        {
            BoundingBoxes = new BoundingBox[1];
            BoundingBoxes[0] = GetBoundingBox(Start, FirstHandle, LastHandle, End, halfWidth);
            return BoundingBoxes[0];
        }
        else
        {
            BoundingBoxes = new BoundingBox[Path.Count + 1];
            BoundingBoxes[0] = GetBoundingBox(Start, FirstHandle, Path[0].A, Path[0].Centre, halfWidth);
            BoundingBox boundBox = new BoundingBox(Vector3.positiveInfinity, Vector3.negativeInfinity);
            BoundingBox.CompareBoundingBoxAndSetExtreme(boundBox, BoundingBoxes[0]);
            for (int i = 1; i < Path.Count; i++)
            {
                BoundingBoxes[i] = GetBoundingBox(Path[i - 1].Centre, Path[i - 1].B, Path[i].A, Path[i].Centre, halfWidth);
                BoundingBox.CompareBoundingBoxAndSetExtreme(boundBox, BoundingBoxes[i]);
            }
            BoundingBoxes[lastIndex + 1] = GetBoundingBox(Path[lastIndex].Centre, Path[lastIndex].B, LastHandle, End, halfWidth);
            BoundingBox.CompareBoundingBoxAndSetExtreme(boundBox, BoundingBoxes[lastIndex + 1]);
            return boundBox;
        }

    }


    public static BoundingBox GetBoundingBox(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float halfWidth)
    {
        // coefficients of t in quadratic equation
        Vector3 a = -(3 * p0) + (9 * p1) - (9 * p2) + (3 * p3);
        Vector3 b = (6 * p0) - (12 * p1) + (6 * p2);
        Vector3 c = -(3 * p0) + (3 * p1);

        // solve the quadratic equation
        Vector2 xSoln = QuadraticSolver(a.x, b.x, c.x);
        Vector2 ySoln = QuadraticSolver(a.y, b.y, c.y);
        Vector2 zSoln = QuadraticSolver(a.z, b.z, c.z);

        List<Vector3> values = new List<Vector3>();

        ValidateSolnAndAdd(values, 0, p0, p1, p2, p3, halfWidth);
        ValidateSolnAndAdd(values, 1, p0, p1, p2, p3, halfWidth);

        ValidateSolnAndAdd(values, xSoln.x, p0, p1, p2, p3, halfWidth);
        ValidateSolnAndAdd(values, xSoln.y, p0, p1, p2, p3, halfWidth);
        ValidateSolnAndAdd(values, ySoln.x, p0, p1, p2, p3, halfWidth);
        ValidateSolnAndAdd(values, ySoln.y, p0, p1, p2, p3, halfWidth);
        ValidateSolnAndAdd(values, zSoln.x, p0, p1, p2, p3, halfWidth);
        ValidateSolnAndAdd(values, zSoln.y, p0, p1, p2, p3, halfWidth);

        // find the minimum and maximum values
        Vector3 minValues = Vector3.positiveInfinity;
        Vector3 maxValues = Vector3.negativeInfinity;

        for (int i = 0; i < values.Count; i++)
        {
            if (values[i].x < minValues.x) minValues.x = values[i].x;
            if (values[i].y < minValues.y) minValues.y = values[i].y;
            if (values[i].z < minValues.z) minValues.z = values[i].z;

            if (values[i].x > maxValues.x) maxValues.x = values[i].x;
            if (values[i].y > maxValues.y) maxValues.y = values[i].y;
            if (values[i].z > maxValues.z) maxValues.z = values[i].z;
        }

        return new BoundingBox(minValues, maxValues);
    }

    public bool CheckPosWithinWidthX_Z(Vector3 pos, float halfWidth)
    {
        bool within = false;
        // first check for curve containing pos
        List<int> indexes = new List<int>();
        for(int i = 0; i < BoundingBoxes.Length; i++)
        {
            if (WindingCountBoundaryTest.CheckWithinBoundary2D(new Vector2(pos.x, pos.z), new Vector2[4]
            {
                new Vector2(BoundingBoxes[i].max.x, BoundingBoxes[i].min.z),
                new Vector2(BoundingBoxes[i].max.x, BoundingBoxes[i].max.z),
                new Vector2(BoundingBoxes[i].min.x, BoundingBoxes[i].max.z),
                new Vector2(BoundingBoxes[i].min.x, BoundingBoxes[i].min.z)
            }))
            {
                // point is within box
                indexes.Add(i);
                //index = i;
            }
        }

        for (int i = 0; i < indexes.Count && !within; i++)
        {
            int index = indexes[i];
            // check curve containing pos
            if (index >= 0 && index < BoundingBoxes.Length)
            {
                if (index == 0 && BoundingBoxes.Length == 1)
                {
                    within = CheckPosWithinCurveX_Z(pos, Start, FirstHandle, LastHandle, End, halfWidth);
                }
                else if (index == 0)
                {
                    within = CheckPosWithinCurveX_Z(pos, Start, FirstHandle, Path[0].A, Path[0].Centre, halfWidth);
                }
                else if (index == BoundingBoxes.Length - 1)
                {
                    within = CheckPosWithinCurveX_Z(pos, Path[Path.Count - 1].Centre, Path[Path.Count - 1].B, LastHandle, End, halfWidth);
                }
                else
                {
                    within = CheckPosWithinCurveX_Z(pos, Path[index - 1].Centre, Path[index - 1].B, Path[index].A, Path[index].Centre, halfWidth);
                }
            }
        }
        return within;
    }

    private bool CheckPosWithinCurveX_Z(Vector3 point, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float halfWidth)
    {
        bool within = false;
        for (int i = 0; i < Segments && !within; i++)
        {
            Vector3 pos = BezierPosition(p0, p1, p2, p3, (float)i / Segments);
            Vector3 norm = BezierNormal(p0, p1, p2, p3, (float)i / Segments);
            Vector3 A = pos + (norm * halfWidth);
            Vector3 D = pos - (norm * halfWidth);

            Vector3 pos1 = BezierPosition(p0, p1, p2, p3, (float)(i + 1) / Segments);
            Vector3 norm1 = BezierNormal(p0, p1, p2, p3, (float)(i + 1) / Segments);
            Vector3 B = pos1 + (norm1 * halfWidth);
            Vector3 C = pos1 - (norm1 * halfWidth);

            within = WindingCountBoundaryTest.CheckWithinBoundary2D(new Vector2(point.x, point.z), new Vector2[4]
            {
                new Vector2(A.x, A.z),
                new Vector2(B.x, B.z),
                new Vector2(C.x, C.z),
                new Vector2(D.x, D.z)
            });
        }
        return within;
    }


    private static void ValidateSolnAndAdd(List<Vector3> values, float soln, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float halfWidth)
    {
        if (soln >= 0 && soln <= 1)
        {
            // add the pos
            Vector3 pos = BezierPosition(p0, p1, p2, p3, soln);
            values.Add(pos);

            // add the pos + normal offsets
            Vector3 norm = BezierNormal(p0, p1, p2, p3, soln);
            values.Add(pos + (norm * halfWidth));
            values.Add(pos - (norm * halfWidth));
        }
    }

    public static Vector2 QuadraticSolver(float a, float b, float c)
    {
        Vector2 soln = Vector2.negativeInfinity;
        float Discr = (b * b) - (4 * a * c);
        if (Discr >= 0)
        {
            soln.x = (-b + Mathf.Sqrt(Discr)) / (2 * a);
            soln.y = (-b - Mathf.Sqrt(Discr)) / (2 * a);
        }
        return soln;
    }

    public static Vector3 BezierPosition(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        Vector3 pos = (Mathf.Pow(1 - t, 3) * p0) + (3 * Mathf.Pow(1 - t, 2) * t * p1) + (3 * (1 - t) * t * t * p2) + (t * t * t * p3);
        return pos;
    }
    public static Vector3 BezierNormal(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        Vector3 norm = (3 * Mathf.Pow(1 - t, 2) * (p1 - p0)) + (6 * (1 - t) * t * (p2 - p1)) + (3 * t * t * (p3 - p2));
        float turnAngle = 90;
        float cos = Mathf.Cos(turnAngle * Mathf.Deg2Rad);
        float sin = Mathf.Sin(turnAngle * Mathf.Deg2Rad);

        Matrix3x3 rotationMat = new Matrix3x3(
            new Vector3(cos, 0, -sin),
            new Vector3( 0,  1,   0 ),
            new Vector3(sin, 0,  cos)
            );

        norm = rotationMat.MATxVEC(norm);
        return norm.normalized;
    }
}
