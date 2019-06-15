using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotInfo
{
    public Vector2Int Pos;
    public BubbleType InsideBubbleType;
    public BubbleState InsideBubbleState;
    public GameObject ConnectedBubble;

    public SlotInfo(Vector2Int v, BubbleType type, BubbleState state, GameObject g)
    {
        Pos = v;
        InsideBubbleType = type;
        InsideBubbleState = state;
        ConnectedBubble = g;
    }

}
