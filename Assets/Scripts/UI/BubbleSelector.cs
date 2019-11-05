using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BubbleSelector : MonoBehaviour
{
    public BubbleType Type;
    public float SelectedScale;
    public float DefaultScale;
    public float InflateTime;

    public float CurrentScale;

    public Color AvailableColor;
    public Color UsedUpColor;

    public float Size;

    public float FadeTime;

    private bool Remained;
    private bool Active;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.instance.AddHandler<BubbleNumSet>(OnBubbleNumSet);
        EventManager.instance.AddHandler<LevelFinish>(OnLevelFinish);
        EventManager.instance.AddHandler<CallActivateBubbleSelectors>(OnCallActivateBubbleSelector);
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveHandler<BubbleNumSet>(OnBubbleNumSet);
        EventManager.instance.RemoveHandler<LevelFinish>(OnLevelFinish);
        EventManager.instance.RemoveHandler<CallActivateBubbleSelectors>(OnCallActivateBubbleSelector);
    }

    // Update is called once per frame
    void Update()
    {
        CheckSelected();
        SetColor();
        SetScale();
    }

    private void SetScale()
    {
        if (GameManager.HeldBubbleType == Type)
        {
            CurrentScale += (SelectedScale - DefaultScale) / InflateTime * Time.deltaTime;
            if (CurrentScale > SelectedScale)
            {
                CurrentScale = SelectedScale;
            }
        }
        else
        {
            CurrentScale -= (SelectedScale - DefaultScale) / InflateTime * Time.deltaTime;
            if (CurrentScale < DefaultScale)
            {
                CurrentScale = DefaultScale;
            }
        }

        GetComponent<RectTransform>().localScale = CurrentScale * Vector3.one;
    }

    private void CheckSelected()
    {
        bool MouseIn = false;

        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);
        for (int i = 0; i < raycastResults.Count; i++)
        {
            if (raycastResults[i].gameObject == gameObject)
            {
                MouseIn = true;
            }
        }

        if ( Remained && Input.GetMouseButtonDown(0) && MouseIn)
        {
            EventManager.instance.Fire(new BubbleSelected(Type));
        }

    }
    
    private void OnBubbleNumSet(BubbleNumSet B)
    {
        if (B.Type == Type)
        {
            Transform Text = transform.Find("RemainedNumber");
            Text.GetComponent<Text>().text = B.Num.ToString();
            Color CurrentColor = GetComponent<Image>().color;

            if (B.Num == 0)
            {
                if (!Remained)
                {
                    GetComponent<Image>().color = new Color(CurrentColor.r, CurrentColor.g, CurrentColor.b, 0);
                    Text.GetComponent<Text>().color = new Color(1, 1, 1, 0);
                    Active = false;
                }
                else
                {
                    Remained = false;
                }
            }
            else
            {
                GetComponent<Image>().color = new Color(CurrentColor.r, CurrentColor.g, CurrentColor.b, 1);
                Text.GetComponent<Text>().color = Color.white;
                Remained = true;
                Active = true;
            }
        }
    }

    private void OnLevelFinish(LevelFinish L)
    {
        if (Active)
        {
            StartCoroutine(Fade(false));
        }
    }

    private void OnLevelLoaded(LevelLoaded L)
    {
        if (Active)
        {
            StartCoroutine(Fade(true));
        }
    }

    private void OnCallActivateBubbleSelector(CallActivateBubbleSelectors Call)
    {
        if (Active)
        {
            StartCoroutine(Fade(true));
        }
    }

    private void OnBackToMenu(BackToMenu Back)
    {
        if (Active)
        {
            StartCoroutine(Fade(false));
        }
    }



    public IEnumerator Fade(bool In)
    {
        if (In)
        {
            GetComponent<Image>().enabled = true;
            transform.Find("RemainedNumber").GetComponent<Text>().enabled = true;
        }

        Color ImageColor = GetComponent<Image>().color;
        Color TextColor = transform.Find("RemainedNumber").GetComponent<Text>().color;

        float TimeCount = 0;
        while (TimeCount < FadeTime)
        {
            TimeCount += Time.deltaTime;
            if (In)
            {
                GetComponent<Image>().color = Color.Lerp(new Color(ImageColor.r, ImageColor.g, ImageColor.b, 0), new Color(ImageColor.r, ImageColor.g, ImageColor.b, 1), TimeCount / FadeTime);
                transform.Find("RemainedNumber").GetComponent<Text>().color = Color.Lerp(new Color(TextColor.r, TextColor.g, TextColor.b, 0), new Color(TextColor.r, TextColor.g, TextColor.b, 1), TimeCount / FadeTime);
            }
            else
            {
                GetComponent<Image>().color = Color.Lerp(new Color(ImageColor.r, ImageColor.g, ImageColor.b, 1), new Color(ImageColor.r, ImageColor.g, ImageColor.b, 0), TimeCount / FadeTime);
                transform.Find("RemainedNumber").GetComponent<Text>().color = Color.Lerp(new Color(TextColor.r, TextColor.g, TextColor.b, 1), new Color(TextColor.r, TextColor.g, TextColor.b, 0), TimeCount / FadeTime);
            }
            yield return null;
        }

        if (!In)
        {
            GetComponent<Image>().enabled = false;
            transform.Find("RemainedNumber").GetComponent<Text>().enabled = false;
        }
    }

    private void SetColor()
    {
        if (Remained)
        {
            GetComponent<Image>().color = new Color(AvailableColor.r, AvailableColor.g, AvailableColor.b, GetComponent<Image>().color.a);
        }
        else
        {
            GetComponent<Image>().color = new Color(UsedUpColor.r, UsedUpColor.g, UsedUpColor.b, GetComponent<Image>().color.a);
        }
    }

}
