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
        if (GameManager.levelState == LevelState.Play || GameManager.levelState == LevelState.Executing)
        {
            CheckSelected();
            SetTransform();
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

            Vector3 WorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            WorldPos.z = 0;
            transform.position = WorldPos + Vector3.up * CurrentOffset * CurrentSize;

        }
        /*else
        {
            CurrentOffset -= OffsetChangeSpeed * Time.deltaTime;
            CurrentSize -= SizeChangeSpeed * Time.deltaTime;
            if(CurrentSize < DefaultSize)
            {
                CurrentSize = DefaultSize;
            }

            if(CurrentOffset < 0)
            {
                CurrentOffset = 0;
            }
        }*/

        transform.localScale = Vector3.one * CurrentSize;
    }

    private void SetColor()
    {
        ColorRecoverTimeCount += Time.deltaTime;
        GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, Mathf.Lerp(0, 1, ColorRecoverTimeCount / ColorRecoverTime));
    }

    private void CheckSelected()
    {
        if(!Selected && CursorManager.CurrentState == CursorState.Release && Input.GetMouseButtonDown(0) && CursorInside())
        {
            GameManager.HeldBubbleType = Type;
            CursorManager.CurrentState = CursorState.Holding;
            Selected = true;
            ActivatedEffect.GetComponent<ParticleSystem>().Play();
        }
        else if (Selected && Input.GetMouseButtonUp(0))
        {
            CursorManager.CurrentState = CursorState.Release;
            Selected = false;

            if (SelectedSlot != null)
            {
                ResetOffsetInfo();
                SelectedSlot.GetComponent<SlotObject>().Selected = false;
                EventManager.instance.Fire(new Place(gameObject, SelectedSlot.GetComponent<SlotObject>().ConnectedSlotInfo.Pos, GameManager.HeldBubbleType));
                transform.position = OriPos;
                transform.localScale = Vector3.one * DefaultSize;
                CurrentOffset = 0;
                CurrentSize = DefaultSize;

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
        Vector3 WorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return WorldPos.x > OriPos.x - DefaultSize / 2 && WorldPos.x < OriPos.x + DefaultSize / 2
            && WorldPos.y > OriPos.y - DefaultSize / 2 && WorldPos.y < OriPos.y + DefaultSize / 2;

    }

    public void SetColor(Color c)
    {
        color = c;
    }

    private void CheckSlotSelection()
    {
        if (GameManager.gameState == GameState.Level)
        {
            if (GameManager.levelState == LevelState.Play)
            {
                if (Selected)
                {
                    foreach (Transform child in AllSlot.transform)
                    {
                        List<GameObject> NearByCircleList = new List<GameObject>();
                        for (int i = 0; i < 4; i++)
                        {
                            NearByCircleList.Add(null);
                        }
                        if (child.GetComponent<SlotObject>().Inside(transform.position) && child.GetComponent<SlotObject>().AvailablePos(NearByCircleList))
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
