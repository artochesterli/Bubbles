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

    public RotationTask(GameObject obj,float rotationangle,float rotationtime)
    {
        Obj = obj;
        RotationAngle = rotationangle;
        RotationTime = rotationtime;
    }

    protected override void Init()
    {
        Angle = Obj.transform.rotation.z;
    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;
        Angle = RotationAngle * TimeCount / RotationTime;
        if (Angle > RotationAngle)
        {
            Angle = RotationAngle;
        }
        Obj.transform.rotation = Quaternion.Euler(0, 0, Angle);
        if (TimeCount >= RotationTime)
        {
            SetState(TaskState.Success);
        }
    }
}
