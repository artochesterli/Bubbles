using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveOutBackDecelerationTask : Task
{
    private readonly GameObject Obj;
    private readonly float MoveBackDis;
    private readonly float MoveBackTime;
    private readonly Vector2 MoveOutBasicDirection;
    private readonly float PowerUpSelfInflatedScale;

    private float MaxBackSpeed;
    private float BackSpeed;
    private float TimeCount;

    public MoveOutBackDecelerationTask(GameObject obj, float backdis, float backtime, Vector2 dir, float inflatedscale)
    {
        Obj = obj;
        MoveBackDis = backdis;
        MoveBackTime = backtime;
        MoveOutBasicDirection = dir;
        PowerUpSelfInflatedScale = inflatedscale;
    }

    protected override void Init()
    {
        base.Init();
        MaxBackSpeed = MoveBackDis / MoveBackTime;
    }

    internal override void Update()
    {
        base.Update();
        TimeCount += Time.deltaTime;
        BackSpeed = Mathf.Lerp(MaxBackSpeed, 0, TimeCount / (MoveBackTime / 2));

        Obj.transform.localScale = Vector3.Lerp(Vector3.one * PowerUpSelfInflatedScale, Vector3.one, (TimeCount+MoveBackTime/2) / MoveBackTime);
        Obj.transform.position -= (Vector3)MoveOutBasicDirection * BackSpeed * Time.deltaTime;

        if (TimeCount >= MoveBackTime / 2)
        {
            SetState(TaskState.Success);
        }
    }
}
