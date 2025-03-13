using AndrewDowsett.Utility;
using System;
using UnityEngine;

[Serializable]
public struct Feature
{
    public string Name;
    public Sprite Icon;
    public ulong Cost;
    public EDevSkill SkillCategory;
    public bool IsUnlocked;

    public string CostString() => $"{Name}\n{Utilities.GetShortCostString(Cost)}";

    public Feature(Feature toCopy)
    {
        Name = toCopy.Name;
        Icon = toCopy.Icon;
        Cost = toCopy.Cost;
        SkillCategory = toCopy.SkillCategory;
        IsUnlocked = toCopy.IsUnlocked;
    }
}