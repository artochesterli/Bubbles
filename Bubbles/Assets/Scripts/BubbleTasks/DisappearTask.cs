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

    private float TimeCount;
    private Color color;

    public DisappearTask(GameObject obj, float time, Vector2Int pos, List<List<SlotInfo>> map, BubbleType type)
    {
        Obj = obj;
        DisappearTime = time;
        Pos = pos;
        Map = map;
        Type = type;
        color = Obj.GetComponent<Bubble>().StableColor;

    }

    protected override void Init()
    {
        switch (Type)
        {
            case BubbleType.Disappear:
                LevelManager.RemainedDisappearBubble++;
                EventManager.instance.Fire(new BubbleNumSet(BubbleType.Disappear, LevelManager.RemainedDisappearBubble));
                break;
            case BubbleType.Normal:
                LevelManager.RemainedNormalBubble++;
                EventManager.instance.Fire(new BubbleNumSet(BubbleType.Normal, LevelManager.RemainedNormalBubble));
                break;
        }

        Map[Pos.x][Pos.y].InsideBubbleType = BubbleType.Null;
        Map[Pos.x][Pos.y].ConnectedBubble = null;
    }

    internal override void Update()
    {
        if (Type != BubbleType.Disappear)
        {
            TimeCount += Time.deltaTime;
            Obj.GetComponent<SpriteRenderer>().color = Color.Lerp(color, new Color(color.r, color.g, color.b, 0), TimeCount / DisappearTime);
            if (TimeCount >= DisappearTime)
            {
                GameObject.Destroy(Obj);
                SetState(TaskState.Success);
            }
        }
        else
        {
            SetState(TaskState.Success);
        }
    }
}
