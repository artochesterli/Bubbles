using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactButton : MonoBehaviour
{
    public GameObject Border;
    public GameObject Text;

    private bool Holding;

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
        if(GameManager.gameState == GameState.HelpText)
        {
            Application.OpenURL("https://cl4929.myportfolio.com/work");
        }
    }

    public ParallelTasks GetAppearTask(float AppearTime)
    {
        return Utility.GetButtonAppearTask(Border, Text, false, AppearTime);

    }

    public ParallelTasks GetDisappearTask(float DisappearTime)
    {
        return Utility.GetButtonUnselectedDisappearTask(Border, Text, false, DisappearTime);
    }
}
