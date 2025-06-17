using System;
using UnityEngine;

public class Buff 
{
    public string ID;
    public string Name;
    public string Decription;
    public Sprite Icon;
    public Action ApplyEffect;
    
    public Buff(string id, string name, string decription,Sprite icon, Action effect)
    {
        ID = id;
        Name = name;
        Decription = decription;
        Icon = icon;
        ApplyEffect = effect;
    }
}
