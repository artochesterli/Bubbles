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

    public float Size;
    public Color SelectedColor;
    public Color NearBySelectedColor;
    public Color DefaultColor;
    public Sprite SelectedSprite;
    public Sprite DefaultSprite;
    
    public SlotType Type;
    public bool Selected;
    public bool NearBySelected;

    public float FinishTime;
    public Color FinishColor;

    public float FinishRotationSpeed;
    public float ShakeSpeed;
    public float MaxShakeDis;


    private bool finish;
    private bool NormalBubbleMatch;
    private Vector3 OriPos;
    private bool TowardShakeTarget;
    private Vector3 ShakeTarget;
    private Vector3 LastShakeTarget;
    private float ShakeTimeCount;
    private float ShakeTime;


    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }


    // Start is called before the first frame update
    void Start()
    {
        OriPos = ConnectedSlotInfo.Location;
        LastShakeTarget = OriPos;
        ShakeTarget = OriPos;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        SetAppearance();
        SetShake();
    }



    private void SetAppearance()
    {
        if (GameManager.levelState !=LevelState.SetUp && GameManager.levelState!=LevelState.Clear)
        {
            if (Selected)
            {
                GetComponent<SpriteRenderer>().color = SelectedColor;
                GetComponent<SpriteRenderer>().sprite = SelectedSprite;
            }
            else if (NearBySelected)
            {
                GetComponent<SpriteRenderer>().color = SelectedColor;
                GetComponent<SpriteRenderer>().sprite = SelectedSprite;
            }
            else
            {
                GetComponent<SpriteRenderer>().color = DefaultColor;
                GetComponent<SpriteRenderer>().sprite = DefaultSprite;
            }
        }
    }

    private void SetShake()
    {
        if (GameManager.levelState != LevelState.SetUp && GameManager.levelState != LevelState.Clear)
        {
            if (Selected)
            {
                if (ShakeTimeCount >= ShakeTime)
                {
                    LastShakeTarget = ShakeTarget;
                    ShakeTarget = OriPos + (Vector3)Random.insideUnitCircle * MaxShakeDis;
                    ShakeTimeCount = 0;
                    ShakeTime = (ShakeTarget - LastShakeTarget).magnitude / ShakeSpeed;
                }

                ShakeTimeCount += Time.deltaTime;
                transform.position = Vector3.Lerp(LastShakeTarget, ShakeTarget, ShakeTimeCount / ShakeTime);
            }
            else
            {
                transform.position = OriPos;
                ShakeTime = 0;
                ShakeTimeCount = 0;
                LastShakeTarget = OriPos;
                ShakeTarget = OriPos;
            }
        }
    }

    public bool CursorInside()
    {
        Vector3 CursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition+(Vector3)CursorManager.Entity.GetComponent<CursorManager>().Offset* CursorManager.Entity.transform.localScale.y);
        return CursorPos.x >= OriPos.x - Size / 2 && CursorPos.x <= OriPos.x + Size / 2 && CursorPos.y >= OriPos.y - Size / 2 && CursorPos.y <= OriPos.y + Size / 2;
    }

    public bool Inside(Vector3 CursorPos)
    {
        return CursorPos.x >= OriPos.x - Size / 2 && CursorPos.x <= OriPos.x + Size / 2 && CursorPos.y >= OriPos.y - Size / 2 && CursorPos.y <= OriPos.y + Size / 2;
    }

    public bool AvailablePos(List<GameObject> NearByCircleList = null)
    {
        Vector2Int Coordinate = ConnectedSlotInfo.Pos;
        bool HaveNearByCircle = false;
        if(Coordinate.x < ConnectedMap.Count - 1 && ConnectedMap[Coordinate.x + 1][Coordinate.y] != null && ConnectedMap[Coordinate.x + 1][Coordinate.y].InsideBubbleType != BubbleType.Null)
        {
            if (NearByCircleList != null)
            {
                NearByCircleList[0] = ConnectedMap[Coordinate.x + 1][Coordinate.y].ConnectedBubble;
            }
            HaveNearByCircle = true;
        }

        if(Coordinate.x > 0 && ConnectedMap[Coordinate.x - 1][Coordinate.y] != null && ConnectedMap[Coordinate.x - 1][Coordinate.y].InsideBubbleType != BubbleType.Null)
        {
            if (NearByCircleList != null)
            {
                NearByCircleList[1] = ConnectedMap[Coordinate.x - 1][Coordinate.y].ConnectedBubble;
            }
            HaveNearByCircle = true;
        }

        if(Coordinate.y < ConnectedMap[Coordinate.x].Count - 1 && ConnectedMap[Coordinate.x][Coordinate.y + 1] != null && ConnectedMap[Coordinate.x][Coordinate.y + 1].InsideBubbleType != BubbleType.Null)
        {
            if (NearByCircleList != null)
            {
                NearByCircleList[2] = ConnectedMap[Coordinate.x][Coordinate.y + 1].ConnectedBubble;
            }
            HaveNearByCircle = true;
        }

        if(Coordinate.y > 0 && ConnectedMap[Coordinate.x][Coordinate.y - 1] != null && ConnectedMap[Coordinate.x][Coordinate.y - 1].InsideBubbleType != BubbleType.Null)
        {
            if (NearByCircleList != null)
            {
                NearByCircleList[3] = ConnectedMap[Coordinate.x][Coordinate.y - 1].ConnectedBubble;
            }
            HaveNearByCircle = true;
        }

        return ConnectedSlotInfo.InsideBubbleType == BubbleType.Null && GameManager.HeldBubbleType != BubbleType.Null && HaveNearByCircle;
    }

    public ColorChangeTask GetFadeTask()
    {
        return new ColorChangeTask(gameObject, DefaultColor, FinishColor, FinishTime, ColorChangeType.Sprite);
    }

}
