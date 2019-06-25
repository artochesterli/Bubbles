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
    Default,
    Blocked,
    Moving,
    Inflated
}

public class Bubble : MonoBehaviour
{
    public BubbleType Type;
    public BubbleState State;
}


