using System;

[Serializable]
public struct Genre
{
    public string Name;
    public float[] Popularity;
    public Genre(string name, float[] popularity) { Name = name; Popularity = popularity; }
}
