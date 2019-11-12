using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ColorChangeType
{
    Sprite,
    Image,
    Text
}

public class ColorChangeTask : Task
{

    private readonly GameObject Obj;
    private readonly Color Start;
    private readonly Color End;
    private readonly float ChangeTime;
    private readonly ColorChangeType Type;

    private float TimeCount;

    public ColorChangeTask(GameObject obj, Color start,Color end,float time, ColorChangeType type)
    {
        Obj = obj;
        Start = start;
        End = end;
        ChangeTime = time;
        Type = type;
    }

    protected override void Init()
    {
        switch (Type)
        {
            case ColorChangeType.Sprite:
                Obj.GetComponent<SpriteRenderer>().color = Start;
                break;
            case ColorChangeType.Image:
                Obj.GetComponent<Image>().color = Start;
                break;
            case ColorChangeType.Text:
                Obj.GetComponent<Text>().color = Start;
                break;
        }
    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;

        switch (Type)
        {
            case ColorChangeType.Sprite:
                Obj.GetComponent<SpriteRenderer>().color = Color.Lerp(Start, End, TimeCount / ChangeTime);
                break;
            case ColorChangeType.Image:
                Obj.GetComponent<Image>().color = Color.Lerp(Start, End, TimeCount / ChangeTime);
                break;
            case ColorChangeType.Text:
                Obj.GetComponent<Text>().color = Color.Lerp(Start, End, TimeCount / ChangeTime);
                break;
        }

        if (TimeCount >= ChangeTime)
        {
            SetState(TaskState.Success);
        }
    }

}
