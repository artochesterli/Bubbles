using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationTask : Task
{
    public GameObject Obj;
    public float RotationAngle;
    public float RotationTime;

    private float Angle;
    private float TimeCount;

    private float OriAngle;

    public RotationTask(GameObject obj,float rotationangle,float rotationtime)
    {
        Obj = obj;
        RotationAngle = rotationangle;
        RotationTime = rotationtime;
    }

    protected override void Init()
    {
        OriAngle = Obj.transform.eulerAngles.z;
    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;
        Angle = RotationAngle * TimeCount / RotationTime;
        if (Mathf.Abs(Angle) > Mathf.Abs(RotationAngle))
        {
            Angle = RotationAngle;
        }

        Obj.transform.rotation = Quaternion.Euler(0, 0, OriAngle - Angle);

        if (TimeCount >= RotationTime)
        {
            SetState(TaskState.Success);
        }
    }
}
