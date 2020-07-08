using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoText : MonoBehaviour
{
    public string DragText;
    public string GoalTextBegin;
    public string GoalTextColored;
    public string GoalTextEnd;
    public string TeleportTextColored;
    public string TeleportText;
    public string RollBackText;

    public Color GoalTextColor;
    public Color TeleportTextColor;

    // Start is called before the first frame update
    void Start()
    {
        //GetComponent<Text>().text = text;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public ColorChangeTask GetAppearTask(float AppearTime)
    {
        Color color = GetComponent<Text>().color;

        return new ColorChangeTask(gameObject, Utility.ColorWithAlpha(color, 0), Utility.ColorWithAlpha(color, 1), AppearTime, ColorChangeType.Text);
    }

    public ColorChangeTask GetDisappearTask(float DisappearTime)
    {
        Color color = GetComponent<Text>().color;

        return new ColorChangeTask(gameObject, Utility.ColorWithAlpha(color, 1), Utility.ColorWithAlpha(color, 0), DisappearTime, ColorChangeType.Text);
    }
}
