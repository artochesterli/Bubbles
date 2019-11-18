using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MainMenuButtonType
{
    PlayButton,
    SelectLevelButton,
    SettingButton,
    CreditButton
}

public class MainMenuButton : MonoBehaviour
{
    public MainMenuButtonType Type;
    public GameObject BorderImage;
    public GameObject InsideImage;
    public GameObject SelectedEffect;

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

            switch (Type)
            {
                case MainMenuButtonType.PlayButton:
                    EventManager.instance.Fire(new CallLoadLevel(LoadLevelType.FromMainMenu, GameManager.CurrentSaveInfo.CurrentLevel));
                    break;
                case MainMenuButtonType.SelectLevelButton:
                    EventManager.instance.Fire(new CallGoToSelectLevel());
                    break;
            }

        }
    }

    public SerialTasks GetSelectedDisappearTask(float SelectedEffectScale, float SelectedEffectTime, float SelectedDisappearTime)
    {
        return Utility.GetButtonSelectedDisappearTask(BorderImage, InsideImage, SelectedEffect, true, SelectedEffectScale, SelectedEffectTime,  SelectedDisappearTime);
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
