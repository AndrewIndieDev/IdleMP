using System;
using UnityEngine;

[Serializable]
public struct Genre
{
    public string Name;
    [HideInInspector] public float[] Popularity;
    public Genre(string name, float[] popularity) { Name = name; Popularity = popularity; }
    public Genre(string name) { Name = name; Popularity = new float[0]; }
}
