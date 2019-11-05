using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShakeTask : Task
{
    public GameObject Obj;
    public float ShakeDis;
    public float ShakeTime;
    public int ShakeCycle;

    private float TimeCount;
    private float OriPos;

    public ShakeTask(GameObject obj, float dis,float time,int cycle)
    {
        Obj = obj;
        ShakeDis = dis;
        ShakeTime = time;
        ShakeCycle = cycle;
    }

    protected override void Init()
    {
        OriPos = Obj.transform.localPosition.x;
    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;
        Obj.transform.localPosition = new Vector3(OriPos, Obj.transform.localPosition.y, Obj.transform.localPosition.z) + Vector3.right * Mathf.Sin(TimeCount / ShakeTime * ShakeCycle * 2 * Mathf.PI)*ShakeDis;
        if (TimeCount >= ShakeTime)
        {
            Obj.transform.localPosition = new Vector3(OriPos, Obj.transform.localPosition.y, Obj.transform.localPosition.z);
            SetState(TaskState.Success);
        }

    }
}
