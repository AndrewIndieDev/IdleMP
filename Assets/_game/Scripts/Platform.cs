using System;

[Serializable]
public struct Platform
{
    public string Name;
    public ulong Price;
    public Platform(string name, ulong price) { Name = name; Price = price; }
}
