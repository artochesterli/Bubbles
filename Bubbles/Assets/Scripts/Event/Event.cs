using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event { }

public class Place : Event
{
    public Vector2Int Pos;
    public Place(Vector2Int v)
    {
        Pos = v;
    }
}

public class MotionFinish : Event { }
