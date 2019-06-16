using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Color DisappearBubbleColor;
    public Color NormalBubbleColor;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        SetPos();
        SetAppearance();
    }

    private void SetAppearance()
    {
        switch (GameManager.State)
        {
            case GameState.Play:
                switch (GameManager.HeldBubbleType)
                {
                    case BubbleType.Disappear:
                        GetComponent<SpriteRenderer>().color = DisappearBubbleColor;
                        break;
                    case BubbleType.Normal:
                        GetComponent<SpriteRenderer>().color = NormalBubbleColor;
                        break;
                    default:
                        break;
                }
                break;
            case GameState.Show:
                GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
                break;
            default:
                break;
        }
    }

    private void SetPos()
    {
        transform.position = Vector3.back * Camera.main.transform.position.z + Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
