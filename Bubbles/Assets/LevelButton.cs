using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    public int LevelIndex;
    public Color UnfinishColor;
    public Color FinishColor;
    public float FinishShakeAmplitude;
    public float FinishShakeCycle;
    public float FinishWaveTime;
    public float FinishWaveScale;
    public float FinishWaveInterval;

    public float SwtichOffset;
    public float SwtichAppearDelay;
    public float SwtichAppearTime;
    public float SwtichMoveTime;

    public int LevelGap;

    public GameObject Image;
    public GameObject Text;
    public GameObject BackUpImage;
    public GameObject BackUpText;
    public GameObject FinishWave;

    private bool Finished;
    private bool Swtiching;
    private float ShakeTimeCount;
    private float WaveTimeCount;
    // Start is called before the first frame update
    void Start()
    {
        Finished = true;
        SetText(Text, LevelIndex.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            StartCoroutine(Swtich(true));
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            StartCoroutine(Swtich(false));
        }

        Shake();
        MakeWave();
    }

    private IEnumerator Swtich(bool Bigger)
    {
        Swtiching = true;

        ResetButton();
        Vector3 TargetPos;
        if (Bigger)
        {
            LevelIndex += LevelGap;
            TargetPos = Vector3.right * SwtichOffset;
        }
        else
        {
            LevelIndex -= LevelGap;
            TargetPos = Vector3.left * SwtichOffset;
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
            }

            float FrontAlpha = 1 - TimeCount / SwtichMoveTime;
            SetColor(Image, Text, FrontAlpha);
            Image.GetComponent<RectTransform>().localPosition = Vector3.Lerp(Vector3.zero, TargetPos, TimeCount / SwtichMoveTime);
            Text.GetComponent<RectTransform>().localPosition = Vector3.Lerp(Vector3.zero, TargetPos, TimeCount / SwtichMoveTime);

            yield return null;
        }

        Image.GetComponent<RectTransform>().localPosition = Vector3.zero;
        Text.GetComponent<RectTransform>().localPosition = Vector3.zero;

        SetColor(Image, Text, 1);
        SetText(Text, LevelIndex.ToString());
        SetColor(BackUpImage, BackUpText, 0);

        Swtiching = false;

    }

    private void SetColor(GameObject Image,GameObject Text,float Alpha)
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
        WaveTimeCount = 0;
    }

    private void Shake()
    {
        if (Finished && !Swtiching)
        {
            ShakeTimeCount += Time.deltaTime;
            float Offset = Mathf.Sin(ShakeTimeCount / FinishShakeCycle * 2 * Mathf.PI) * FinishShakeAmplitude;
            Image.GetComponent<RectTransform>().localPosition = Vector3.up * Offset;
            Text.GetComponent<RectTransform>().localPosition = Vector3.up * Offset;
            FinishWave.GetComponent<RectTransform>().localPosition = Vector3.up * Offset;
        }
    }

    private void MakeWave()
    {
        if (Finished && !Swtiching)
        {
            FinishWave.GetComponent<Image>().enabled = true;
            WaveTimeCount += Time.deltaTime;
            FinishWave.GetComponent<RectTransform>().localScale = Vector3.Lerp(Vector3.one, Vector3.one * FinishWaveScale, WaveTimeCount / FinishWaveTime);
            FinishWave.GetComponent<Image>().color = Color.Lerp(new Color(FinishColor.r, FinishColor.g, FinishColor.b, 1), new Color(FinishColor.r, FinishColor.g, FinishColor.b, 0), WaveTimeCount / FinishWaveTime);
            if (WaveTimeCount >= FinishWaveInterval + FinishWaveTime)
            {
                WaveTimeCount = 0;
            }
            
        }
        else
        {
            FinishWave.GetComponent<Image>().enabled = false;
        }
    }
}
