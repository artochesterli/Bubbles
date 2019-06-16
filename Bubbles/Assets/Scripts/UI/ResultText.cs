using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultText : MonoBehaviour
{
    public string SuccessText;
    public string FailText;
    public float ShowTime;


    // Start is called before the first frame update
    void Start()
    {
        EventManager.instance.AddHandler<LevelFinish>(OnLevelFinish);
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveHandler<LevelFinish>(OnLevelFinish);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnLevelFinish(LevelFinish L)
    {
        if (L.Success)
        {
            StartCoroutine(ShowText(SuccessText));
        }
        else
        {
            StartCoroutine(ShowText(FailText));
        }
    }

    private IEnumerator ShowText(string s)
    {
        GetComponent<Text>().text = s;
        GetComponent<Text>().color = new Color(1, 1, 1, 0);
        float TimeCount = 0;
        while (TimeCount < ShowTime)
        {
            TimeCount += Time.deltaTime;
            GetComponent<Text>().color = Color.Lerp(new Color(1, 1, 1, 0), Color.white, TimeCount / ShowTime);
            yield return null;
        }
    }
}
