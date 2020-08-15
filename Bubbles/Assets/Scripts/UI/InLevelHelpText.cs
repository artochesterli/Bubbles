using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum LevelTiming
{
    Enter,
    Place,
    MotionFinish,
    Leave
}

public class TutorialText
{
    public string Text;
    public int Level;
    public LevelTiming AppearTiming;
    public LevelTiming DisappearTiming;

    public TutorialText(string s,int level, LevelTiming appear,LevelTiming disappear)
    {
        Text = s;
        Level = level;
        AppearTiming = appear;
        DisappearTiming = disappear;
    }
}

public class InLevelHelpText : MonoBehaviour
{
    public float ShowHideTime;

    private bool InTutorial;
    private string CurrentText;

    private List<TutorialText> TutorialTextList;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.instance.AddHandler<FinishLoadLevel>(OnFinishLoadLevel);
        EventManager.instance.AddHandler<Place>(OnPlace);
        EventManager.instance.AddHandler<MotionFinish>(OnMotionFinish);
        EventManager.instance.AddHandler<CallBackToSelectLevel>(OnCallBackToSelectLevel);
        EventManager.instance.AddHandler<CallLoadLevel>(OnLevelFinish);

        TutorialTextList = new List<TutorialText>();

        TutorialTextList.Add(new TutorialText("Drag the white orbs to the grid", 1, LevelTiming.Enter, LevelTiming.Place));
        TutorialTextList.Add(new TutorialText("Double tap to undo if need", 1, LevelTiming.MotionFinish, LevelTiming.Leave));
        TutorialTextList.Add(new TutorialText("Double tap to undo if need", 2, LevelTiming.MotionFinish, LevelTiming.Leave));
        TutorialTextList.Add(new TutorialText("Orbs with energy will push nearby orbs", 4, LevelTiming.Enter, LevelTiming.Leave));
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveHandler<FinishLoadLevel>(OnFinishLoadLevel);
        EventManager.instance.RemoveHandler<Place>(OnPlace);
        EventManager.instance.RemoveHandler<MotionFinish>(OnMotionFinish);
        EventManager.instance.RemoveHandler<CallBackToSelectLevel>(OnCallBackToSelectLevel);
        EventManager.instance.RemoveHandler<CallLoadLevel>(OnLevelFinish);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnLevelFinish(CallLoadLevel e)
    {
        if(e.Type == LoadLevelType.LevelFinish)
        {
            for (int i = 0; i < TutorialTextList.Count; i++)
            {
                if (GameManager.ActivatedLevel.GetComponent<LevelManager>().LevelIndex == TutorialTextList[i].Level && TutorialTextList[i].Text == CurrentText && TutorialTextList[i].DisappearTiming == LevelTiming.Leave)
                {
                    CurrentText = "";
                    StartCoroutine(HideText());
                    break;
                }
            }
        }
    }

    private void OnCallBackToSelectLevel(CallBackToSelectLevel e)
    {
        for(int i = 0; i < TutorialTextList.Count; i++)
        {
            if(GameManager.ActivatedLevel.GetComponent<LevelManager>().LevelIndex == TutorialTextList[i].Level && TutorialTextList[i].Text == CurrentText)
            {
                CurrentText = "";
                StartCoroutine(HideText());
                break;
            }
        }
    }

    private void OnFinishLoadLevel(FinishLoadLevel e)
    {
        for (int i = 0; i < TutorialTextList.Count; i++)
        {
            if (GameManager.ActivatedLevel.GetComponent<LevelManager>().LevelIndex == TutorialTextList[i].Level && TutorialTextList[i].Text != CurrentText && TutorialTextList[i].AppearTiming == LevelTiming.Enter)
            {
                CurrentText = TutorialTextList[i].Text;
                GetComponent<Text>().text = CurrentText;
                StartCoroutine(ShowText());
                break;
            }
        }
    }

    private void OnPlace(Place e)
    {
        for (int i = 0; i < TutorialTextList.Count; i++)
        {
            if (GameManager.ActivatedLevel.GetComponent<LevelManager>().LevelIndex == TutorialTextList[i].Level && TutorialTextList[i].Text == CurrentText && TutorialTextList[i].DisappearTiming == LevelTiming.Place)
            {
                CurrentText = "";
                StartCoroutine(HideText());
                break;
            }
        }
    }

    private void OnMotionFinish(MotionFinish e)
    {
        for (int i = 0; i < TutorialTextList.Count; i++)
        {
            if (GameManager.ActivatedLevel.GetComponent<LevelManager>().LevelIndex == TutorialTextList[i].Level && TutorialTextList[i].Text != CurrentText && TutorialTextList[i].AppearTiming == LevelTiming.MotionFinish)
            {
                CurrentText = TutorialTextList[i].Text;
                GetComponent<Text>().text = CurrentText;
                StartCoroutine(ShowText());
                break;
            }
        }
    }

    private IEnumerator ShowText()
    {
        float TimeCount = 0;

        Color color = GetComponent<Text>().color;

        while(TimeCount < ShowHideTime)
        {
            TimeCount += Time.deltaTime;
            GetComponent<Text>().color = Color.Lerp(Utility.ColorWithAlpha(color, 0), Utility.ColorWithAlpha(color, 1), TimeCount / ShowHideTime);
            yield return null;
        }
    }

    private IEnumerator HideText()
    {
        float TimeCount = 0;

        Color color = GetComponent<Text>().color;

        while (TimeCount < ShowHideTime)
        {
            TimeCount += Time.deltaTime;
            GetComponent<Text>().color = Color.Lerp(Utility.ColorWithAlpha(color, 1), Utility.ColorWithAlpha(color, 0), TimeCount / ShowHideTime);
            yield return null;
        }
    }
}
