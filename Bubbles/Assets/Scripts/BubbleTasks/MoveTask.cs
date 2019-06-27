using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveTaskMode
{
    Visual,
    Immediate,
    Delay
}

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
    private readonly MoveTaskMode Mode;

    private float TimeCount;

    public MoveTask(GameObject obj, Vector3 pos, Vector3 dir ,float dis, float time , Vector2Int start, Vector2Int end , BubbleType type = BubbleType.Null, List<List<SlotInfo>> map=null , MoveTaskMode mode = MoveTaskMode.Visual)
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

        if (Mode == MoveTaskMode.Immediate)
        {
            SetMapInfo();
        }

    }

    protected override void Init()
    {
        Obj.GetComponent<Bubble>().State = BubbleState.Moving;
        Obj.transform.localPosition = Pos;

        if(Mode == MoveTaskMode.Delay)
        {
            SetMapInfo();
        }
        
    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;
        Obj.transform.localPosition = Vector3.Lerp(Pos, Pos + Dir * MoveDis, TimeCount / MoveTime);

        if (TimeCount >= MoveTime)
        {
            Obj.GetComponent<Bubble>().State = BubbleState.Default;
            SetState(TaskState.Success);
        }
    }

    private void SetMapInfo()
    {
        Map[Start.x][Start.y].InsideBubbleState = BubbleState.Default;
        Map[Start.x][Start.y].InsideBubbleType = BubbleType.Null;
        Map[Start.x][Start.y].ConnectedBubble = null;
        Map[End.x][End.y].InsideBubbleState = BubbleState.Default;
        Map[End.x][End.y].InsideBubbleType= Type;
        Map[End.x][End.y].ConnectedBubble = Obj;
    }
}
