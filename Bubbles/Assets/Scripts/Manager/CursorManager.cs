using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CursorType
{
    InMenu,
    InLevel
}

public enum CursorState
{
    Release,
    Holding
}

public class CursorManager : MonoBehaviour
{
    public static GameObject AllSlot;
    public GameObject ActivateEffect;

    public float PressingTime;

    public Color NullColor;
    public Color DisappearBubbleColor;
    public Color NormalBubbleColor;
    public Color ExpandBubbleColor;

    public float ColorChangeTime;
    public float Scale;
    public float InflatedScale;

    public float InSlotScale;
    public float InSlotScaleChangeTime;

    private List<GameObject> OffsetCircles;
    private List<GameObject> NearBySelectedSlots;
    private GameObject SelectedSlot;

    private bool ColorChanging;
    private Color CurrentColor;
    public CursorState CurrentState;
    // Start is called before the first frame update
    void Start()
    {
        DeactivateCursor();

        OffsetCircles = new List<GameObject>();
        NearBySelectedSlots = new List<GameObject>();
        EventManager.instance.AddHandler<BubbleSelected>(OnBubbleSelected);
        EventManager.instance.AddHandler<Place>(OnPlace);
    }

    private void OnDestroy()
    {
        AllSlot = null;
        EventManager.instance.RemoveHandler<BubbleSelected>(OnBubbleSelected);
        EventManager.instance.RemoveHandler<Place>(OnPlace);
    }

    // Update is called once per frame
    void Update()
    {
        SetPos();
        CheckSlotSelection();
        SetScale();

        CheckInput();
    }

    private GameObject GetNearBySlot(GameObject Slot, Vector2Int Offset)
    {
        List<List<SlotInfo>> Map = Slot.GetComponent<SlotObject>().ConnectedMap;
        Vector2Int Coordination = Slot.GetComponent<SlotObject>().ConnectedSlotInfo.Pos + Offset;
        return Map[Coordination.x][Coordination.y].Entity;
    }

    private void CheckSlotSelection()
    {
        if (GameManager.gameState == GameState.Level)
        {
            if (GameManager.levelState == LevelState.Play)
            {
                if (CurrentState == CursorState.Holding)
                {
                    foreach (Transform child in AllSlot.transform)
                    {
                        List<GameObject> NearByCircleList = new List<GameObject>();
                        for (int i = 0; i < 4; i++)
                        {
                            NearByCircleList.Add(null);
                        }
                        if (child.GetComponent<SlotObject>().CursorInside() && child.GetComponent<SlotObject>().AvailablePos(NearByCircleList))
                        {
                            if (child.gameObject != SelectedSlot)
                            {
                                if (SelectedSlot != null)
                                {
                                    SelectedSlot.GetComponent<SlotObject>().Selected = false;
                                    ResetOffsetInfo();
                                }
                                child.GetComponent<SlotObject>().Selected = true;
                                SelectedSlot = child.gameObject;
                                for (int i = 0; i < NearByCircleList.Count; i++)
                                {
                                    if (NearByCircleList[i] != null)
                                    {

                                        OffsetCircles.Add(NearByCircleList[i]);
                                        NearByCircleList[i].GetComponent<Bubble>().Offseting = true;

                                        GameObject Slot = null;

                                        switch (i)
                                        {
                                            case 0:
                                                Slot = GetNearBySlot(child.gameObject, Vector2Int.right);
                                                Slot.GetComponent<SlotObject>().NearBySelected = true;
                                                NearByCircleList[i].GetComponent<Bubble>().OffsetDirection = Vector2.right;
                                                break;
                                            case 1:
                                                Slot = GetNearBySlot(child.gameObject, Vector2Int.left);
                                                Slot.GetComponent<SlotObject>().NearBySelected = true;
                                                NearByCircleList[i].GetComponent<Bubble>().OffsetDirection = Vector2.left;
                                                break;
                                            case 2:
                                                Slot = GetNearBySlot(child.gameObject, Vector2Int.up);
                                                Slot.GetComponent<SlotObject>().NearBySelected = true;
                                                NearByCircleList[i].GetComponent<Bubble>().OffsetDirection = Vector2.up;
                                                break;
                                            case 3:
                                                Slot = GetNearBySlot(child.gameObject, Vector2Int.down);
                                                Slot.GetComponent<SlotObject>().NearBySelected = true;
                                                NearByCircleList[i].GetComponent<Bubble>().OffsetDirection = Vector2.down;
                                                break;
                                            default:
                                                break;

                                        }

                                        NearBySelectedSlots.Add(Slot);
                                    }
                                }
                            }
                            return;
                        }
                    }
                    if (SelectedSlot != null)
                    {
                        SelectedSlot.GetComponent<SlotObject>().Selected = false;
                        SelectedSlot = null;
                        ResetOffsetInfo();
                    }
                }
                else
                {
                    if (SelectedSlot != null)
                    {
                        SelectedSlot.GetComponent<SlotObject>().Selected = false;
                        SelectedSlot = null;
                        ResetOffsetInfo();
                    }
                }
            }
        }

    }

    private void ResetOffsetInfo()
    {
        for (int i = 0; i < OffsetCircles.Count; i++)
        {
            OffsetCircles[i].GetComponent<Bubble>().Offseting = false;
        }

        for(int i = 0; i < NearBySelectedSlots.Count; i++)
        {
            NearBySelectedSlots[i].GetComponent<SlotObject>().NearBySelected = false;
        }

        OffsetCircles.Clear();
        NearBySelectedSlots.Clear();
    }

