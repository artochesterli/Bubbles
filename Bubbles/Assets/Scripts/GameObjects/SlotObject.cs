using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotObject : MonoBehaviour
{
    public SlotInfo ConnectedSlotInfo;
    public bool Selected;

    public Color SelectedColor;
    public Color DefaultColor;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.instance.AddHandler<Place>(OnPlace);
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveHandler<Place>(OnPlace);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseOver()
    {
        if (ConnectedSlotInfo.InsideBubbleType==BubbleType.Null && GameManager.State==GameState.Play)
        {
            Selected = true;
            GetComponent<SpriteRenderer>().color = SelectedColor;
        }
    }

    private void OnMouseExit()
    {
        Selected = false;
        GetComponent<SpriteRenderer>().color = DefaultColor;
    }

    private void OnMouseDown()
    {
        if (Selected)
        {
            EventManager.instance.Fire(new Place(ConnectedSlotInfo.Pos));
        }
    }

    private void OnPlace(Place P)
    {
        Selected = false;
        GetComponent<SpriteRenderer>().color = DefaultColor;
    }
}
