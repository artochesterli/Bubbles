using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFillTask : Task
{
    private readonly GameObject Obj;
    private readonly float StartFill;
    private readonly float EndFill;
    private readonly float StateTime;

    private float TimeCount;

    public UIFillTask(GameObject obj, float startfill, float endfill, float time)
    {
        Obj = obj;
        StartFill = startfill;
        EndFill = endfill;
        StateTime = time;
    }

    protected override void Init()
    {
        base.Init();
        TimeCount = 0;
        if(StateTime <= 0)
        {
            SetState(TaskState.Success);
        }
    }

    internal override void Update()
    {
        base.Update();
        TimeCount += Time.deltaTime;
        Obj.GetComponent<Image>().fillAmount = Mathf.Lerp(StartFill, EndFill, TimeCount / StateTime);
        if (TimeCount > StateTime)
        {
            SetState(TaskState.Success);
        }
    }
}
