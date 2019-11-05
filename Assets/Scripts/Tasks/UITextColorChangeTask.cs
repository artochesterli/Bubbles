using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITextColorChangeTask : Task
{
    private readonly GameObject Obj;
    private readonly Color StartColor;
    private readonly Color EndColor;
    private readonly float StateTime;

    private float TimeCount;

    public UITextColorChangeTask(GameObject obj, Color startcolor, Color endcolor, float time)
    {
        Obj = obj;
        StartColor = startcolor;
        EndColor = endcolor;
        StateTime = time;
    }


    protected override void Init()
    {
        base.Init();
        TimeCount = 0;
        if (StateTime <= 0)
        {
            SetState(TaskState.Success);
        }
    }

    internal override void Update()
    {
        base.Update();

        TimeCount += Time.deltaTime;
        Obj.GetComponent<Text>().color = Color.Lerp(StartColor, EndColor, TimeCount / StateTime);
        if (TimeCount > StateTime)
        {
            SetState(TaskState.Success);
        }
    }
}
