using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Bird", menuName = "Scriptable Objects/BirdList")]
public class BirdList : ScriptableObject, IEnumerable<Bird>
{
    public Bird[] List;

    public IEnumerator<Bird> GetEnumerator()
    {
        foreach (Bird lBird in List) yield return lBird;
    }

    // Implémentation non générique obligatoire
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

}

[Serializable]
public struct Bird
{
    public int Index;
    public string Id;
    public Sprite Image;
    public Vector2 ImageOutlineOffset;
    public float ScoreToBeAccessible;
    public Color Color;
    public GameObject BirdPrefab;
    public BirdOverride BirdOverride;

    public Material Pipes;
    public GameObject CamMeteo;
    public GameObject MapMeteo;
    public GameObject CanvasMeteo;

    public AudioClip IntroMusic;
    public AudioClip LoopMusic;
}
