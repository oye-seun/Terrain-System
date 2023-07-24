using UnityEngine;

[System.Serializable]
public class ControlPoint
{
    public Vector3 A;
    public Vector3 Centre;
    public Vector3 B;
    public ControlPoint(Vector3 centre, Vector3 a, Vector3 b)
    {
        Centre = centre;
        A = a;
        B = b;
    }
}
