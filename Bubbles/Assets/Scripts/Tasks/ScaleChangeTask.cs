using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleChangeTask : Task
{
    private readonly GameObject Obj;
    private readonly float BeginScale;
    private readonly float EndScale;
    private readonly float DeflateTime;

    private float TimeCount;

    public ScaleChangeTask(GameObject obj, float start, float end, float time)
    {
        Obj = obj;
        BeginScale = start;
        EndScale = end;
        DeflateTime = time;
    }

    protected override void Init()
    {
        if (Obj.GetComponent<Bubble>() != null)
        {
            Obj.GetComponent<Bubble>().State = BubbleState.Stable;
        }
        Obj.transform.localScale = BeginScale * Vector3.one;
    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;
        Obj.transform.localScale = Vector3.Lerp(BeginScale*Vector3.one, EndScale*Vector3.one, TimeCount / DeflateTime);
        if (TimeCount >= DeflateTime)
        {
            SetState(TaskState.Success);
        }
    }

}
