using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackButton : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        switch (GameManager.gameState)
        {
            case GameState.Level:
                EventManager.instance.Fire(new CallBackToSelectLevel());
                break;
            case GameState.SelectLevelMenu:
                EventManager.instance.Fire(new CallBackToMainMenu());
                break;
        }
    }

    public ColorChangeTask GetAppearTask(float AppearTime)
    {
        Color color = GetComponent<Image>().color;

        return new ColorChangeTask(gameObject, Utility.ColorWithAlpha(color, 0), Utility.ColorWithAlpha(color, 1), AppearTime, ColorChangeType.Image);
    }

    public ColorChangeTask GetDisappearTask(float DisappearTime)
    {
        Color color = GetComponent<Image>().color;

        return new ColorChangeTask(gameObject, Utility.ColorWithAlpha(color, 1), Utility.ColorWithAlpha(color, 0), DisappearTime, ColorChangeType.Image);
    }
}
