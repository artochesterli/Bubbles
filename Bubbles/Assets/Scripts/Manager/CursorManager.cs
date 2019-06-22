using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Color DisappearBubbleColor;
    public Color NormalBubbleColor;
    public Color NoBubbleColor;

    public float ChangeTime;
    public float Scale;
    public float InflatedScale;

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
                            GetComponent<SpriteRenderer>().color = DisappearBubbleColor;
                            CurrentColor = DisappearBubbleColor;
                            break;
                        case BubbleType.Normal:
                            GetComponent<SpriteRenderer>().color = NormalBubbleColor;
                            CurrentColor = NormalBubbleColor;
                            break;
                        default:
                            break;
                    }
                    break;
                case GameState.Show:
                    transform.localScale = Vector3.one * Scale;
                    GetComponent<SpriteRenderer>().color = NoBubbleColor;
                    break;
                default:
                    break;
            }
        }
    }

    private void SetPos()
    {
        transform.position = Vector3.back * Camera.main.transform.position.z + Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private IEnumerator ChangeColor(Color Start, Color End)
    {
        ColorChanging = true;
        transform.localScale = Scale * Vector3.one;
        float TimeCount = 0;
        while (TimeCount < ChangeTime / 2)
        {
            TimeCount += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Scale*Vector3.one, InflatedScale*Vector3.one, 2 * TimeCount / ChangeTime);
            GetComponent<SpriteRenderer>().color = Color.Lerp(Start, End, TimeCount / ChangeTime);
            yield return null;
        }

        while(TimeCount < ChangeTime)
        {
            TimeCount += Time.deltaTime;
            transform.localScale = Vector3.Lerp(InflatedScale * Vector3.one, Scale * Vector3.one, (2 * TimeCount - ChangeTime) / ChangeTime);
            GetComponent<SpriteRenderer>().color = Color.Lerp(Start, End, TimeCount / ChangeTime);
            yield return null;
        }
        CurrentColor = End;
        ColorChanging = false;
    }

    private void OnBubbleSelected(BubbleSelected B)
    {
        StopAllCoroutines();
        switch (B.Type)
        {
            case BubbleType.Disappear:
                StartCoroutine(ChangeColor(CurrentColor, DisappearBubbleColor));
                break;
            case BubbleType.Normal:
                StartCoroutine(ChangeColor(CurrentColor, NormalBubbleColor));
                break;
        }
    }
}
