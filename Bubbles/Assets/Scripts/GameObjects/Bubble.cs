using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BubbleType
{
    Null,
    Disappear,
    Normal,
    Expand
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
    public Color ExhaustColor;

    public float PrePushOffset;
    public float OffsetTime;
    public float OffsetStartDis;
    public float OffsetEndDis;

    public bool Offseting;
    public Vector2 OffsetDirection;
    public Vector2 OriPos;
    public GameObject DirectionIndicator;

    private float OffsetTimeCount;
    private Vector2 OffsetEffectOriPos;
    

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
        if (Offseting)
        {
            DirectionIndicator.GetComponent<SpriteRenderer>().enabled = true;
            OffsetTimeCount += Time.deltaTime;
            Color color = DirectionIndicator.GetComponent<SpriteRenderer>().color;
            DirectionIndicator.GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(color.r, color.g, color.b, 1), new Color(color.r, color.g, color.b, 0), OffsetTimeCount / OffsetTime);


            if (OffsetTimeCount >= OffsetTime)
            {
                OffsetTimeCount = 0;
            }

            DirectionIndicator.transform.localPosition = Vector2.Lerp(OffsetDirection * OffsetStartDis, OffsetDirection * OffsetEndDis, OffsetTimeCount / OffsetTime);
            DirectionIndicator.transform.rotation = Quaternion.Euler(0,0, Vector2.SignedAngle(Vector2.right, OffsetDirection));
        }
        else
        {
            DirectionIndicator.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    
}


