using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformTask : Task
{
    private readonly GameObject Obj;
    private readonly Vector3 Pos;
    private readonly Vector3 Dir;
    private readonly float TransformDis;
    private readonly float TransformTime;

    private float TimeCount;

    public TransformTask(GameObject obj,Vector3 pos, Vector3 dir,float transformDis,float transformTime)
    {
        Obj = obj;
        Pos = pos;
        Dir = dir;
        TransformDis = transformDis;
        TransformTime = transformTime;
    }

    protected override void Init()
    {
        Obj.transform.localPosition = Pos;
        if (TransformTime == 0)
        {
            Obj.transform.localPosition = Pos + Dir * TransformDis;
            SetState(TaskState.Success);
        }
    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;
        Obj.transform.localPosition = Vector3.Lerp(Pos, Pos + Dir * TransformDis, TimeCount / TransformTime);

        if (TimeCount >= TransformTime)
        {
            SetState(TaskState.Success);
        }
    }
}
