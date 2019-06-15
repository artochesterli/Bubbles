using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeflateTask : Task
{
    private readonly GameObject Obj;
    private readonly Vector3 InflatedScale;
    private readonly Vector3 OriScale;
    private readonly float DeflateTime;

    private float TimeCount;

    public DeflateTask(GameObject obj, Vector3 start, Vector3 end, float time)
    {
        Obj = obj;
        InflatedScale = start;
        OriScale = end;
        DeflateTime = time;
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
        Obj.transform.localScale = Vector3.Lerp(InflatedScale, OriScale, TimeCount / DeflateTime);
        if (TimeCount >= DeflateTime)
        {
            Obj.GetComponent<Bubble>().State = BubbleState.Default;
            SetState(TaskState.Success);
        }
    }
}
