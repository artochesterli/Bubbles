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
    }

    private void OnDestroy()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        if (!Shaking&&GameManager.gameState == GameState.SelectLevelMenu)
        {
            StartCoroutine(ClickedShake());
            if (Right)
            {
                transform.root.GetComponent<SelectLevelMenuManager>().IncreaseLevel();
            }
            else
            {
                transform.root.GetComponent<SelectLevelMenuManager>().DecreaseLevel();
            }
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

    public ColorChangeTask GetDisappearTask()
    {
        Color color = GetComponent<Image>().color;

        return new ColorChangeTask(gameObject, Utility.ColorWithAlpha(color, 1), Utility.ColorWithAlpha(color, 0), DisappearTime, ColorChangeType.Image);
    }

    public ColorChangeTask GetAppearTask()
    {
        Color color = GetComponent<Image>().color;

        return new ColorChangeTask(gameObject, Utility.ColorWithAlpha(color, 0), Utility.ColorWithAlpha(color, 1), AppearTime, ColorChangeType.Image);
    }
}
