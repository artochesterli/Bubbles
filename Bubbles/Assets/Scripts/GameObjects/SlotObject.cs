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
    public Color DefaultColor;
    
    public SlotType Type;
    public bool Selected;

    public float FinishWaitTime;
    public float FinishTime;
    public Color FinishColor;

    public float FinishRotationSpeed;


    private bool finish;

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
        GetComponent<SpriteRenderer>().color = DefaultColor;
    }

    // Update is called once per frame
    void Update()
    {
        /*if(GameManager.State == GameState.Show)
        {
            CursorManager.InAvailableSlot = false;
            State = SlotState.Default;
            GetComponent<SpriteRenderer>().color = DefaultColor;
        }*/
        if (finish)
        {
            transform.Rotate(Vector3.forward, FinishRotationSpeed * Time.deltaTime);
        }

    }

    

    private void OnMouseOver()
    {
        if (GameManager.State == GameState.Play && ConnectedSlotInfo.InsideBubbleType==BubbleType.Null && GameManager.HeldBubbleType!=BubbleType.Null && AvailablePos())
        {
            CursorManager.InAvailableSlot = true;
            Selected = true;
            GetComponent<SpriteRenderer>().color = SelectedColor;
        }
    }

    private void OnMouseExit()
    {
        if (GameManager.State == GameState.Play)
        {
            CursorManager.InAvailableSlot = false;
            Selected = false;
            GetComponent<SpriteRenderer>().color = DefaultColor;
        }
    }

    private void OnMouseDown()
    {
        if (Selected)
        {
            CursorManager.InAvailableSlot = false;
            Selected = false;
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
        StartCoroutine(FinishEffect());
    }

    private IEnumerator FinishEffect()
    {
        yield return new WaitForSeconds(FinishWaitTime);

        finish = true;

        float TimeCount = 0;
        while (TimeCount < FinishTime)
        {
            TimeCount += Time.deltaTime;
            Color CurrentColor = GetComponent<SpriteRenderer>().color;
            GetComponent<SpriteRenderer>().color = Color.Lerp(DefaultColor, FinishColor, TimeCount / FinishTime);
            yield return null;
        }
    }

}
