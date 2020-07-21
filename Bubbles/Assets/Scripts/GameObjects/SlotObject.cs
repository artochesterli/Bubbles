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

public class NearByInfo
{
    public GameObject RightBubble;
    public GameObject LeftBubble;
    public GameObject TopBubble;
    public GameObject DownBubble;

    public NearByInfo(GameObject right, GameObject left, GameObject top, GameObject down)
    {
        RightBubble = right;
        LeftBubble = left;
        TopBubble = top;
        DownBubble = down;
    }

    public NearByInfo()
    {
        RightBubble = LeftBubble = TopBubble = DownBubble = null;
    }

    public bool Available()
    {
        return RightBubble || LeftBubble || TopBubble || DownBubble;
    }
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

        if (GameManager.levelState != LevelState.SetUp && GameManager.levelState != LevelState.Clear)
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

    public bool Inside(Vector3 CursorPos)
    {
        return CursorPos.x >= OriPos.x - Size / 2 && CursorPos.x <= OriPos.x + Size / 2 && CursorPos.y >= OriPos.y - Size / 2 && CursorPos.y <= OriPos.y + Size / 2;
    }

    public NearByInfo GetNearByInfo()
    {
        NearByInfo Info = new NearByInfo();

        Vector2Int Coordinate = ConnectedSlotInfo.Pos;

        if(Coordinate.x < ConnectedMap.Count - 1 && ConnectedMap[Coordinate.x + 1][Coordinate.y] != null && ConnectedMap[Coordinate.x + 1][Coordinate.y].InsideBubbleType != BubbleType.Null)
        {
            Info.RightBubble = ConnectedMap[Coordinate.x + 1][Coordinate.y].ConnectedBubble;
        }

        if(Coordinate.x > 0 && ConnectedMap[Coordinate.x - 1][Coordinate.y] != null && ConnectedMap[Coordinate.x - 1][Coordinate.y].InsideBubbleType != BubbleType.Null)
        {
            Info.LeftBubble = ConnectedMap[Coordinate.x - 1][Coordinate.y].ConnectedBubble;
        }

        if(Coordinate.y < ConnectedMap[Coordinate.x].Count - 1 && ConnectedMap[Coordinate.x][Coordinate.y + 1] != null && ConnectedMap[Coordinate.x][Coordinate.y + 1].InsideBubbleType != BubbleType.Null)
        {
            Info.TopBubble = ConnectedMap[Coordinate.x][Coordinate.y + 1].ConnectedBubble;
        }

        if(Coordinate.y > 0 && ConnectedMap[Coordinate.x][Coordinate.y - 1] != null && ConnectedMap[Coordinate.x][Coordinate.y - 1].InsideBubbleType != BubbleType.Null)
        {
            Info.DownBubble = ConnectedMap[Coordinate.x][Coordinate.y - 1].ConnectedBubble;
        }

        return Info;
    }

    public ColorChangeTask GetFadeTask()
    {
        return new ColorChangeTask(gameObject, DefaultColor, FinishColor, FinishTime, ColorChangeType.Sprite);
    }

}
