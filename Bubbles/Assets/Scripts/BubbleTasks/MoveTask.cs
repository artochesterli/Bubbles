using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTask : Task
{
    private readonly GameObject Obj;
    private readonly Vector3 Pos;
    private readonly Vector3 Direction;
    private readonly float MoveDis;
    private readonly float MoveTime;

    private float TimeCount;

    public MoveTask(GameObject obj, Vector3 pos, Vector3 dir, float dis, float time)
    {
        Obj = obj;
        Pos = pos;
        Direction = dir;
        MoveDis = dis;
        MoveTime = time;
    }

    protected override void Init()
    {
        Obj.GetComponent<Bubble>().State = BubbleState.Moving;
        Obj.transform.localPosition = Pos;
        TimeCount = 0;
    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;
        Obj.transform.localPosition = Vector3.Lerp(Pos, Pos + Direction * MoveDis, TimeCount / MoveTime);

        if (TimeCount >= MoveTime)
        {
            SetState(TaskState.Success);
        }
    }
}
