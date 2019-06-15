using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBubble : MonoBehaviour
{
    public Color DefaultColor;
    public Color InfectedColor;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SetAppearance();
    }

    private void SetAppearance()
    {
        switch (GetComponent<Bubble>().State)
        {
            case BubbleState.Default:
                GetComponent<SpriteRenderer>().color = DefaultColor;
                break;
            case BubbleState.Blocked:
                GetComponent<SpriteRenderer>().color = InfectedColor;
                break;
            case BubbleState.Moving:
                GetComponent<SpriteRenderer>().color = InfectedColor;
                break;
            case BubbleState.Inflated:
                GetComponent<SpriteRenderer>().color = InfectedColor;
                break;
            default:
                break;
        }
    }
}
