using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformTask : Task
{
    private readonly GameObject Obj;
    private readonly Vector3 BeginPos;
    private readonly Vector3 TargetPos;
    private readonly float TransformDis;
    private readonly float TransformTime;

    private float TimeCount;

    public TransformTask(GameObject obj,Vector3 begin, Vector3 target,float transformTime)
    {
        Obj = obj;
        BeginPos = begin;
        TargetPos = target;
        TransformTime = transformTime;
    }

    protected override void Init()
    {
        Obj.transform.localPosition = BeginPos;
        if (TransformTime == 0)
        {
            Obj.transform.localPosition = TargetPos;
            SetState(TaskState.Success);
        }
    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;
        Obj.transform.localPosition = Vector3.Lerp(BeginPos, TargetPos, TimeCount / TransformTime);

        if (TimeCount >= TransformTime)
        {
            SetState(TaskState.Success);
        }
    }
}
