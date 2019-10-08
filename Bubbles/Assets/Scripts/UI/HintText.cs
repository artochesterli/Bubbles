using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintText : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Hide();
    }

    // Update is called once per frame
    void Update()
    {
        if (CheckCircleRemained())
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    private bool CheckCircleRemained()
    {
        return LevelManager.RemainedDisappearBubble > 0 || LevelManager.RemainedNormalBubble > 0 || GameManager.levelState!=LevelState.Play;
    }

    private void Show()
    {
        GetComponent<Text>().color = Color.white;
    }
    
    private void Hide()
    {
        GetComponent<Text>().color = new Color(1, 1, 1, 0);
    }
}
