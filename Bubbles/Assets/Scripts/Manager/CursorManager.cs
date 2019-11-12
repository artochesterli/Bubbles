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

        OffsetCircles = new List<GameObject>();
        NearBySelectedSlots = new List<GameObject>();
    }

    private void OnDestroy()
    {
        AllSlot = null;
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



    private void SetPos()
    {

        GetComponent<RectTransform>().position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        GetComponent<RectTransform>().position -= GetComponent<RectTransform>().position.z * Vector3.forward;
    }



}
