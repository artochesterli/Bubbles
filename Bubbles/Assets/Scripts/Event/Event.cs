using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event { }

public class Place : Event
{
    public Vector2Int Pos;
    public BubbleType Type;

    public Place(Vector2Int v, BubbleType type)
    {
        Pos = v;
        Type = type;
    }
}

public class MotionFinish : Event { }

public class LevelFinish : Event
{
    public int Index;
    public bool Success;
    public LevelFinish(int num, bool b)
    {
        Success = b;
        Index = num;
    }
}
