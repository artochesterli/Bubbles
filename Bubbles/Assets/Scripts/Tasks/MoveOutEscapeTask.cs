using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveOutEscapeTask : Task
{
    private readonly GameObject Obj;
    private readonly Vector2 StartPoint;
    private readonly Vector2 EndPoint;
    private readonly Vector2 MidPoint;
    private readonly float MoveOutTime;
    private readonly float MoveOutMaxTimeScale;
    private readonly float MoveOutAcTimePercentage;

    private float TimeCount;
    private float TimeScale;
    private Color color;

    public MoveOutEscapeTask(GameObject obj, Vector2 start, Vector2 end, Vector2 mid, float time, float maxtimescale, float actime)
    {
        Obj = obj;
        StartPoint = start;
        EndPoint = end;
        MidPoint = mid;
        MoveOutTime = time;
        MoveOutMaxTimeScale = maxtimescale;
        MoveOutAcTimePercentage = actime;
    }

    protected override void Init()
    {
        base.Init();
        color = Obj.GetComponent<SpriteRenderer>().color;
    }

    internal override void Update()
    {
        base.Update();
        TimeCount += Time.deltaTime * TimeScale;

        if (TimeCount < MoveOutTime * MoveOutAcTimePercentage)
        {
            TimeScale += MoveOutMaxTimeScale / (MoveOutAcTimePercentage * MoveOutTime) * Time.deltaTime;
        }
        else
        {
            TimeScale -= (2 * MoveOutMaxTimeScale - (2 - MoveOutMaxTimeScale * MoveOutAcTimePercentage) / (1 - MoveOutAcTimePercentage)) / ((1 - MoveOutAcTimePercentage) * MoveOutTime) * Time.deltaTime;
        }
        Vector2 v1 = Vector2.Lerp(StartPoint, MidPoint, TimeCount / MoveOutTime);
        Vector2 v2 = Vector2.Lerp(MidPoint, EndPoint, TimeCount / MoveOutTime);
        Obj.transform.position = Vector2.Lerp(v1, v2, TimeCount / MoveOutTime);
        Obj.GetComponent<SpriteRenderer>().color = Color.Lerp(Utility.ColorWithAlpha(color, 1), Utility.ColorWithAlpha(color,0), TimeCount / MoveOutTime);

        if(TimeCount >= MoveOutTime)
        {
            SetState(TaskState.Success);
        }
    }
}
