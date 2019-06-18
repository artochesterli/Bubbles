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

    private bool Remained;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.instance.AddHandler<LevelLoaded>(OnLevelLoaded);
        EventManager.instance.AddHandler<BubbleNumSet>(OnBubbleNumSet);
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveHandler<LevelLoaded>(OnLevelLoaded);
        EventManager.instance.RemoveHandler<BubbleNumSet>(OnBubbleNumSet);
    }

    // Update is called once per frame
    void Update()
    {
        CheckSelected();
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
        if (EventSystem.current.IsPointerOverGameObject() && Remained && Input.GetMouseButtonDown(0))
        {
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = Input.mousePosition;

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);

            for(int i=0; i< raycastResults.Count; i++)
            {
                if (raycastResults[i].gameObject==gameObject)
                {
                    GameManager.HeldBubbleType = Type;
                    return;
                }
            }
        }
    }

    /*private void SetText()
    {
        switch (Type)
        {
            case BubbleType.Disappear:
                num = LevelManager.RemainedDisappearBubble;
                break;
            case BubbleType.Normal:
                num = LevelManager.RemainedNormalBubble;
                break;
            default:
                break;
        }
        transform.Find("RemainedNumber").GetComponent<Text>().text = num.ToString();
    }*/

    private void OnLevelLoaded(LevelLoaded L)
    {
        Remained = false;
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
                }
                Remained = false;
            }
            else
            {
                GetComponent<Image>().color = new Color(CurrentColor.r, CurrentColor.g, CurrentColor.b, 1);
                Text.GetComponent<Text>().color = Color.white;
                Remained = true;
            }


        }
    }

}
