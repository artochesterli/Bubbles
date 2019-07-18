using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleChangeInfo
{
    public GameObject Bubble;
    public BubbleType Type;
    public bool Placed;
    public Vector2Int From;
    public Vector2Int To;
    public Vector3 BeginPos;
    public Vector3 EndPos;

    public BubbleChangeInfo(GameObject obj, BubbleType type, bool placed,Vector2Int from,Vector2Int to,Vector3 beginPos,Vector3 endPos)
    {
        Bubble = obj;
        Type = type;
        Placed = placed;
        From = from;
        To = to;
        BeginPos = beginPos;
        EndPos = endPos;
    }
}
