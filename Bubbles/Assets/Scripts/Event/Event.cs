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

public class LevelLoaded : Event
{
    public int Index;
    public LevelLoaded(int num)
    {
        Index = num;
    }
}

public class BubbleNumSet : Event
{
    public BubbleType Type;
    public int Num;
    public BubbleNumSet(BubbleType type,int num)
    {
        Type = type;
        Num = num;
    }
}

public class BubbleSelected : Event
{
    public BubbleType Type;
    public BubbleSelected (BubbleType type)
    {
        Type = type;
    }
}

