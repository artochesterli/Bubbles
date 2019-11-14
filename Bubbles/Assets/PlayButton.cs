using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour
{
    public GameObject BorderImage;
    public GameObject InsideImage;
    public GameObject SelectedEffect;
    public float SelectedEffectTime;
    public float SelectedEffectScale;
    public float AfterSelectedEffectTime;
    public float SelectedDisappearTime;
    public float UnselectedDisappearTime;
    public float AppearTime;


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
            EventManager.instance.Fire(new CallLoadLevel(LoadLevelType.FromMainMenu, GameManager.CurrentSaveInfo.CurrentLevel, gameObject));
        }
    }

    public SerialTasks GetSelectedDisappearTask()
    {
        return Utility.GetButtonSelectedDisappearTask(BorderImage, InsideImage, SelectedEffect, true, SelectedEffectScale, SelectedEffectTime, AfterSelectedEffectTime, SelectedDisappearTime);
    }

    public ParallelTasks GetUnselectedDisappearTask()
    {
        return Utility.GetButtonUnselectedDisappearTask(BorderImage, InsideImage, true, UnselectedDisappearTime);
    }

    public ParallelTasks GetAppearTask()
    {
        return Utility.GetButtonAppearTask(BorderImage, InsideImage, true, AppearTime);
    }
}
