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
    public Color DefaultColor;
    public Sprite SelectedSprite;
    public Sprite DefaultSprite;
    
    public SlotType Type;
    public bool Selected;

    public float FinishWaitTime;
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
        EventManager.instance.AddHandler<LevelFinish>(OnLevelFinish);
    }

    private void OnDisable()
    {
        EventManager.instance.RemoveHandler<LevelFinish>(OnLevelFinish);
    }

    // Start is called before the first frame update
    void Start()
    {
        OriPos = transform.position;
        LastShakeTarget = OriPos;
        ShakeTarget = OriPos;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (finish)
        {
            transform.Rotate(Vector3.forward, FinishRotationSpeed * Time.deltaTime);
        }

        SetAppearance();
        SetShake();
        
    }

    private void SetAppearance()
    {
        if (GameManager.State !=GameState.SetUp && GameManager.State!=GameState.Clear)
        {
            if (Selected)
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

    public bool CursorInside()
    {
        
        Vector3 MousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return MousePos.x >= OriPos.x - Size / 2 && MousePos.x <= OriPos.x + Size / 2 && MousePos.y >= OriPos.y - Size / 2 && MousePos.y <= OriPos.y + Size / 2;
    }

    public bool AvailablePos()
    {
        Vector2Int Coordinate = ConnectedSlotInfo.Pos;
        return ConnectedSlotInfo.InsideBubbleType == BubbleType.Null && GameManager.HeldBubbleType != BubbleType.Null &&
            (Coordinate.x < ConnectedMap.Count - 1 && ConnectedMap[Coordinate.x + 1][Coordinate.y] != null && ConnectedMap[Coordinate.x + 1][Coordinate.y].InsideBubbleType != BubbleType.Null ||
            Coordinate.x > 0 && ConnectedMap[Coordinate.x - 1][Coordinate.y] != null && ConnectedMap[Coordinate.x - 1][Coordinate.y].InsideBubbleType != BubbleType.Null ||
            Coordinate.y < ConnectedMap[Coordinate.x].Count - 1 && ConnectedMap[Coordinate.x][Coordinate.y + 1] != null && ConnectedMap[Coordinate.x][Coordinate.y + 1].InsideBubbleType != BubbleType.Null ||
            Coordinate.y > 0 && ConnectedMap[Coordinate.x][Coordinate.y - 1] != null && ConnectedMap[Coordinate.x][Coordinate.y - 1].InsideBubbleType != BubbleType.Null);
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
