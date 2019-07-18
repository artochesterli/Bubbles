using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppearTask : Task
{
    private readonly GameObject Obj;
    private readonly float AppearTime;
    private readonly Vector2Int Pos;
    private readonly List<List<SlotInfo>> Map;
    private readonly BubbleType Type;

    private float TimeCount;
    private Color color;

    public AppearTask(GameObject obj,float time,Vector2Int pos, List<List<SlotInfo>> map, BubbleType type)
    {
        Obj = obj;
        AppearTime = time;
        Pos = pos;
        Map = map;
        Type = type;
    }

    protected override void Init()
    {
        color = Obj.GetComponent<SpriteRenderer>().color;

        Map[Pos.x][Pos.y].InsideBubbleType = Type;
        Map[Pos.x][Pos.y].ConnectedBubble = Obj;
    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;
        Obj.GetComponent<SpriteRenderer>().color = Color.Lerp( new Color(color.r, color.g, color.b, 0), new Color(color.r, color.g, color.b, 1), TimeCount / AppearTime);
        if (TimeCount >= AppearTime)
        {
            SetState(TaskState.Success);
        }
    }
}
