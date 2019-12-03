﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InLevelHelpText : MonoBehaviour
{
    public string DragHint = "Drag the circles to the grid";
    public string RollBackHint = "Double tap to go back if needed";
    public string ExhaustHint = "Exhausted circles won't be pushed";
    public float ShowHideTime;

    private bool InTutorial;
    private string CurrentText;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.instance.AddHandler<FinishLoadLevel>(OnFinishLoadLevel);
        EventManager.instance.AddHandler<Place>(OnPlace);
        EventManager.instance.AddHandler<MotionFinish>(OnMotionFinish);
        EventManager.instance.AddHandler<CallBackToSelectLevel>(OnCallBackToSelectLevel);
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveHandler<FinishLoadLevel>(OnFinishLoadLevel);
        EventManager.instance.RemoveHandler<Place>(OnPlace);
        EventManager.instance.RemoveHandler<MotionFinish>(OnMotionFinish);
        EventManager.instance.RemoveHandler<CallBackToSelectLevel>(OnCallBackToSelectLevel);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCallBackToSelectLevel(CallBackToSelectLevel e)
    {
        if (InTutorial)
        {
            InTutorial = false;
            StartCoroutine(HideText());
        }
    }

    private void OnFinishLoadLevel(FinishLoadLevel e)
    {
        if (e.index == 1)
        {
            InTutorial = true;
            CurrentText = DragHint;
            GetComponent<Text>().text = CurrentText;
            StartCoroutine(ShowText());
        }
        else if(e.index == 11)
        {
            InTutorial = true;
            CurrentText = ExhaustHint;
            GetComponent<Text>().text = CurrentText;
            StartCoroutine(ShowText());
        }
    }

    private void OnPlace(Place e)
    {
        if (InTutorial)
        {
            if(CurrentText == DragHint)
            {
                StartCoroutine(HideText());
                InTutorial = false;
            }
        }
    }

    private void OnMotionFinish(MotionFinish e)
    {
        if (InTutorial)
        {
            CurrentText = RollBackHint;
            GetComponent<Text>().text = CurrentText;
            StartCoroutine(ShowText());
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