    private void CheckInput()
    {
        if (GameManager.gameState == GameState.Level)
        {
            if (Input.GetMouseButtonUp(0))
            {
                DeactivateCursor();
                if (SelectedSlot != null)
                {
                    if (GameManager.HeldBubbleType == BubbleType.Expand)
                    {
                        GameManager.ActivatedLevel.GetComponent<LevelManager>().ExpandToNormal();
                    }

                    SelectedSlot.GetComponent<SlotObject>().Selected = false;
                    EventManager.instance.Fire(new Place(SelectedSlot.GetComponent<SlotObject>().ConnectedSlotInfo.Pos, GameManager.HeldBubbleType));
                    GameManager.HeldBubbleType = BubbleType.Null;
                }
                else if(GameManager.HeldBubbleType!=BubbleType.Null)
                {
                    if (GameManager.HeldBubbleType == BubbleType.Expand)
                    {
                        GameManager.ActivatedLevel.GetComponent<LevelManager>().CutMap();
                    }
                    CancelBubble();
                }
            }
        }
    }

    private void SetScale()
    {
        if (GameManager.gameState == GameState.Level)
        {

            transform.localScale = Vector3.one*InSlotScale;
            ActivateEffect.transform.localScale = transform.localScale;
        }
    }

    private void SetPos()
    {

        GetComponent<RectTransform>().position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        GetComponent<RectTransform>().position -= GetComponent<RectTransform>().position.z * Vector3.forward;
    }

    private IEnumerator ChangeColor(Color Start, Color End)
    {
        ColorChanging = true;
        transform.localScale = Scale * Vector3.one;
        float TimeCount = 0;
        while (TimeCount < ColorChangeTime / 2)
        {
            TimeCount += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Scale*Vector3.one, InflatedScale*Vector3.one, 2 * TimeCount / ColorChangeTime);
            GetComponent<Image>().color = Color.Lerp(Start, End, TimeCount / ColorChangeTime);
            yield return null;
        }

        while(TimeCount < ColorChangeTime)
        {
            TimeCount += Time.deltaTime;
            transform.localScale = Vector3.Lerp(InflatedScale * Vector3.one, Scale * Vector3.one, (2 * TimeCount - ColorChangeTime) / ColorChangeTime);
            GetComponent<Image>().color = Color.Lerp(Start, End, TimeCount / ColorChangeTime);
            yield return null;
        }
        CurrentColor = End;
        ColorChanging = false;
    }

    private void OnBubbleSelected(BubbleSelected B)
    {
        ActivateCursor(B.Type);
        if (B.Type == BubbleType.Expand)
        {
            GameManager.ActivatedLevel.GetComponent<LevelManager>().ExpandMap();
        }
    }

    private void ActivateCursor(BubbleType Type)
    {
        Cursor.visible = false;
        CurrentState = CursorState.Holding;
        GetComponent<Image>().enabled = true;
        switch (Type)
        {
            case BubbleType.Disappear:
                GetComponent<Image>().color = DisappearBubbleColor;
                LevelManager.RemainedDisappearBubble--;
                EventManager.instance.Fire(new BubbleNumSet(Type, LevelManager.RemainedDisappearBubble));
                break;
            case BubbleType.Normal:
                GetComponent<Image>().color = NormalBubbleColor;
                LevelManager.RemainedNormalBubble--;
                EventManager.instance.Fire(new BubbleNumSet(Type, LevelManager.RemainedNormalBubble));
                break;
            case BubbleType.Expand:
                GetComponent<Image>().color = ExpandBubbleColor;
                LevelManager.RemainedExpandBubble--;
                EventManager.instance.Fire(new BubbleNumSet(Type, LevelManager.RemainedExpandBubble));
                break;
        }

        ActivateEffect.GetComponent<ParticleSystem>().Play();

        GameManager.HeldBubbleType = Type;

    }

    private void DeactivateCursor()
    {
        Cursor.visible = true;
        CurrentState = CursorState.Release;
        GetComponent<Image>().enabled = false;
        ActivateEffect.GetComponent<ParticleSystem>().Stop();
    }

    private void CancelBubble()
    {
        switch (GameManager.HeldBubbleType)
        {
            case BubbleType.Disappear:
                LevelManager.RemainedDisappearBubble++;
                EventManager.instance.Fire(new BubbleNumSet(GameManager.HeldBubbleType, LevelManager.RemainedDisappearBubble));
                break;
            case BubbleType.Normal:
                LevelManager.RemainedNormalBubble++;
                EventManager.instance.Fire(new BubbleNumSet(GameManager.HeldBubbleType, LevelManager.RemainedNormalBubble));
                break;
            case BubbleType.Expand:
                LevelManager.RemainedExpandBubble++;
                EventManager.instance.Fire(new BubbleNumSet(GameManager.HeldBubbleType, LevelManager.RemainedExpandBubble));
                break;
        }

        GameManager.HeldBubbleType = BubbleType.Null;
    }

    private void OnPlace(Place P)
    {
        ResetOffsetInfo();
        SelectedSlot.GetComponent<SlotObject>().Selected = false;
        SelectedSlot = null;
    }

}
