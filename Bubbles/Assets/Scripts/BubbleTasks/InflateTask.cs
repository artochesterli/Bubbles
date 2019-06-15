using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InflateTask : Task
{
    private readonly GameObject Obj;
    private readonly Vector3 OriScale;
    private readonly Vector3 InflatedScale;
    private readonly float InflateTime;

    private float TimeCount;

    public InflateTask(GameObject obj,Vector3 start,Vector3 end, float time)
    {
        Obj = obj;
        OriScale = start;
        InflatedScale = end;
        InflateTime = time;
    }

    protected override void Init()
    {
        Obj.GetComponent<Bubble>().State = BubbleState.Inflated;
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
            if (Obj.GetComponent<Bubble>().Type == BubbleType.Disappear)
            {
                GameObject.Destroy(Obj);
            }
        }
    }
}
