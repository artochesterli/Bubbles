using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UsableCircle : MonoBehaviour
{
    public static GameObject AllSlot;

    public BubbleType Type;
    public float DefaultSize;
    public float SelectedSize;
    public float SelectedOffset;
    public GameObject ActivatedEffect;
    public GameObject ReleaseEffect;
    public GameObject TempActivatedEffectPrefab;

    public float InflationTime;
    public float ColorRecoverTime;

    public float DisappearTime;

    private Vector2 OriPos;
    private bool Selected;
    private float CurrentSize;
    private float CurrentOffset;
    private Color color;
    private float ColorRecoverTimeCount;

    private List<GameObject> NearBySelectedSlots;
    private GameObject SelectedSlot;

    private NearByInfo CurrentNearByInfo;

    // Start is called before the first frame update
    void Start()
    {
        color = GetComponent<SpriteRenderer>().color;
        color = new Color(color.r, color.g, color.b, 0);
        ColorRecoverTimeCount = ColorRecoverTime;

        OriPos = transform.position;
        Selected = false;
        CurrentSize = DefaultSize;
        NearBySelectedSlots = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.levelState == LevelState.Play || GameManager.levelState == LevelState.Executing || GameManager.levelState == LevelState.SetUp)
        {
            CheckSelected();
            SetTransform();
        }

        if (GameManager.levelState == LevelState.Play || GameManager.levelState == LevelState.Executing)
        {
            SetColor();
        }

        if (GameManager.levelState == LevelState.Play)
        {
            CheckSlotSelection();
        }

    }

    private void SetTransform()
    {
        float SizeChangeSpeed = (SelectedSize - DefaultSize) / InflationTime;
        float OffsetChangeSpeed = SelectedOffset / InflationTime;
        if (Selected)
        {
            CurrentOffset += OffsetChangeSpeed * Time.deltaTime;
            CurrentSize += SizeChangeSpeed * Time.deltaTime;
            if(CurrentSize > SelectedSize)
            {
                CurrentSize = SelectedSize;
            }
            if(CurrentOffset > SelectedOffset)
            {
                CurrentOffset = SelectedOffset;
            }

            Vector3 WorldPos;

            if(SystemInfo.deviceType == DeviceType.Handheld)
            {
                WorldPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
            }
            else
            {
                WorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }

            WorldPos.z = 0;
            transform.position = WorldPos + Vector3.up * CurrentOffset * CurrentSize;

        }

        transform.localScale = Vector3.one * CurrentSize;
    }

    private void SetColor()
    {
        ColorRecoverTimeCount += Time.deltaTime;
        GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, Mathf.Lerp(0, 1, ColorRecoverTimeCount / ColorRecoverTime));
    }

    

    private void CheckSelected()
    {

        bool PressDown = Input.touchCount > 0 && SystemInfo.deviceType == DeviceType.Handheld || Input.GetMouseButtonDown(0) && SystemInfo.deviceType == DeviceType.Desktop;
        bool Release = Input.touchCount == 0 && SystemInfo.deviceType == DeviceType.Handheld || Input.GetMouseButtonUp(0)&& SystemInfo.deviceType == DeviceType.Desktop;

        if (!Selected && GameManager.cursorState == CursorState.Release && PressDown && CursorInside())
        {
            GameManager.HeldBubbleType = Type;
            GameManager.cursorState = CursorState.Holding;

            Selected = true;
            ActivatedEffect.GetComponent<ParticleSystem>().Play();
        }

        if (Selected && Release)
        {
            GameManager.cursorState = CursorState.Release;

            Selected = false;

            if (SelectedSlot != null)
            {
                ResetNearByBubble(CurrentNearByInfo);
                CurrentNearByInfo = null;

                ResetOffsetInfo();
                SelectedSlot.GetComponent<SlotObject>().Selected = false;
                EventManager.instance.Fire(new Place(gameObject, SelectedSlot.GetComponent<SlotObject>().ConnectedSlotInfo.Pos, GameManager.HeldBubbleType));
                transform.position = OriPos;
                transform.localScale = Vector3.one * DefaultSize;
                CurrentOffset = 0;
                CurrentSize = DefaultSize;
                ColorRecoverTimeCount = ColorRecoverTime;
                GetComponent<SpriteRenderer>().color = Utility.ColorWithAlpha(GetComponent<SpriteRenderer>().color, 0);

                GameManager.HeldBubbleType = BubbleType.Null;

                ActivatedEffect.GetComponent<ParticleSystem>().Stop();

                gameObject.SetActive(false);
            }
            else
            {
                GameObject Temp = GameObject.Instantiate(TempActivatedEffectPrefab, transform.position, Quaternion.Euler(0, 0, 0));
                Temp.GetComponent<ParticleSystem>().Stop();

                transform.position = OriPos;
                transform.localScale = Vector3.one * DefaultSize;
                CurrentSize = DefaultSize;
                CurrentOffset = 0;
                GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, 0);
                ColorRecoverTimeCount = 0;

                ActivatedEffect.GetComponent<ParticleSystem>().Stop();
                ActivatedEffect.GetComponent<ParticleSystem>().Clear();


            }
        }
    }


    private bool CursorInside()
    {
        Vector3 WorldPos;

        if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            WorldPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
        }
        else
        {
            WorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        return WorldPos.x > OriPos.x - DefaultSize / 2 && WorldPos.x < OriPos.x + DefaultSize / 2
            && WorldPos.y > OriPos.y - DefaultSize / 2 && WorldPos.y < OriPos.y + DefaultSize / 2;

    }

    public void SetColor(Color c)
    {
        color = c;
    }

    private void ResetNearByBubble(NearByInfo PreviousNearByInfo)
    {
        if (PreviousNearByInfo != null) //Reset previous near by bubbles
        {
            if (PreviousNearByInfo.RightBubble != null)
            {
                PreviousNearByInfo.RightBubble.GetComponent<NormalBubble>().IntendMoveDir = Direction.Null;
            }

            if (PreviousNearByInfo.LeftBubble != null)
            {
                PreviousNearByInfo.LeftBubble.GetComponent<NormalBubble>().IntendMoveDir = Direction.Null;
            }

            if (PreviousNearByInfo.TopBubble != null)
            {
                PreviousNearByInfo.TopBubble.GetComponent<NormalBubble>().IntendMoveDir = Direction.Null;
            }

            if (PreviousNearByInfo.DownBubble != null)
            {
                PreviousNearByInfo.DownBubble.GetComponent<NormalBubble>().IntendMoveDir = Direction.Null;
            }
        }
    }

    private void CheckSlotSelection()
    {
        if (GameManager.gameState == GameState.Level)
        {
            if (Selected)//Hold
            {
                foreach (Transform child in AllSlot.transform)
                {
                    if (child.GetComponent<SlotObject>().Inside(transform.position))
                    {
                        NearByInfo PreviousNearByInfo = CurrentNearByInfo;

                        CurrentNearByInfo = child.GetComponent<SlotObject>().GetNearByInfo();
                        if (CurrentNearByInfo.Available())
                        {
                            if (child.gameObject != SelectedSlot) //Select a new available slot
                            {
                                if (GameManager.CurrentConfig.Vibration)
                                {
                                    Taptic.Light();
                                }

                                ResetNearByBubble(PreviousNearByInfo);

                                if (SelectedSlot != null)
                                {
                                    SelectedSlot.GetComponent<SlotObject>().Selected = false;
                                    ResetOffsetInfo();
                                }
                                child.GetComponent<SlotObject>().Selected = true;
                                SelectedSlot = child.gameObject;


                                if (SelectedSlot.GetComponent<SlotObject>().Type != SlotType.Teleport || !GameManager.ActivatedLevel.GetComponent<LevelManager>().TeleportAvailable(SelectedSlot.GetComponent<SlotObject>().ConnectedSlotInfo))
                                {
                                    ActivatedEffect.GetComponent<ParticleSystem>().Stop();
                                    if (CurrentNearByInfo.RightBubble)
                                    {
                                        CurrentNearByInfo.RightBubble.GetComponent<NormalBubble>().IntendMoveDir = Direction.Right;
                                        ReleaseEffect.transform.Find("Right").GetComponent<ParticleSystem>().Play();
                                    }
                                    else
                                    {
                                        ReleaseEffect.transform.Find("Right").GetComponent<ParticleSystem>().Stop();
                                    }
                                    if (CurrentNearByInfo.LeftBubble)
                                    {
                                        CurrentNearByInfo.LeftBubble.GetComponent<NormalBubble>().IntendMoveDir = Direction.Left;
                                        ReleaseEffect.transform.Find("Left").GetComponent<ParticleSystem>().Play();
                                    }
                                    else
                                    {
                                        ReleaseEffect.transform.Find("Left").GetComponent<ParticleSystem>().Stop();
                                    }
                                    if (CurrentNearByInfo.TopBubble)
                                    {
                                        CurrentNearByInfo.TopBubble.GetComponent<NormalBubble>().IntendMoveDir = Direction.Up;
                                        ReleaseEffect.transform.Find("Up").GetComponent<ParticleSystem>().Play();
                                    }
                                    else
                                    {
                                        ReleaseEffect.transform.Find("Up").GetComponent<ParticleSystem>().Stop();
                                    }
                                    if (CurrentNearByInfo.DownBubble)
                                    {
                                        CurrentNearByInfo.DownBubble.GetComponent<NormalBubble>().IntendMoveDir = Direction.Down;
                                        ReleaseEffect.transform.Find("Down").GetComponent<ParticleSystem>().Play();
                                    }
                                    else
                                    {
                                        ReleaseEffect.transform.Find("Down").GetComponent<ParticleSystem>().Stop();
                                    }
                                }
                                else
                                {
                                    ResetNearByBubble(CurrentNearByInfo);
                                }
                            }
                            return;
                        }
                    }
                }
                if (SelectedSlot != null) // Not in available slot
                {
                    SelectedSlot.GetComponent<SlotObject>().Selected = false;
                    SelectedSlot = null;
                    ResetOffsetInfo();

                    ActivatedEffect.GetComponent<ParticleSystem>().Play();
                    foreach (Transform Effect in ReleaseEffect.transform)
                    {
                        Effect.GetComponent<ParticleSystem>().Stop();
                    }

                    ResetNearByBubble(CurrentNearByInfo);
                    CurrentNearByInfo = null;
                }
            }
            else
            {
                if (SelectedSlot != null)//Release
                {
                    SelectedSlot.GetComponent<SlotObject>().Selected = false;
                    SelectedSlot = null;
                    ResetOffsetInfo();

                    foreach (Transform Effect in ReleaseEffect.transform)
                    {
                        Effect.GetComponent<ParticleSystem>().Stop();
                    }

                    ResetNearByBubble(CurrentNearByInfo);
                    CurrentNearByInfo = null;
                }
            }

        }

    }

    private void ResetOffsetInfo()
    {
        for (int i = 0; i < NearBySelectedSlots.Count; i++)
        {
            NearBySelectedSlots[i].GetComponent<SlotObject>().NearBySelected = false;
        }

        NearBySelectedSlots.Clear();
    }

    private GameObject GetNearBySlot(GameObject Slot, Vector2Int Offset)
    {
        List<List<SlotInfo>> Map = Slot.GetComponent<SlotObject>().ConnectedMap;
        Vector2Int Coordination = Slot.GetComponent<SlotObject>().ConnectedSlotInfo.Pos + Offset;
        return Map[Coordination.x][Coordination.y].Entity;
    }

    public ColorChangeTask GetDisappearTask()
    {
        Color color = GetComponent<SpriteRenderer>().color;

        return new ColorChangeTask(gameObject, Utility.ColorWithAlpha(color, 1), Utility.ColorWithAlpha(color, 0), DisappearTime, ColorChangeType.Sprite);
    }
}
