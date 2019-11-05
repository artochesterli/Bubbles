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
    private readonly List<bool> ShootParticles;

    private float TimeCount;
    private GameObject ActivateEffect;
    private GameObject ReleaseEffect;
    private GameObject EmptyReleaseEffect;

    public ReleaseTask(GameObject obj,float time,float initScale,float finishScale,Color initColor,Color finishColor,List<List<SlotInfo>> map, Vector2Int pos, List<bool> shootParticles)
    {
        Obj = obj;
        ReleaseTime = time;
        InitScale = initScale;
        FinishScale = finishScale;
        InitColor = initColor;
        FinishColor = finishColor;
        Map = map;
        Pos = pos;
        ShootParticles = shootParticles;
        SetMapInfo();
    }

    protected override void Init()
    {
        Obj.GetComponent<Bubble>().State = BubbleState.Exhausted;
        Obj.transform.localScale = InitScale * Vector3.one;
        Obj.GetComponent<SpriteRenderer>().color = InitColor;

        Obj.GetComponent<CircleCollider2D>().enabled = false;

        ActivateEffect = Obj.transform.Find("ActivateEffect").gameObject;
        ReleaseEffect = Obj.transform.Find("ReleaseEffect").gameObject;
        EmptyReleaseEffect = Obj.transform.Find("EmptyReleaseEffect").gameObject;

        ActivateEffect.GetComponent<ParticleSystem>().Stop();

        bool Push = false;

        for(int i = 0; i < ReleaseEffect.transform.childCount; i++)
        {
            if (ShootParticles[i])
            {
                ReleaseEffect.transform.GetChild(i).GetComponent<ParticleSystem>().Play();
                Push = true;
            }
        }

        if (!Push)
        {
            EmptyReleaseEffect.GetComponent<ParticleSystem>().Play();
        }

        if (Obj.GetComponent<Bubble>().Type == BubbleType.Normal)
        {
            //Obj.transform.Find("InTargetEffect").GetComponent<ParticleSystem>().Stop();
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
