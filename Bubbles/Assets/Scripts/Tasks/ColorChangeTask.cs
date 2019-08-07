using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChangeTask : Task
{
    public GameObject Obj;
    public Color Start;
    public Color End;
    public float ChangeTime;

    private float TimeCount;

    public ColorChangeTask(GameObject obj, Color start,Color end,float time)
    {
        Obj = obj;
        Start = start;
        End = end;
        ChangeTime = time;
    }

    protected override void Init()
    {
        Obj.GetComponent<SpriteRenderer>().color = Start;
    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;
        Obj.GetComponent<SpriteRenderer>().color = Color.Lerp(Start, End, TimeCount / ChangeTime);
        if (TimeCount >= ChangeTime)
        {
            SetState(TaskState.Success);
        }
    }

}
