using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotInfo
{
    public Vector2Int Pos;
    public SlotType slotType;
    public BubbleType InsideBubbleType;
    public BubbleState InsideBubbleState;
    public GameObject ConnectedBubble;
    public Vector3 Location;
    public GameObject Entity;

    public SlotInfo(Vector2Int v, SlotType sType, BubbleType type, BubbleState state, GameObject bubble, Vector3 loc, GameObject entity)
    {
        Pos = v;
        slotType = sType;
        InsideBubbleType = type;
        InsideBubbleState = state;
        ConnectedBubble = bubble;
        Location = loc;
        Entity = entity;
    }

}
