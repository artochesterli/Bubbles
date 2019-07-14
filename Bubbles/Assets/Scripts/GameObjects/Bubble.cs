using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BubbleType
{
    Null,
    Disappear,
    Normal
}

public enum BubbleState
{
    Stable,
    Activated,
    Exhausted
}

public class Bubble : MonoBehaviour
{
    public BubbleType Type;
    public BubbleState State;
    public Color StableColor;
    public Color ActivatedColor;
    public Color ExhaustedColor;

    public float ColorChangeTime;

    private Color CurrentColor;
    private Color Buffer;
    private float Timer;

    private void Start()
    {
        CurrentColor = StableColor;
        Buffer = StableColor;
    }

    private void Update()
    {
        //SetAppeaance();
    }

    private void SetAppeaance()
    {
        Timer += Time.deltaTime;

        switch (State)
        {
            case BubbleState.Stable:
                if (CurrentColor != StableColor)
                {
                    Buffer = CurrentColor;
                    CurrentColor = StableColor;
                    Timer = 0;
                }
                GetComponent<SpriteRenderer>().color = Color.Lerp(Buffer, StableColor, Timer / ColorChangeTime);
                break;
            case BubbleState.Activated:
                if (CurrentColor != ActivatedColor)
                {
                    Buffer = CurrentColor;
                    CurrentColor = ActivatedColor;
                    Timer = 0;
                }
                GetComponent<SpriteRenderer>().color = Color.Lerp(Buffer, ActivatedColor, Timer / ColorChangeTime);
                break;
            case BubbleState.Exhausted:
                if (CurrentColor != ExhaustedColor)
                {
                    Buffer = CurrentColor;
                    CurrentColor = ExhaustedColor;
                    Timer = 0;
                }
                GetComponent<SpriteRenderer>().color = Color.Lerp(Buffer, ExhaustedColor, Timer / ColorChangeTime);
                break;
        }
    }

}


