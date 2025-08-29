using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Gesture
{
    public string Name;
    public List<Vector2> Points;

    public Gesture(List<Vector2> points, string name = "")
    {
        Name = name;
        Points = new List<Vector2>(points);
    }
}