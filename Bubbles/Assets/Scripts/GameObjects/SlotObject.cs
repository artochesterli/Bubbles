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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseEnter()
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
}
