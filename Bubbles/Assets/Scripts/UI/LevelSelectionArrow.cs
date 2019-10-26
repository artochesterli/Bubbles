using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionArrow : MonoBehaviour
{
    public bool Right;
    public float AppearTime;
    public float DisappearTime;
    public Color color;

    public float ShakeTime;
    public float ShakeDistance;

    private bool LevelButtonClicked;
    private Vector2 OriPos;
    private bool Shaking;

    // Start is called before the first frame update
    void Start()
    {
        OriPos = GetComponent<RectTransform>().localPosition;
        EventManager.instance.AddHandler<TransferToLevelPlay>(OnTransferToLevelPlay);
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveHandler<TransferToLevelPlay>(OnTransferToLevelPlay);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        if (!Shaking&&!LevelButtonClicked)
        {
            StartCoroutine(ClickedShake());
            if (Right)
            {
                transform.parent.GetComponent<SelectLevelMenuManager>().IncreaseLevel();
            }
            else
            {
                transform.parent.GetComponent<SelectLevelMenuManager>().DecreaseLevel();
            }
        }
    }

    private IEnumerator Disappeear()
    {
        float TimeCount = 0;
        while (TimeCount < DisappearTime)
        {
            TimeCount += Time.deltaTime;
            GetComponent<Image>().color = new Color(color.r, color.g, color.b, 1 - TimeCount / DisappearTime);
            yield return null;
        }
    }

    private IEnumerator ClickedShake()
    {
        Shaking = true;

        Vector2 Direction;
        if (Right)
        {
            Direction = Vector2.right;
        }
        else
        {
            Direction = Vector2.left;
        }

        float TimeCount = 0;
        while (TimeCount < ShakeTime)
        {
            TimeCount += Time.deltaTime;
            float Factor;
            if (TimeCount < ShakeTime/2)
            {
                Factor = TimeCount / (ShakeTime / 2);
            }
            else
            {
                Factor = 1 - (TimeCount-ShakeTime/2) / (ShakeTime / 2);
            }

            GetComponent<RectTransform>().localPosition = Vector2.Lerp(OriPos, OriPos + Direction * ShakeDistance, Factor);

            yield return null;
        }

        Shaking = false;
    }

    private void OnTransferToLevelPlay(TransferToLevelPlay T)
    {
        LevelButtonClicked = true;
        StartCoroutine(Disappeear());
    }
}
