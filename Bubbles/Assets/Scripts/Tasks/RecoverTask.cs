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
    private readonly Color EnergyColor;

    private float TimeCount;
    private GameObject StableEffect;
    private GameObject ActivateEffect;
    private GameObject ReleaseEffect;

    public RecoverTask(GameObject obj,float time,float initScale,float finishScale,Color initColor,Color finishColor, List<List<SlotInfo>> map,Vector2Int pos,Color energyColor)
    {
        Obj = obj;
        RecoverTime = time;
        InitScale = initScale;
        FinishScale = finishScale;
        InitColor = initColor;
        FinishColor = finishColor;
        Map = map;
        Pos = pos;
        EnergyColor = energyColor;
        SetMapInfo();
    }

    protected override void Init()
    {
        Obj.GetComponent<Bubble>().State = BubbleState.Stable;
        Obj.transform.localScale = InitScale * Vector3.one;
        Obj.GetComponent<SpriteRenderer>().color = InitColor;
        Obj.GetComponent<CircleCollider2D>().enabled = true;

        StableEffect = Obj.transform.Find("StableEffect").gameObject;
        StableEffect.GetComponent<ParticleSystem>().Play();

        ActivateEffect= Obj.transform.Find("ActivateEffect").gameObject;
        ActivateEffect.GetComponent<ParticleSystem>().Stop();
        ReleaseEffect= Obj.transform.Find("ReleaseEffect").gameObject;

        if (RecoverTime == 0)
        {
            if (Map[Pos.x][Pos.y].slotType == SlotType.Target && Obj.GetComponent<Bubble>().Type == BubbleType.Normal)
            {
                Obj.transform.Find("InTargetEffect").GetComponent<ParticleSystem>().Play();
            }
            Obj.GetComponent<SpriteRenderer>().sortingLayerName = "Circle";
            SetState(TaskState.Success);
        }

        ActivateEffect.GetComponent<ParticleSystem>().startColor = EnergyColor;
        foreach(Transform child in ReleaseEffect.transform)
        {
            child.GetComponent<ParticleSystem>().startColor = EnergyColor;
        }
    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;

        Obj.transform.localScale = Vector3.Lerp(InitScale * Vector3.one, FinishScale * Vector3.one, TimeCount / RecoverTime);
        Obj.GetComponent<SpriteRenderer>().color = Color.Lerp(InitColor, FinishColor, TimeCount / RecoverTime);

        if (TimeCount >= RecoverTime)
        {
            if(Map[Pos.x][Pos.y].slotType==SlotType.Target&& Obj.GetComponent<Bubble>().Type == BubbleType.Normal)
            {
                Obj.transform.Find("InTargetEffect").GetComponent<ParticleSystem>().Play();
            }

            Obj.GetComponent<SpriteRenderer>().sortingLayerName = "Circle";
            SetState(TaskState.Success);
        }
    }

    private void SetMapInfo()
    {
        Map[Pos.x][Pos.y].InsideBubbleState = BubbleState.Stable;
    }

}
