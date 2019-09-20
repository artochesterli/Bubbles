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

    public float PrePushOffset;
    public float OffsetTime;
    public bool Offseting;
    public Vector2 OffsetDirection;
    public Vector2 OriPos;

    private float CurrentOffset;
    

    void Start()
    {
        OriPos = transform.position;
    }

    void Update()
    {
        //CheckOffset();
    }

    private void CheckOffset()
    {
        float OffsetSpeed = PrePushOffset / OffsetTime;
        if (Offseting)
        {
            transform.position += OffsetSpeed * (Vector3)OffsetDirection * Time.deltaTime;
            if (((Vector2)transform.position - OriPos).magnitude>PrePushOffset)
            {
                transform.position = OriPos + OffsetDirection * PrePushOffset;
            }
        }
        else
        {
            transform.position -= OffsetSpeed * (Vector3)OffsetDirection * Time.deltaTime;
            if (Vector2.Dot((Vector2)transform.position - OriPos, OffsetDirection) < 0)
            {
                transform.position = OriPos;
            }
        }
    }

    
}


