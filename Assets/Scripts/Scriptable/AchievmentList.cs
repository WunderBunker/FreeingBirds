using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AchievmentList", menuName = "Scriptable Objects/AchievmentList")]
public class AchievmentList : ScriptableObject
{
    public List<Achievment> List = new();
}

[Serializable]
public struct Achievment
{
    public string Name;
    public int Id;
}