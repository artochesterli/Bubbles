using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionFinishTask : Task
{
    protected override void Init()
    {
        SetState(TaskState.Success);
        EventManager.instance.Fire(new MotionFinish());
    }
}
