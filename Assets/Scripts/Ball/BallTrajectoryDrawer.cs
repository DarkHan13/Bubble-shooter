using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallTrajectoryDrawer : MonoBehaviour
{
    public static BallTrajectoryDrawer I;
    
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private Material dashLineMaterial;
    private static readonly int Tiling = Shader.PropertyToID("_Tiling");

    private void Reset()
    {
        trajectoryLine = GetComponent<LineRenderer>();
        trajectoryLine.material = dashLineMaterial;
    }

    private void Awake()
    {
        I = this;
    }

    public void Clear() => trajectoryLine.positionCount = 0;

    public void Draw(List<Vector2> points, float dashLength = 1f)
    {

        float length = 0f;
        trajectoryLine.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++)
        {
            trajectoryLine.SetPosition(i, points[i]);
            if (i != points.Count - 1)
                length += Vector3.Distance(points[i], points[i + 1]);
        }

        trajectoryLine.material.SetFloat(Tiling, length / dashLength);
    }
}
