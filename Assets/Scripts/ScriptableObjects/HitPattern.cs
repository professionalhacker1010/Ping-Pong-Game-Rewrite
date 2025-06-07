using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pattern
{
    public List<Vector3> list;
    public Vector3 this[int i]
    {
        get { return list[i]; }
    }

    public int size { get => list.Count; }
}

[System.Serializable]
public class PatternList
{
    public List<Pattern> list;
    public Pattern this[int i]
    {
        get { return list[i]; }
    }
    public int size { get => list.Count; }
}

[CreateAssetMenu(fileName = "HitPattern", menuName = "HitPattern")]
public class HitPattern : ScriptableObject
{
    public PatternList patterns;
    public Pattern this[int i]
    {
        get { return patterns[i]; }
    }
    public int size { get => patterns.size; }
}
