using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearTask : Task
{
    private readonly GameObject Obj;
    private readonly float DisappearTime;
    private readonly Vector2Int Pos;
    private readonly List<List<SlotInfo>> Map;
    private readonly BubbleType Type;
    private readonly bool RollBack;

    private float TimeCount;
    private Color color;
    private GameObject ActivateEffect;

    public DisappearTask(GameObject obj, float time, Vector2Int pos, List<List<SlotInfo>> map, BubbleType type,bool rollback)
    {
        Obj = obj;
        DisappearTime = time;
        Pos = pos;
        Map = map;
        Type = type;
        RollBack = rollback;
        
    }

    protected override void Init()
    {
        Map[Pos.x][Pos.y].InsideBubbleType = BubbleType.Null;
        Map[Pos.x][Pos.y].ConnectedBubble = null;

        if (RollBack)
        {
            switch (Type)
            {
                case BubbleType.Disappear:
                    LevelManager.RemainedDisappearBubble++;
                    break;
                case BubbleType.Normal:
                    LevelManager.RemainedNormalBubble++;
                    break;
            }
        }

        color = Obj.GetComponent<SpriteRenderer>().color;

        ActivateEffect = Obj.transform.Find("ActivateEffect").gameObject;
        ActivateEffect.GetComponent<ParticleSystem>().Stop();
        
    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;
        Obj.GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(color.r, color.g, color.b, 1), new Color(color.r, color.g, color.b, 0), TimeCount / DisappearTime);
        if (TimeCount >= DisappearTime)
        {
            if (Type == BubbleType.Disappear || RollBack)
            {
                GameObject.Destroy(Obj);
            }
            SetState(TaskState.Success);
        }
    }
}
