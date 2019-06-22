using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SlotType
{
    Normal,
    Target
}

public class SlotObject : MonoBehaviour
{
    public List<List<SlotInfo>> ConnectedMap;
    public SlotInfo ConnectedSlotInfo;
    public Vector2 MapPivotOffset;
    
    public float SelectedAlpha;
    public float DefaultAlpha;
    public SlotType Type;

    private bool Selected;
    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnDestroy()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseOver()
    {
        if (GameManager.State == GameState.Play && ConnectedSlotInfo.InsideBubbleType==BubbleType.Null && GameManager.HeldBubbleType!=BubbleType.Null && AvailablePos())
        {
            Selected = true;
            Color color = GetComponent<SpriteRenderer>().color;
            GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, SelectedAlpha);
        }
    }

    private void OnMouseExit()
    {
        Selected = false;
        Color color = GetComponent<SpriteRenderer>().color;
        GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, DefaultAlpha);
    }

    private void OnMouseDown()
    {
        if (Selected)
        {
            EventManager.instance.Fire(new Place(ConnectedSlotInfo.Pos, GameManager.HeldBubbleType));
            Selected = false;
            Color color = GetComponent<SpriteRenderer>().color;
            GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, DefaultAlpha);
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
}
