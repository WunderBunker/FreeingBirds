using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "AchievementList", menuName = "Scriptable Objects/AchievementList")]
public class AchievementList : ScriptableObject
{
    public List<Achievement> List = new();
}

[Serializable]
public struct Achievement
{
    public string Name;
    public LocalizedString LocalText;
    public LocalArg[] LocalArg;
    public int Id;
}

[Serializable]
public struct LocalArg
{
    public int Index;
    public string String;
    public LocalizedString LocalizedString;
}