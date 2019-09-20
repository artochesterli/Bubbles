using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTask : Task
{
    private readonly GameObject Obj;
    private readonly Vector3 BeginPos;
    private readonly Vector3 TargetPos;
    private readonly float MoveTime;
    private readonly Vector2Int Start;
    private readonly Vector2Int End;
    private readonly BubbleType Type;
    private readonly List<List<SlotInfo>> Map;

    private float TimeCount;
    private float Speed;

    public MoveTask(GameObject obj, Vector3 begin, Vector3 target , float time , Vector2Int start, Vector2Int end , BubbleType type = BubbleType.Null, List<List<SlotInfo>> map=null)
    {
        Obj = obj;
        BeginPos = begin;
        TargetPos = target;
        MoveTime = time;
        Start = start;
        End = end;
        Type = type;
        Map = map;

        SetMapInfo();

    }

    protected override void Init()
    {
        Activate();

        Obj.transform.localPosition = BeginPos;

        if (MoveTime == 0)
        {
            Obj.GetComponent<Bubble>().OriPos = TargetPos;
            Obj.transform.localPosition = TargetPos;
            SetState(TaskState.Success);
        }
    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;

        if (TimeCount >= MoveTime)
        {
            Obj.GetComponent<Bubble>().OriPos = TargetPos;
            Obj.transform.localPosition = TargetPos;
            SetState(TaskState.Success);
        }
        else if(TimeCount >= MoveTime / 2)
        {
            Speed = 2 * (TargetPos-BeginPos).magnitude / MoveTime * ((MoveTime - TimeCount) / (MoveTime / 2));
            Obj.transform.localPosition += Speed * (TargetPos - BeginPos).normalized * Time.deltaTime;
            if (Vector3.Dot(Obj.transform.localPosition - TargetPos, TargetPos - BeginPos) > 0)
            {
                Obj.transform.localPosition = TargetPos;
            }
        }
        else
        {
            Speed = 2 * (TargetPos - BeginPos).magnitude / MoveTime * TimeCount / (MoveTime / 2);
            Obj.transform.localPosition += Speed * (TargetPos - BeginPos).normalized * Time.deltaTime;
        }
    }

    private void SetMapInfo()
    {
        
        Map[Start.x][Start.y].InsideBubbleState = BubbleState.Stable;
        Map[Start.x][Start.y].InsideBubbleType = BubbleType.Null;
        Map[Start.x][Start.y].ConnectedBubble = null;

        Map[End.x][End.y].InsideBubbleState = BubbleState.Activated;
        Map[End.x][End.y].InsideBubbleType= Type;
        Map[End.x][End.y].ConnectedBubble = Obj;
    }

    private void Activate()
    {
        Obj.GetComponent<Bubble>().State = BubbleState.Activated;
        Obj.transform.Find("StableEffect").GetComponent<ParticleSystem>().Stop();
        Obj.transform.Find("StableEffect").GetComponent<ParticleSystem>().Clear();

        Obj.transform.Find("ActivateEffect").GetComponent<ParticleSystem>().Play();
    }
}
