using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class WindingCountBoundaryTest
{
    public static bool CheckWithinBoundary(Vector3 pos, IEnumerable<Vector3> boundary, Plane plane)
    {
        Vector2[] bound = new Vector2[boundary.Count()];
        Vector2 pos2 = Vector2.zero;
        for (int i = 0; i < boundary.Count(); i++)
        {
            if (plane == Plane.X_Y) bound[i] = new Vector2(boundary.ElementAt(i).x, boundary.ElementAt(i).y);
            else if (plane == Plane.X_Z) bound[i] = new Vector2(boundary.ElementAt(i).x, boundary.ElementAt(i).z);
            else if (plane == Plane.Y_Z) bound[i] = new Vector2(boundary.ElementAt(i).y, boundary.ElementAt(i).z);
        }

        if (plane == Plane.X_Y) pos2.x = pos.x; pos2.y = pos.y;
        if (plane == Plane.X_Z) pos2.x = pos.x; pos2.y = pos.z;
        if (plane == Plane.Y_Z) pos2.x = pos.y; pos2.y = pos.z;

        return CheckWithinBoundary2D(pos2, bound);
    }

    public static bool CheckWithinBoundary2D(Vector2 pos, IEnumerable<Vector2> boundary)
    {
        int winding = 0;
        for (int i = 0; i < boundary.Count(); i++)
        {
            int nextIndex = (i + 1) % boundary.Count();
            float val = (pos.y - boundary.ElementAt(i).y) * (pos.y - boundary.ElementAt(nextIndex).y);
            if (val < 0)
            {
                Vector2 dir = (boundary.ElementAt(i) - boundary.ElementAt(nextIndex)).normalized;
                Vector2 intersection = VectorIntersection.VectorIntersectionCheck(pos, Vector2.right, boundary.ElementAt(i), dir);
                if ((boundary.ElementAt(i) - intersection).sqrMagnitude <= (boundary.ElementAt(i) - boundary.ElementAt(nextIndex)).sqrMagnitude
                    && (intersection - pos).x > 0
                    && Vector2.Dot(dir, (boundary.ElementAt(i) - intersection).normalized) > 0.99f)
                {
                    if (dir.y > 0)
                        winding++;
                    else
                        winding--;
                }
            }
        }
        return winding != 0;
    }

    public static bool Check3d(Vector3 pos, IEnumerable<Vector3> boundary)
    {
        pos.y = boundary.ElementAt(0).y;
        int winding = 0;
        for (int i = 0; i < boundary.Count(); i++)
        {
            int nextIndex = (i + 1) % boundary.Count();
            float val = (pos.z - boundary.ElementAt(i).z) * (pos.z - boundary.ElementAt(nextIndex).z);
            if (val < 0)
            {
                Vector3 dir = (boundary.ElementAt(i) - boundary.ElementAt(nextIndex)).normalized;
                Vector3 intersection = VectorIntersection.VectorIntersectionCheck(pos, Vector3.right, boundary.ElementAt(i), dir);
                if ((intersection - pos).x > 0
                    && dot(dir, (boundary.ElementAt(i) - intersection).normalized) > 0.99f)
                {
                    if (dir.z > 0)
                        winding++;
                    else
                        winding--;
                }
            }
        }
        return winding != 0;
    }

    private static float dot(Vector3 A, Vector3 B)
    {
        return (A.x * B.x) + (A.z * B.z);
    }

    public enum Plane
    {
        X_Y,
        X_Z,
        Y_Z,
    }
}
