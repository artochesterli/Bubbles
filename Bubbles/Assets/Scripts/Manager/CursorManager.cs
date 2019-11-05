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
    public static GameObject Entity;
    public static GameObject AllSlot;
    public static CursorState CurrentState;
    public GameObject ActivateEffect;
    public GameObject CursorImage;
    public Vector2 Offset;

    public float RollBackInputInterval;

    public Color NullColor;
    public Color DisappearBubbleColor;
    public Color NormalBubbleColor;
    public Color ExpandBubbleColor;

    public float ColorChangeTime;

    public float OutSlotScale;
    public float InSlotScale;
    public float InSlotScaleChangeTime;

    private List<GameObject> OffsetCircles;
    private List<GameObject> NearBySelectedSlots;
    private GameObject SelectedSlot;

    private bool ColorChanging;
    private Color CurrentColor;


    private float RollBackInputIntervalTimeCount;
    private bool RollBackFirstTap;
    // Start is called before the first frame update
    void Start()
    {
        Entity = gameObject;
        ActivateEffect.transform.localPosition = Offset;
        CursorImage.GetComponent<RectTransform>().localPosition = Offset;

        DeactivateCursor();

        OffsetCircles = new List<GameObject>();
        NearBySelectedSlots = new List<GameObject>();
        EventManager.instance.AddHandler<BubbleSelected>(OnBubbleSelected);
        //EventManager.instance.AddHandler<Place>(OnPlace);
        EventManager.instance.AddHandler<RollBack>(OnRollBack);
    }

    private void OnDestroy()
    {
        AllSlot = null;
        EventManager.instance.RemoveHandler<BubbleSelected>(OnBubbleSelected);
        EventManager.instance.RemoveHandler<Place>(OnPlace);
        EventManager.instance.RemoveHandler<RollBack>(OnRollBack);
    }

    // Update is called once per frame
    void Update()
    {
        SetPos();
        //CheckSlotSelection();
        //SetScale();

        GetRollBackInput();
        //CheckInput();
    }

    private GameObject GetNearBySlot(GameObject Slot, Vector2Int Offset)
    {
        List<List<SlotInfo>> Map = Slot.GetComponent<SlotObject>().ConnectedMap;
        Vector2Int Coordination = Slot.GetComponent<SlotObject>().ConnectedSlotInfo.Pos + Offset;
        return Map[Coordination.x][Coordination.y].Entity;
    }

    private void GetRollBackInput()
    {
        if(GameManager.levelState == LevelState.Play && CurrentState == CursorState.Release)
        {
            if (RollBackFirstTap)
            {
                RollBackInputIntervalTimeCount += Time.deltaTime;
                if (RollBackInputIntervalTimeCount >= RollBackInputInterval)
                {
                    RollBackFirstTap = false;
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (!RollBackFirstTap)
                {
                    RollBackInputIntervalTimeCount = 0;
                    RollBackFirstTap = true;
                }
                else
                {
                    EventManager.instance.Fire(new RollBack());
                }
            }
        }
        else
        {
            RollBackFirstTap = false;
            RollBackInputIntervalTimeCount = 0;
        }
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
                                    Handheld.Vibrate();
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

                                        GameObject Slot = null;

                                        switch (i)
                                        {
                                            case 0:
                                                Slot = GetNearBySlot(child.gameObject, Vector2Int.right);
                                                Slot.GetComponent<SlotObject>().NearBySelected = true;
                                                break;
                                            case 1:
                                                Slot = GetNearBySlot(child.gameObject, Vector2Int.left);
                                                Slot.GetComponent<SlotObject>().NearBySelected = true;
                                                break;
                                            case 2:
                                                Slot = GetNearBySlot(child.gameObject, Vector2Int.up);
                                                Slot.GetComponent<SlotObject>().NearBySelected = true;
                                                break;
                                            case 3:
                                                Slot = GetNearBySlot(child.gameObject, Vector2Int.down);
                                                Slot.GetComponent<SlotObject>().NearBySelected = true;
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

                    SelectedSlot.GetComponent<SlotObject>().Selected = false;
                    //EventManager.instance.Fire(new Place(SelectedSlot.GetComponent<SlotObject>().ConnectedSlotInfo.Pos, GameManager.HeldBubbleType));
                    GameManager.HeldBubbleType = BubbleType.Null;
                }
                else if(GameManager.HeldBubbleType!=BubbleType.Null)
                {
                    CancelBubble();
                }
            }
        }
    }

    private void SetScale()
    {
        if (GameManager.gameState == GameState.Level && GameManager.levelState == LevelState.Play && CurrentState == CursorState.Holding)
        {
            if (SelectedSlot)
            {

            }
            else
            {

            }
            transform.localScale = Vector3.one*OutSlotScale;
            ActivateEffect.transform.localScale = transform.localScale;
        }
    }

    private void SetPos()
    {

        GetComponent<RectTransform>().position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        GetComponent<RectTransform>().position -= GetComponent<RectTransform>().position.z * Vector3.forward;
    }

    private void OnBubbleSelected(BubbleSelected B)
    {
        ActivateCursor(B.Type);
    }

    private void ActivateCursor(BubbleType Type)
    {
        Cursor.visible = false;
        CurrentState = CursorState.Holding;
        CursorImage.GetComponent<Image>().enabled = true;
        switch (Type)
        {
            case BubbleType.Disappear:
                CursorImage.GetComponent<Image>().color = DisappearBubbleColor;
                LevelManager.RemainedDisappearBubble--;
                EventManager.instance.Fire(new BubbleNumSet(Type, LevelManager.RemainedDisappearBubble));
                break;
            case BubbleType.Normal:
                CursorImage.GetComponent<Image>().color = NormalBubbleColor;
                LevelManager.RemainedNormalBubble--;
                EventManager.instance.Fire(new BubbleNumSet(Type, LevelManager.RemainedNormalBubble));
                break;
        }

        ActivateEffect.GetComponent<ParticleSystem>().Play();

        GameManager.HeldBubbleType = Type;

    }

    private void DeactivateCursor()
    {
        Cursor.visible = true;
        CurrentState = CursorState.Release;
        CursorImage.GetComponent<Image>().enabled = false;
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
        }

        GameManager.HeldBubbleType = BubbleType.Null;
    }

    private void OnPlace(Place P)
    {
        ResetOffsetInfo();
        SelectedSlot.GetComponent<SlotObject>().Selected = false;
        SelectedSlot = null;
    }

    private void OnRollBack(RollBack R)
    {
        DeactivateCursor();
        CancelBubble();
    }

}
