using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySelfTask : Task
{
    private readonly GameObject Obj;
    
    public DestroySelfTask(GameObject obj)
    {
        Obj = obj;
    }

    protected override void Init()
    {
        base.Init();
        GameObject.Destroy(Obj);
        SetState(TaskState.Success);
    }
}
