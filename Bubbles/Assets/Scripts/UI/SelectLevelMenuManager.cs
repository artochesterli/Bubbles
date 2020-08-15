using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectLevelMenuManager : MonoBehaviour
{
    public int GameMinLevelIndex;
    public int GameMaxLevelIndex;
    public int LevelButtonNumber;
    public GameObject AllLevelButtons;
    public GameObject RightArrow;
    public GameObject LeftArrow;

    private int CurrentMinLevel;

    // Start is called before the first frame update
    void Start()
    {
        CurrentMinLevel = 1;
    }

    // Update is called once per frame
    void Update()
    {
        SetButtonFinish();
    }

    private void SetButtonFinish()
    {
        int index = 0;
        foreach (Transform child in AllLevelButtons.transform)
        {
            if (GameManager.CurrentSaveInfo.LevelFinished[CurrentMinLevel+index - 1])
            {
                child.GetComponent<LevelButton>().Finished = true;
            }
            else
            {
                child.GetComponent<LevelButton>().Finished = false;
            }
            index++;
        }
    }

    //public void SetLevelIndex()
    //{
    //    int CurrentLevel = GameManager.CurrentSaveInfo.CurrentLevel;

    //    CurrentMinLevel = CurrentLevel / LevelButtonNumber * LevelButtonNumber + 1;


    //    for(int i = 0; i < AllLevelButtons.transform.childCount; i++)
    //    {
    //        child.GetComponent<LevelButton>().LevelIndex =
    //    }

    //    foreach(Transform child in AllLevelButtons.transform)
    //    {
            
    //    }
    //}

    public void IncreaseLevel()
    {
        if(CurrentMinLevel + 2*LevelButtonNumber - 1 <= GameMaxLevelIndex)
        {
            CurrentMinLevel += LevelButtonNumber;
            foreach (Transform child in AllLevelButtons.transform)
            {
                StartCoroutine(child.GetComponent<LevelButton>().Swtich(true));
            }
        }
    }

    public void DecreaseLevel()
    {
        if(CurrentMinLevel-LevelButtonNumber >= GameMinLevelIndex)
        {
            CurrentMinLevel -= LevelButtonNumber;
            foreach (Transform child in AllLevelButtons.transform)
            {
                StartCoroutine(child.GetComponent<LevelButton>().Swtich(false));
            }
        }
    }




}
