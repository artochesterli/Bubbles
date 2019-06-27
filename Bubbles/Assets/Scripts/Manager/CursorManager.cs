using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    public static bool InAvailableSlot;

    public Color DisappearBubbleColor;
    public Color NormalBubbleColor;
    public Color GreyDisappearBubbleColor;
    public Color GreyNormalBubbleColor;

    public float ColorChangeTime;
    public float Scale;
    public float InflatedScale;

    public float InSlotScale;
    public float InSlotScaleChangeTime;
    

    private bool ColorChanging;
    private Color CurrentColor;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        EventManager.instance.AddHandler<BubbleSelected>(OnBubbleSelected);
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveHandler<BubbleSelected>(OnBubbleSelected);
    }

    // Update is called once per frame
    void Update()
    {
        SetPos();
        SetAppearance();
        SetScale();
    }

    private void SetScale()
    {
        if (InAvailableSlot)
        {
            transform.localScale += Vector3.one * (InSlotScale - Scale) / InSlotScaleChangeTime * Time.deltaTime;
            if (transform.localScale.x > InSlotScale)
            {
                transform.localScale = Vector3.one * InSlotScale;
            }
        }
        else
        {
            transform.localScale -= Vector3.one * (InSlotScale - Scale) / InSlotScaleChangeTime * Time.deltaTime;
            if (transform.localScale.x < Scale)
            {
                transform.localScale = Vector3.one * Scale;
            }
        }
    }

    private void SetAppearance()
    {
        if (!ColorChanging)
        {
            switch (GameManager.State)
            {
                case GameState.Play:
                    switch (GameManager.HeldBubbleType)
                    {
                        case BubbleType.Disappear:
                            GetComponent<Image>().color = DisappearBubbleColor;
                            CurrentColor = DisappearBubbleColor;
                            break;
                        case BubbleType.Normal:
                            GetComponent<Image>().color = NormalBubbleColor;
                            CurrentColor = NormalBubbleColor;
                            break;
                        default:
                            break;
                    }
                    break;
                case GameState.Show:
                    switch (GameManager.HeldBubbleType)
                    {
                        case BubbleType.Disappear:
                            GetComponent<Image>().color = GreyDisappearBubbleColor;
                            CurrentColor = GreyDisappearBubbleColor;
                            break;
                        case BubbleType.Normal:
                            GetComponent<Image>().color = GreyNormalBubbleColor;
                            CurrentColor = GreyNormalBubbleColor;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            
        }
    }

    private void SetPos()
    {
        GetComponent<RectTransform>().position = Input.mousePosition;
    }

    private IEnumerator ChangeColor(Color Start, Color End)
    {
        ColorChanging = true;
        transform.localScale = Scale * Vector3.one;
        float TimeCount = 0;
        while (TimeCount < ColorChangeTime / 2)
        {
            TimeCount += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Scale*Vector3.one, InflatedScale*Vector3.one, 2 * TimeCount / ColorChangeTime);
            GetComponent<Image>().color = Color.Lerp(Start, End, TimeCount / ColorChangeTime);
            yield return null;
        }

        while(TimeCount < ColorChangeTime)
        {
            TimeCount += Time.deltaTime;
            transform.localScale = Vector3.Lerp(InflatedScale * Vector3.one, Scale * Vector3.one, (2 * TimeCount - ColorChangeTime) / ColorChangeTime);
            GetComponent<Image>().color = Color.Lerp(Start, End, TimeCount / ColorChangeTime);
            yield return null;
        }
        CurrentColor = End;
        ColorChanging = false;
    }

    private void OnBubbleSelected(BubbleSelected B)
    {
        StopAllCoroutines();
        switch (GameManager.State)
        {
            case GameState.Play:
                switch (B.Type)
                {
                    case BubbleType.Disappear:
                        StartCoroutine(ChangeColor(CurrentColor, DisappearBubbleColor));
                        break;
                    case BubbleType.Normal:
                        StartCoroutine(ChangeColor(CurrentColor, NormalBubbleColor));
                        break;
                }
                break;
            case GameState.Show:
                switch (B.Type)
                {
                    case BubbleType.Disappear:
                        StartCoroutine(ChangeColor(CurrentColor, GreyDisappearBubbleColor));
                        break;
                    case BubbleType.Normal:
                        StartCoroutine(ChangeColor(CurrentColor, GreyNormalBubbleColor));
                        break;
                }
                break;
        }
        
    }

}
