using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTask : Task
{
    private readonly GameObject Obj;
    private readonly Vector3 Pos;
    private readonly Vector3 Dir;
    private readonly float MoveDis;
    private readonly float MoveTime;
    private readonly Vector2Int Start;
    private readonly Vector2Int End;
    private readonly BubbleType Type;
    private readonly List<List<SlotInfo>> Map;
    private readonly BubbleTaskMode Mode;

    private float TimeCount;

    public MoveTask(GameObject obj, Vector3 pos, Vector3 dir ,float dis, float time , Vector2Int start, Vector2Int end , BubbleType type = BubbleType.Null, List<List<SlotInfo>> map=null , BubbleTaskMode mode = BubbleTaskMode.Visual)
    {
        Obj = obj;
        Pos = pos;
        Dir = dir;
        MoveDis = dis;
        MoveTime = time;
        Start = start;
        End = end;
        Type = type;
        Map = map;
        Mode = mode;

        if (Mode == BubbleTaskMode.Immediate)
        {
            SetMapInfo();
        }

    }

    protected override void Init()
    {
        if (Mode == BubbleTaskMode.Immediate)
        {
            Obj.GetComponent<Bubble>().State = BubbleState.Activated;
        }
        else
        {
            Obj.GetComponent<Bubble>().State = BubbleState.Stable;
        }

        Obj.transform.localPosition = Pos;

        if(Mode == BubbleTaskMode.Delay)
        {
            SetMapInfo();
        }

        if (MoveTime == 0)
        {
            Obj.transform.localPosition = Pos + Dir * MoveDis;
            SetState(TaskState.Success);
        }
    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;
        Obj.transform.localPosition = Vector3.Lerp(Pos, Pos + Dir * MoveDis, TimeCount / MoveTime);

        if (TimeCount >= MoveTime)
        {
            SetState(TaskState.Success);
        }
    }

    private void SetMapInfo()
    {
        Map[Start.x][Start.y].InsideBubbleState = BubbleState.Stable;
        Map[Start.x][Start.y].InsideBubbleType = BubbleType.Null;
        Map[Start.x][Start.y].ConnectedBubble = null;
        if (Mode == BubbleTaskMode.Immediate)
        {
            Map[End.x][End.y].InsideBubbleState = BubbleState.Activated;
        }
        else
        {
            Map[End.x][End.y].InsideBubbleState = BubbleState.Stable;
        }
        
        Map[End.x][End.y].InsideBubbleType= Type;
        Map[End.x][End.y].ConnectedBubble = Obj;
    }
}
