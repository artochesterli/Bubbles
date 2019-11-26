using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public ColorChangeTask GetDisappearTask(float DisappearTime)
    {
        Color color = GetComponent<Text>().color;

        return new ColorChangeTask(gameObject, Utility.ColorWithAlpha(color, 1), Utility.ColorWithAlpha(color, 0), DisappearTime, ColorChangeType.Text);
    }


    public ColorChangeTask GetAppearTask(float AppearTime)
    {
        Color color = GetComponent<Text>().color;

        return new ColorChangeTask(gameObject, Utility.ColorWithAlpha(color, 0), Utility.ColorWithAlpha(color, 1), AppearTime, ColorChangeType.Text);
    }
}
