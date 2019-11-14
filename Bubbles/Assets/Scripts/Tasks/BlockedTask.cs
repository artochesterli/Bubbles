using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockedTask : Task
{
    private readonly GameObject Obj;
    private readonly Vector3 Pos;
    private readonly Vector3 Direction;
    private readonly float BlockedDis;
    private readonly float BlockedTime;
    private readonly List<List<SlotInfo>> Map;
    private readonly Vector2Int V;

    private float TimeCount;
    private bool forward;

    public BlockedTask(GameObject obj, Vector3 pos, Vector3 dir, float dis, float time, Vector2Int v, List<List<SlotInfo>> map)
    {
        Obj = obj;
        Pos = pos;
        Direction = dir;
        BlockedDis = dis;
        BlockedTime = time;
        V = v;
        Map = map;

        SetMapInfo();
    }

    protected override void Init()
    {
        Activate();

        Obj.transform.localPosition = Pos;

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
            Obj.transform.localPosition = Vector3.Lerp(Pos, Pos + Direction * BlockedDis, 2 * TimeCount/BlockedTime);
        }
        else
        {
            Obj.transform.localPosition = Vector3.Lerp(Pos + Direction * BlockedDis, Pos, 2 * (TimeCount - BlockedTime / 2) / BlockedTime);
        }

        if (TimeCount > BlockedTime)
        {
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
