using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTask : Task
{
    private readonly GameObject Obj;
    private Vector3 BeginPos;
    private readonly Vector3 TargetPos;
    private readonly float MoveTime;
    private readonly Vector2Int Start;
    private readonly Vector2Int End;
    private readonly BubbleType Type;
    private readonly Direction Dir;
    private readonly List<List<SlotInfo>> Map;
    private readonly bool Teleport;

    private float TimeCount;
    private float Speed;

    public MoveTask(GameObject obj, Vector3 begin, Vector3 target , Direction dir, float time , Vector2Int start, Vector2Int end , BubbleType type = BubbleType.Null, List<List<SlotInfo>> map=null, bool teleport = false)
    {
        Obj = obj;
        BeginPos = begin;
        TargetPos = target;
        MoveTime = time;
        Start = start;
        End = end;
        Type = type;
        Dir = dir;
        Map = map;
        Teleport = teleport;

        SetMapInfo();

    }

    protected override void Init()
    {
        if (Teleport)
        {
            EnergyLost();
        }
        else
        {
            Activate();
        }

        Obj.transform.localPosition = BeginPos;

        if (MoveTime == 0)
        {
            Obj.transform.localPosition = TargetPos;
            if (Obj.GetComponent<NormalBubble>())
            {
                Obj.GetComponent<NormalBubble>().SelfPosInfo.LegalPos = TargetPos;
            }

            SetState(TaskState.Success);
        }
        else
        {
            if (Obj.GetComponent<NormalBubble>())
            {
                Obj.GetComponent<NormalBubble>().SelfPosInfo.LegalPos = BeginPos + (TargetPos-BeginPos).normalized* Obj.GetComponent<NormalBubble>().SelfPosInfo.DicDirOffset[Dir];
                BeginPos = (Vector3)Obj.GetComponent<NormalBubble>().SelfPosInfo.LegalPos;
                Obj.GetComponent<NormalBubble>().SelfPosInfo.DicDirOffset[Dir] = 0;
                Obj.GetComponent<NormalBubble>().SelfPosInfo.DicDirMoveTask[Dir] = true;
            }
        }
    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;

        if (TimeCount >= MoveTime)
        {
            if (Obj.GetComponent<NormalBubble>())
            {
                Obj.GetComponent<NormalBubble>().SelfPosInfo.LegalPos = TargetPos;
                Obj.GetComponent<NormalBubble>().SelfPosInfo.DicDirMoveTask[Dir] = false;
            }
            else
            {
                Obj.transform.localPosition = TargetPos;
            }


            SetState(TaskState.Success);
        }
        else if(TimeCount >= MoveTime / 2)
        {
            Speed = 2 * (TargetPos-BeginPos).magnitude / MoveTime * ((MoveTime - TimeCount) / (MoveTime / 2));

            if (Obj.GetComponent<NormalBubble>())
            {
                Obj.GetComponent<NormalBubble>().SelfPosInfo.LegalPos += Speed * (Vector2)(TargetPos - BeginPos).normalized * Time.deltaTime;
                if (Vector3.Dot((Vector3)Obj.GetComponent<NormalBubble>().SelfPosInfo.LegalPos - TargetPos, TargetPos - BeginPos) > 0)
                {
                    Obj.GetComponent<NormalBubble>().SelfPosInfo.LegalPos = TargetPos;
                }
            }
            else
            {
                Obj.transform.localPosition += Speed * (TargetPos - BeginPos).normalized * Time.deltaTime;
                if (Vector3.Dot(Obj.transform.localPosition - TargetPos, TargetPos - BeginPos) > 0)
                {
                    Obj.transform.localPosition = TargetPos;
                }
            }

        }
        else
        {
            Speed = 2 * (TargetPos - BeginPos).magnitude / MoveTime * TimeCount / (MoveTime / 2);

            if (Obj.GetComponent<NormalBubble>())
            {
                Obj.GetComponent<NormalBubble>().SelfPosInfo.LegalPos += Speed * (Vector2)(TargetPos - BeginPos).normalized * Time.deltaTime;
            }
            else
            {
                Obj.transform.localPosition += Speed * (TargetPos - BeginPos).normalized * Time.deltaTime;
            }

        }
    }

    private void SetMapInfo()
    {
        
        Map[Start.x][Start.y].InsideBubbleState = BubbleState.Stable;
        Map[Start.x][Start.y].InsideBubbleType = BubbleType.Null;
        Map[Start.x][Start.y].ConnectedBubble = null;

        if (Teleport)
        {
            Map[End.x][End.y].InsideBubbleState = BubbleState.Exhausted;
        }
        else
        {
            Map[End.x][End.y].InsideBubbleState = BubbleState.Activated;
        }

        Map[End.x][End.y].InsideBubbleType= Type;
        Map[End.x][End.y].ConnectedBubble = Obj;
    }

    private void Activate()
    {
        Obj.GetComponent<Bubble>().State = BubbleState.Activated;

        Obj.transform.Find("ActivateEffect").GetComponent<ParticleSystem>().Play();
    }

    private void EnergyLost()
    {
        Obj.GetComponent<Bubble>().State = BubbleState.Stable;
        GameObject ActivateEffect = Obj.transform.Find("ActivateEffect").gameObject;
        GameObject Temp = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Effect/TempActivateEffect"), Obj.transform.position, Quaternion.Euler(0, 0, 0));
        ActivateEffect.GetComponent<ParticleSystem>().Stop();
        ActivateEffect.GetComponent<ParticleSystem>().Clear();
        Temp.GetComponent<ParticleSystem>().Stop();

        Obj.GetComponent<Bubble>().State = BubbleState.Exhausted;
        Obj.GetComponent<SpriteRenderer>().color = Obj.GetComponent<Bubble>().ExhaustColor;
        //Obj.transform.localScale = Vector3.one * 0.8f;
    }
}
