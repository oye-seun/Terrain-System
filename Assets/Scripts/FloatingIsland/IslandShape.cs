using System.Collections.Generic;
using UnityEngine;

public class IslandShape
{
    public int NoOfPoints;
    public float AverageRadius;
    public float RadiusOffsetScale;
    public float SamplingScale;
    public Vector3 Origin;
    public Vector3 SampleOrigin;

    public IslandShape(int noOfPoints, float averageRadius, float radiusOffsetScale, Vector3 origin, float samplingScale, Vector3 sampleOrigin)
    {
        NoOfPoints = noOfPoints;
        AverageRadius = averageRadius;
        RadiusOffsetScale = radiusOffsetScale;
        Origin = origin;
        SamplingScale = samplingScale;
        SampleOrigin = sampleOrigin;
    }

    public List<Vector3> GenerateBoundary()
    {
        List<Vector3> boundary = new List<Vector3>();
        float angleDiv = 360.0f / (float)NoOfPoints;

        //for (int i = 0; i < NoOfPoints; i++)
        //{
        //    Vector3 dir = new Vector3(Mathf.Cos(i * angleDiv * Mathf.Deg2Rad), 0, Mathf.Sin(i * angleDiv * Mathf.Deg2Rad)).normalized;
        //    RadiusOffsetScale = (RadiusOffsetScale > AverageRadius) ? AverageRadius - 0.5f : RadiusOffsetScale;
        //    Vector3 pos = Origin + (dir * (AverageRadius + (Random.Range(-1, 1) * RadiusOffsetScale)));
        //    boundary.Add(pos);
        //}

        for (int i = 0; i < NoOfPoints; i++)
        {
            Vector3 dir = new Vector3(Mathf.Cos(i * angleDiv * Mathf.Deg2Rad), 0, Mathf.Sin(i * angleDiv * Mathf.Deg2Rad)).normalized;
            Vector3 origin = SampleOrigin + (dir * SamplingScale);
            float offset = Mathf.PerlinNoise(origin.x, origin.z);
            offset = (offset * 2) - 1;

            Vector3 pos = Origin + (dir * (AverageRadius + (RadiusOffsetScale * offset)));
            boundary.Add(pos);
        }

        return boundary;
    }
}
