using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReleaseTask : Task
{
    private readonly GameObject Obj;
    private readonly float ReleaseTime;
    private readonly float InitScale;
    private readonly float FinishScale;
    private readonly Color InitColor;
    private readonly Color FinishColor;
    private readonly List<List<SlotInfo>> Map;
    private readonly Vector2Int Pos;

    private float TimeCount;
    private GameObject ActivateEffect;
    private GameObject ReleaseEffect;

    public ReleaseTask(GameObject obj,float time,float initScale,float finishScale,Color initColor,Color finishColor,List<List<SlotInfo>> map, Vector2Int pos)
    {
        Obj = obj;
        ReleaseTime = time;
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
        Obj.GetComponent<Bubble>().State = BubbleState.Exhausted;
        Obj.transform.localScale = InitScale * Vector3.one;
        Obj.GetComponent<SpriteRenderer>().color = InitColor;

        ActivateEffect = Obj.transform.Find("ActivateEffect").gameObject;
        ReleaseEffect = Obj.transform.Find("ReleaseEffect").gameObject;

        ActivateEffect.GetComponent<ParticleSystem>().Stop();

        foreach(Transform child in ReleaseEffect.transform)
        {
            child.GetComponent<ParticleSystem>().Play();
        }
    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;

        Obj.transform.localScale = Vector3.Lerp(InitScale * Vector3.one, FinishScale * Vector3.one, TimeCount / ReleaseTime);
        Obj.GetComponent<SpriteRenderer>().color = Color.Lerp(InitColor, FinishColor, TimeCount / ReleaseTime);

        if (TimeCount >= ReleaseTime)
        {
            foreach (Transform child in ReleaseEffect.transform)
            {
                child.GetComponent<ParticleSystem>().Stop();
            }

            SetState(TaskState.Success);
        }
    }

    private void SetMapInfo()
    {
        Map[Pos.x][Pos.y].InsideBubbleState = BubbleState.Exhausted;
    }
}
