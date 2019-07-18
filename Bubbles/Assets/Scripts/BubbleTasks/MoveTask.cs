﻿using System.Collections;
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

    private float TimeCount;
    private float Speed;

    public MoveTask(GameObject obj, Vector3 pos, Vector3 dir ,float dis, float time , Vector2Int start, Vector2Int end , BubbleType type = BubbleType.Null, List<List<SlotInfo>> map=null)
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

        SetMapInfo();


    }

    protected override void Init()
    {
        Activate();

        Obj.transform.localPosition = Pos;

        if (MoveTime == 0)
        {
            Obj.transform.localPosition = Pos + Dir * MoveDis;
            SetState(TaskState.Success);
        }
    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;

        //Obj.transform.localPosition = Vector3.Lerp(Pos, Pos + Dir * MoveDis, TimeCount / MoveTime);

        if (TimeCount >= MoveTime)
        {
            Obj.transform.localPosition = Pos + Dir * MoveDis;
            SetState(TaskState.Success);
        }
        else if(TimeCount >= MoveTime / 2)
        {
            Speed = 2 * MoveDis / MoveTime * ((MoveTime - TimeCount) / (MoveTime / 2));
            Obj.transform.localPosition += Speed * Dir * Time.deltaTime;
        }
        else
        {
            Speed = 2 * MoveDis / MoveTime * TimeCount / (MoveTime / 2);
            Obj.transform.localPosition += Speed * Dir * Time.deltaTime;
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
        Obj.transform.Find("ActivateEffect").GetComponent<ParticleSystem>().Play();
    }
}
