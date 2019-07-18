using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoverTask : Task
{
    private readonly GameObject Obj;
    private readonly float RecoverTime;
    private readonly float InitScale;
    private readonly float FinishScale;
    private readonly Color InitColor;
    private readonly Color FinishColor;
    private readonly List<List<SlotInfo>> Map;
    private readonly Vector2Int Pos;

    private float TimeCount;
    private GameObject StableEffect;

    public RecoverTask(GameObject obj,float time,float initScale,float finishScale,Color initColor,Color finishColor, List<List<SlotInfo>> map,Vector2Int pos)
    {
        Obj = obj;
        RecoverTime = time;
        InitScale = initScale;
        FinishScale = finishScale;
        InitColor = initColor;
        FinishColor = finishColor;
        Map = map;
        Pos = pos;
        SetMapInfo();
    }

    protected override void Init()
    {
        Obj.GetComponent<Bubble>().State = BubbleState.Stable;
        Obj.transform.localScale = InitScale * Vector3.one;
        Obj.GetComponent<SpriteRenderer>().color = InitColor;

        StableEffect = Obj.transform.Find("StableEffect").gameObject;
        StableEffect.GetComponent<ParticleSystem>().Play();

    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;

        Obj.transform.localScale = Vector3.Lerp(InitScale * Vector3.one, FinishScale * Vector3.one, TimeCount / RecoverTime);
        Obj.GetComponent<SpriteRenderer>().color = Color.Lerp(InitColor, FinishColor, TimeCount / RecoverTime);

        if (TimeCount >= RecoverTime)
        {
            SetState(TaskState.Success);
        }
    }

    private void SetMapInfo()
    {
        Map[Pos.x][Pos.y].InsideBubbleState = BubbleState.Stable;
    }

}
