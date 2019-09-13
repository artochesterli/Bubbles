using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitTask : Task
{
    private readonly float WaitTime;

    private float TimeCount;

    public WaitTask(float t)
    {
        WaitTime = t;
    }

    protected override void Init()
    {
        //Debug.Log(WaitTime);
        if (WaitTime == 0)
        {
            SetState(TaskState.Success);
        }
    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;
        if (TimeCount >= WaitTime)
        {
            SetState(TaskState.Success);
        }
    }

}
