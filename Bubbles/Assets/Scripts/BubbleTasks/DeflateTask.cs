using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeflateTask : Task
{
    private readonly GameObject Obj;
    private readonly Vector3 InflatedScale;
    private readonly Vector3 OriScale;
    private readonly float DeflateTime;
    private readonly BubbleTaskMode Mode;
    private readonly Vector2Int Pos;
    private readonly List<List<SlotInfo>> Map;

    private float TimeCount;

    public DeflateTask(GameObject obj, Vector3 start, Vector3 end, float time, Vector2Int pos, List<List<SlotInfo>> map, BubbleTaskMode mode = BubbleTaskMode.Visual)
    {
        Obj = obj;
        InflatedScale = start;
        OriScale = end;
        DeflateTime = time;
        Pos = pos;
        Map = map;
        Mode = mode;
        if (Mode == BubbleTaskMode.Immediate)
        {
            SetMapInfo();
        }
    }

    protected override void Init()
    {
        Obj.GetComponent<Bubble>().State = BubbleState.Stable;
        Obj.transform.localScale = OriScale;
        TimeCount = 0;
    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;
        Obj.transform.localScale = Vector3.Lerp(InflatedScale, OriScale, TimeCount / DeflateTime);
        if (TimeCount >= DeflateTime)
        {
            SetState(TaskState.Success);
            if (Obj.GetComponent<Bubble>().Type == BubbleType.Disappear && Mode != BubbleTaskMode.Visual)
            {
                GameObject.Destroy(Obj);
            }
        }
    }

    private void SetMapInfo()
    {
        Map[Pos.x][Pos.y].InsideBubbleState = BubbleState.Stable;
    }
}
