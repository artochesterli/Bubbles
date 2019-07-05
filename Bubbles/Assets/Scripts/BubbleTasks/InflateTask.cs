using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InflateTask : Task
{
    private readonly GameObject Obj;
    private readonly Vector3 OriScale;
    private readonly Vector3 InflatedScale;
    private readonly float InflateTime;
    private readonly BubbleTaskMode Mode;
    private readonly List<List<SlotInfo>> Map;
    private readonly Vector2Int Pos;

    private float TimeCount;

    public InflateTask(GameObject obj,Vector3 start,Vector3 end, float time, Vector2Int pos, List<List<SlotInfo>> map, BubbleTaskMode mode=BubbleTaskMode.Visual)
    {
        Obj = obj;
        OriScale = start;
        InflatedScale = end;
        InflateTime = time;
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
        Obj.GetComponent<Bubble>().State = BubbleState.Exhausted;
        
        Obj.transform.localScale = OriScale;
        TimeCount = 0;
    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;
        Obj.transform.localScale = Vector3.Lerp(OriScale, InflatedScale, TimeCount/InflateTime);
        if (TimeCount >= InflateTime)
        {
            SetState(TaskState.Success);
        }
    }

    private void SetMapInfo()
    {
        Map[Pos.x][Pos.y].InsideBubbleState = BubbleState.Exhausted;
    }
}
