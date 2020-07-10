using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MainMenuButtonType
{
    PlayButton,
    SelectLevelButton,
    SettingButton,
    InfoButton
}

public class MainMenuButton : MonoBehaviour
{
    public MainMenuButtonType Type;
    public GameObject BorderImage;
    public GameObject InsideImage;
    //public GameObject SelectedEffect;

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
            if (GameManager.CurrentConfig.Vibration)
            {
                Taptic.Light();
            }
            GetComponent<AudioSource>().Play();

            switch (Type)
            {
                case MainMenuButtonType.PlayButton:
                    EventManager.instance.Fire(new CallLoadLevel(LoadLevelType.FromMainMenu, GameManager.CurrentSaveInfo.CurrentLevel));
                    break;
                case MainMenuButtonType.SelectLevelButton:
                    EventManager.instance.Fire(new CallGoToSelectLevel());
                    break;
                case MainMenuButtonType.InfoButton:
                    EventManager.instance.Fire(new CallGoToInfo());
                    break;
                case MainMenuButtonType.SettingButton:
                    EventManager.instance.Fire(new CallGoToSetting());
                    break;
            }

        }
    }
}
