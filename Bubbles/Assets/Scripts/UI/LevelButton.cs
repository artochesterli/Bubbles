using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public int LevelIndex;
    public Color UnfinishColor;
    public Color FinishColor;

    public float SelectInflationScale;

    public float SwtichOffset;
    public float SwtichAppearDelay;
    public float SwtichAppearTime;
    public float SwtichMoveTime;

    public int LevelGap;

    public GameObject Image;
    public GameObject Text;
    public GameObject BackUpImage;
    public GameObject BackUpText;
    public GameObject SelectedEffect;

    public bool Finished;
    private bool Swtiching;
    private float ShakeTimeCount;

    private bool LevelButtonClicked;
    // Start is called before the first frame update
    void Start()
    {
        SetText(Text, LevelIndex.ToString());

        SetColor(Image, Text, 0);

    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator Swtich(bool Bigger)
    {
        Swtiching = true;

        ResetButton();
        Vector3 TargetPos;
        Vector3 StartPos;

        if (Bigger)
        {
            LevelIndex += LevelGap;
            TargetPos = Vector3.left* SwtichOffset;
            StartPos = Vector3.right * SwtichOffset;
        }
        else
        {
            LevelIndex -= LevelGap;
            TargetPos = Vector3.right * SwtichOffset;
            StartPos = Vector3.left * SwtichOffset;
        }

        SetText(BackUpText, LevelIndex.ToString());

        float TimeCount = 0;

        float SwtichTime = SwtichAppearDelay + SwtichAppearTime;
        while (TimeCount < SwtichTime)
        {
            TimeCount += Time.deltaTime;
            if (TimeCount >= SwtichAppearDelay)
            {
                float BackAlpha = (TimeCount - SwtichAppearDelay) / (SwtichTime - SwtichAppearDelay);
                SetColor(BackUpImage, BackUpText, BackAlpha);

                BackUpImage.GetComponent<RectTransform>().localPosition = Vector3.Lerp(StartPos, Vector3.zero, BackAlpha);
                BackUpText.GetComponent<RectTransform>().localPosition = Vector3.Lerp(StartPos, Vector3.zero, BackAlpha);

            }

            float FrontAlpha = 1 - TimeCount / SwtichMoveTime;
            SetColor(Image, Text, FrontAlpha);
            //Image.GetComponent<RectTransform>().localPosition = Vector3.Lerp(Vector3.zero, TargetPos, TimeCount / SwtichMoveTime);
            //Text.GetComponent<RectTransform>().localPosition = Vector3.Lerp(Vector3.zero, TargetPos, TimeCount / SwtichMoveTime);

            yield return null;
        }

        Image.GetComponent<RectTransform>().localPosition = Vector3.zero;
        Text.GetComponent<RectTransform>().localPosition = Vector3.zero;

        SetColor(Image, Text, 1);
        SetText(Text, LevelIndex.ToString());
        SetColor(BackUpImage, BackUpText, 0);

        Swtiching = false;

    }

    public void SetColor(GameObject Image,GameObject Text,float Alpha)
    {
        if (Finished)
        {
            Image.GetComponent<Image>().color = new Color(FinishColor.r, FinishColor.g, FinishColor.b, Alpha);
            Text.GetComponent<Text>().color = new Color(FinishColor.r, FinishColor.g, FinishColor.b, Alpha);
        }
        else
        {
            Image.GetComponent<Image>().color = new Color(UnfinishColor.r, UnfinishColor.g, UnfinishColor.b, Alpha);
            Text.GetComponent<Text>().color = new Color(UnfinishColor.r, UnfinishColor.g, UnfinishColor.b, Alpha);
        }
    }

    private void SetText(GameObject Text,string s)
    {
        Text.GetComponent<Text>().text = s;
    }

    private void ResetButton()
    {
        Image.GetComponent<RectTransform>().localPosition = Vector3.zero;
        Text.GetComponent<RectTransform>().localPosition = Vector3.zero;
        ShakeTimeCount = 0;
    }

    private void OnMouseDown()
    {
        if (!Swtiching && GameManager.gameState == GameState.SelectLevelMenu)
        {
            if (GameManager.CurrentConfig.Vibration)
            {
                Taptic.Light();
            }
            GetComponent<AudioSource>().Play();

            ResetButton();
            SelectedEffect.GetComponent<Image>().enabled = true;
            EventManager.instance.Fire(new CallLoadLevel(LoadLevelType.FromSelectionMenu, LevelIndex, gameObject));
        }
    }
}
