using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BubbleType
{
    Null,
    Disappear,
    Normal
}

public enum BubbleState
{
    Stable,
    Activated,
    Exhausted
}

public class Bubble : MonoBehaviour
{
    public BubbleType Type;
    public BubbleState State;

    public Color NormalColor;
    public Color ActivateColor;
    public Color ExhaustColor;

}


