using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SlotType
{
    Normal,
    Target,
    Teleport
}

public enum SlotState
{
    Default,
    Infected,
    Selected,
}

public class SlotObject : MonoBehaviour
{
    public List<List<SlotInfo>> ConnectedMap;
    public SlotInfo ConnectedSlotInfo;
    public Vector2 MapPivotOffset;

    public Color SelectedColor;
    public Color InfectedColor;
    public Color DefaultColor;
    
    public SlotType Type;
    public SlotState State;

    public float FadeWaitTime;
    public float FadeTime;
    public Color FadeColor;

    public float LightingWaitTime;
    public float LightingTime;
    public Color LightingColor;

    private void OnEnable()
    {
        EventManager.instance.AddHandler<LevelFinish>(OnLevelFinish);
    }

    private void OnDisable()
    {
        EventManager.instance.RemoveHandler<LevelFinish>(OnLevelFinish);
    }

    // Start is called before the first frame update
    void Start()
    {
        State = SlotState.Default;
        GetComponent<SpriteRenderer>().color = DefaultColor;
    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.State == GameState.Show)
        {
            CursorManager.InAvailableSlot = false;
            State = SlotState.Default;
            GetComponent<SpriteRenderer>().color = DefaultColor;
        }
    }

    private void OnMouseOver()
    {
        if (GameManager.State == GameState.Play && ConnectedSlotInfo.InsideBubbleType==BubbleType.Null && GameManager.HeldBubbleType!=BubbleType.Null && AvailablePos())
        {
            CursorManager.InAvailableSlot = true;
            State = SlotState.Selected;
            GetComponent<SpriteRenderer>().color = SelectedColor;
        }
    }

    private void OnMouseExit()
    {
        if (GameManager.State == GameState.Play)
        {
            CursorManager.InAvailableSlot = false;
            State = SlotState.Default;
            GetComponent<SpriteRenderer>().color = DefaultColor;
        }
    }

    private void OnMouseDown()
    {
        if (State==SlotState.Selected)
        {
            CursorManager.InAvailableSlot = false;
            State = SlotState.Default;
            GetComponent<SpriteRenderer>().color = DefaultColor;
            EventManager.instance.Fire(new Place(ConnectedSlotInfo.Pos, GameManager.HeldBubbleType));
        }
    }

    public bool AvailablePos()
    {
        Vector2Int Coordinate = new Vector2Int(Mathf.RoundToInt(transform.localPosition.x - MapPivotOffset.x), Mathf.RoundToInt(transform.localPosition.y - MapPivotOffset.y));
        return Coordinate.x < ConnectedMap.Count - 1 && ConnectedMap[Coordinate.x + 1][Coordinate.y] != null && ConnectedMap[Coordinate.x + 1][Coordinate.y].InsideBubbleType != BubbleType.Null ||
            Coordinate.x > 0 && ConnectedMap[Coordinate.x - 1][Coordinate.y] != null && ConnectedMap[Coordinate.x - 1][Coordinate.y].InsideBubbleType != BubbleType.Null ||
            Coordinate.y < ConnectedMap[Coordinate.x].Count - 1 && ConnectedMap[Coordinate.x][Coordinate.y + 1] != null && ConnectedMap[Coordinate.x][Coordinate.y + 1].InsideBubbleType != BubbleType.Null ||
            Coordinate.y > 0 && ConnectedMap[Coordinate.x][Coordinate.y - 1] != null && ConnectedMap[Coordinate.x][Coordinate.y - 1].InsideBubbleType != BubbleType.Null;
    }

    private void OnLevelFinish(LevelFinish L)
    {
        if (L.Success)
        {
            StartCoroutine(Lighting());
        }
        else
        {
            //StartCoroutine(Fade());
        }
    }

    private IEnumerator Fade()
    {
        yield return new WaitForSeconds(FadeWaitTime);

        float TimeCount = 0;
        Color CurrentColor = GetComponent<SpriteRenderer>().color;
        while (TimeCount < FadeTime)
        {
            TimeCount += Time.deltaTime;
            GetComponent<SpriteRenderer>().color = Color.Lerp(CurrentColor, FadeColor, TimeCount / FadeTime);
            yield return null;
        }
    }

    private IEnumerator Lighting()
    {
        yield return new WaitForSeconds(LightingWaitTime);

        float TimeCount = 0;
        while (TimeCount < LightingTime)
        {
            TimeCount += Time.deltaTime;
            Color CurrentColor = GetComponent<SpriteRenderer>().color;
            GetComponent<SpriteRenderer>().color = Color.Lerp(CurrentColor, LightingColor, TimeCount / LightingTime);
            yield return null;
        }
    }

}
