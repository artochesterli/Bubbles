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

    private int num;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        CheckSelected();
        SetScale();
        SetText();
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
        if (EventSystem.current.IsPointerOverGameObject() && num > 0 && Input.GetMouseButtonDown(0))
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

    private void SetText()
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
    }
}
