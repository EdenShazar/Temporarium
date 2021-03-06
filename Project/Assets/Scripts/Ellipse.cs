﻿using UnityEngine;
using System.Collections;

// for sanity & debugging, adjustment of code from:
// https://answers.unity.com/questions/631201/draw-an-ellipse-in-unity-3d.html

[RequireComponent(typeof(LineRenderer))]
public class Ellipse : MonoBehaviour
{

    public float rX = 5;
    public float rY = 3;
    public float centerX = 1;
    public float centerY = 1;
    public float theta = 0f;
    public int resolution = 30;

    LineRenderer lr;

    private Vector3[] positions;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.positionCount = resolution + 1;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
    }

    public void Draw() {
        positions = CreateEllipse(rX, rY, centerX, centerY, theta, resolution);
        for (int i = 0; i <= resolution; i++)
        {
            lr.SetPosition(i, positions[i]);
        }
    }

    Vector3[] CreateEllipse(float a, float b, float h, float k, float theta, int resolution)
    {

        positions = new Vector3[resolution + 1];
        Quaternion q = Quaternion.AngleAxis(theta, Vector3.forward);
        Vector3 center = new Vector3(h, k, 0.0f);

        for (int i = 0; i <= resolution; i++)
        {
            float angle = (float)i / (float)resolution * 2.0f * Mathf.PI;
            positions[i] = new Vector3(a * Mathf.Cos(angle), b * Mathf.Sin(angle), 0.0f);
            positions[i] = q * positions[i] + center;
        }

        return positions;
    }
}