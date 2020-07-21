using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockedTask : Task
{
    private readonly GameObject Obj;
    private Vector3 Pos;
    private readonly Vector3 Direction;
    private readonly Direction Dir;
    private readonly float BlockedDis;
    private readonly float BlockedTime;
    private readonly List<List<SlotInfo>> Map;
    private readonly Vector2Int V;

    private float TimeCount;
    private bool forward;
    private Vector3 OriPos;

    public BlockedTask(GameObject obj, Vector3 pos, Vector3 dir, Direction d, float dis, float time, Vector2Int v, List<List<SlotInfo>> map)
    {
        Obj = obj;
        Pos = pos;
        Direction = dir;
        Dir = d;
        BlockedDis = dis;
        BlockedTime = time;
        V = v;
        Map = map;
        OriPos = Pos;

        SetMapInfo();
    }

    protected override void Init()
    {
        Activate();

        if (Obj.GetComponent<NormalBubble>())
        {
            Obj.GetComponent<NormalBubble>().SelfPosInfo.LegalPos = Pos + Direction * Obj.GetComponent<NormalBubble>().SelfPosInfo.DicDirOffset[Dir];
            Obj.GetComponent<NormalBubble>().SelfPosInfo.DicDirOffset[Dir] = 0;
            Obj.GetComponent<NormalBubble>().SelfPosInfo.DicDirMoveTask[Dir] = true;
        }
        else
        {
            Obj.transform.localPosition = Pos;
        }

        forward = true;


    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;

        if (TimeCount >= BlockedTime / 2)
        {
            forward = false;
        }

        if (forward)
        {
            if (Obj.GetComponent<NormalBubble>())
            {
                Obj.GetComponent<NormalBubble>().SelfPosInfo.LegalPos = Vector3.Lerp(Pos, Pos + Direction * BlockedDis, 2 * TimeCount / BlockedTime);
            }
            else
            {
                Obj.transform.localPosition = Vector3.Lerp(Pos, Pos + Direction * BlockedDis, 2 * TimeCount / BlockedTime);
            }

        }
        else
        {
            if (Obj.GetComponent<NormalBubble>())
            {
                Obj.GetComponent<NormalBubble>().SelfPosInfo.LegalPos = Vector3.Lerp(Pos + Direction * BlockedDis, OriPos, 2 * (TimeCount - BlockedTime / 2) / BlockedTime);
            }
            else
            {
                Obj.transform.localPosition = Vector3.Lerp(Pos + Direction * BlockedDis, OriPos, 2 * (TimeCount - BlockedTime / 2) / BlockedTime);
            }

        }

        if (TimeCount > BlockedTime)
        {
            if (Obj.GetComponent<NormalBubble>())
            {
                Obj.GetComponent<NormalBubble>().SelfPosInfo.DicDirMoveTask[Dir] = false;
            }
            SetState(TaskState.Success);
        }
    }

    private void SetMapInfo()
    {
        Map[V.x][V.y].InsideBubbleState = BubbleState.Activated;
    }

    private void Activate()
    {
        Obj.GetComponent<Bubble>().State = BubbleState.Activated;

        Obj.transform.Find("ActivateEffect").GetComponent<ParticleSystem>().Play();
    }
}
