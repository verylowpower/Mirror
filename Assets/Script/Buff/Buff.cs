using System;
using UnityEngine;

public class Buff
{
    public string ID;
    public string Name;
    public string Description;
    public Sprite Icon;
    public Action ApplyEffect;
    public string RequirementBuffID;

    public Buff(string id, string name, string description, Sprite icon, Action effect, string requirementBuffID = null)
    {
        ID = id;
        Name = name;
        Description = description;
        Icon = icon;
        ApplyEffect = effect;
        RequirementBuffID = requirementBuffID;
    }
}
