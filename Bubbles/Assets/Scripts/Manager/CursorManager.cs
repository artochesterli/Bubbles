using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    public static GameObject AllSlot;

    public Color NullColor;
    public Color DisappearBubbleColor;
    public Color NormalBubbleColor;
    public Color ExhaustDisappearBubbleColor;
    public Color ExhaustNormalBubbleColor;

    public float ColorChangeTime;
    public float Scale;
    public float InflatedScale;

    public float InSlotScale;
    public float InSlotScaleChangeTime;


    private GameObject SelectedSlot;

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
        AllSlot = null;
        EventManager.instance.RemoveHandler<BubbleSelected>(OnBubbleSelected);
    }

    // Update is called once per frame
    void Update()
    {
        SetPos();
        CheckSlotSelection();
        SetAppearance();
        SetScale();
        CheckInput();
    }

    private void CheckSlotSelection()
    {
        if(GameManager.State == GameState.Play)
        {
            foreach (Transform child in AllSlot.transform)
            {
                if (child.GetComponent<SlotObject>().CursorInside() && child.GetComponent<SlotObject>().AvailablePos())
                {
                    if (child.gameObject != SelectedSlot)
                    {
                        if (SelectedSlot != null)
                        {
                            SelectedSlot.GetComponent<SlotObject>().Selected = false;
                        }
                        child.GetComponent<SlotObject>().Selected = true;
                        SelectedSlot = child.gameObject;
                    }
                    return;
                }
            }
        }

        if (SelectedSlot != null)
        {
            SelectedSlot.GetComponent<SlotObject>().Selected = false;
            SelectedSlot = null;
        }

    }

    private void CheckInput()
    {
        if (Input.GetMouseButtonDown(0) && SelectedSlot != null)
        {
            SelectedSlot.GetComponent<SlotObject>().Selected = false;
            EventManager.instance.Fire(new Place(SelectedSlot.GetComponent<SlotObject>().ConnectedSlotInfo.Pos, GameManager.HeldBubbleType));
        }
    }

    private void SetScale()
    {
        if (SelectedSlot)
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
        GameObject ActivateEffect = transform.Find("ActivateEffect").gameObject;

        if (!ColorChanging)
        {
            switch (GameManager.State)
            {
                case GameState.Play:
                    if (!ActivateEffect.GetComponent<ParticleSystem>().isPlaying)
                    {
                        ActivateEffect.GetComponent<ParticleSystem>().Play();
                    }

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
                            GetComponent<Image>().color = NullColor;
                            CurrentColor = NullColor;
                            ActivateEffect.GetComponent<ParticleSystem>().Stop();
                            break;
                    }

                    
                    break;
                case GameState.Run:
                    switch (GameManager.HeldBubbleType)
                    {
                        case BubbleType.Disappear:
                            GetComponent<Image>().color = ExhaustDisappearBubbleColor;
                            CurrentColor = ExhaustDisappearBubbleColor;
                            break;
                        case BubbleType.Normal:
                            GetComponent<Image>().color = ExhaustNormalBubbleColor;
                            CurrentColor = ExhaustNormalBubbleColor;
                            break;
                        default:
                            GetComponent<Image>().color = NullColor;
                            CurrentColor = NullColor;
                            break;
                    }

                    ActivateEffect.GetComponent<ParticleSystem>().Stop();
                    break;
                default:
                    break;
            }
            
        }
    }

    private void SetPos()
    {

        GetComponent<RectTransform>().position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        GetComponent<RectTransform>().position -= GetComponent<RectTransform>().position.z * Vector3.forward;
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
            case GameState.Run:
                switch (B.Type)
                {
                    case BubbleType.Disappear:
                        StartCoroutine(ChangeColor(CurrentColor, ExhaustDisappearBubbleColor));
                        break;
                    case BubbleType.Normal:
                        StartCoroutine(ChangeColor(CurrentColor, ExhaustNormalBubbleColor));
                        break;
                }
                break;
        }
        
    }

}
