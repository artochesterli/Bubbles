using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MainMenuButtonType
{
    PlayButton,
    SelectLevelButton,
    SettingButton,
    HelpButton
}

public class MainMenuButton : MonoBehaviour
{
    public MainMenuButtonType Type;
    public GameObject BorderImage;
    public GameObject InsideImage;
    public GameObject SelectedEffect;

    public float InflationScale;

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
        if (GameManager.gameState == GameState.MainMenu)
        {
            Taptic.Light();
            GetComponent<AudioSource>().Play();

            switch (Type)
            {
                case MainMenuButtonType.PlayButton:
                    EventManager.instance.Fire(new CallLoadLevel(LoadLevelType.FromMainMenu, GameManager.CurrentSaveInfo.CurrentLevel));
                    break;
                case MainMenuButtonType.SelectLevelButton:
                    EventManager.instance.Fire(new CallGoToSelectLevel());
                    break;
                case MainMenuButtonType.HelpButton:
                    EventManager.instance.Fire(new CallGoToHelp());
                    break;
                case MainMenuButtonType.SettingButton:
                    EventManager.instance.Fire(new CallGoToSetting());
                    break;
            }

        }
    }

    public SerialTasks GetSelectedDisappearTask(float InflationTime, float DeflationTime)
    {
        return Utility.GetButtonSelectedDisappearTask(BorderImage, InsideImage, 1, InflationScale, InflationTime, DeflationTime, true);
    }

    public ParallelTasks GetUnselectedDisappearTask(float UnselectedDisappearTime)
    {
        return Utility.GetButtonUnselectedDisappearTask(BorderImage, InsideImage, true, UnselectedDisappearTime);
    }

    public ParallelTasks GetAppearTask(float AppearTime)
    {
        return Utility.GetButtonAppearTask(BorderImage, InsideImage, true, AppearTime);
    }
}
