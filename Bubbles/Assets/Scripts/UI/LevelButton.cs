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
    public float FinishWaveInitScale;
    public float FinishWaveScale;
    public float FinishWaveInterval;

    public float SwtichOffset;
    public float SwtichAppearDelay;
    public float SwtichAppearTime;
    public float SwtichMoveTime;

    public int LevelGap;

    public float SelectedEffectScale;
    public float SelectedEffectTime;
    public float AfterSelectedEffectTime;
    public float UnselectedDisappearTime;
    public float SelectedDisappearTime;
    public float AppearDelay;
    public float AppearTime;

    public GameObject Image;
    public GameObject Text;
    public GameObject BackUpImage;
    public GameObject BackUpText;
    public GameObject FinishWave;
    public GameObject SelectedEffect;

    public bool Finished;
    private bool Swtiching;
    private float ShakeTimeCount;
    private float WaveTimeCount;

    private bool LevelButtonClicked;
    // Start is called before the first frame update
    void Start()
    {
        SetText(Text, LevelIndex.ToString());

    }

    private void OnDestroy()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.gameState == GameState.Menu)
        {
            SetColor(Image, Text, 1);
            Shake();
            //MakeWave();
        }
        else
        {
            ShakeTimeCount = 0;
        }
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
        FinishWave.GetComponent<Image>().color = new Color(FinishColor.r, FinishColor.g, FinishColor.b, 0);
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
            FinishWave.GetComponent<RectTransform>().localScale = Vector3.Lerp(Vector3.one* FinishWaveInitScale, Vector3.one * FinishWaveScale, (WaveTimeCount-FinishWaveInterval) / FinishWaveTime);

            float Alpha;
            if (WaveTimeCount - FinishWaveInterval< FinishWaveTime/2)
            {
                Alpha = (WaveTimeCount - FinishWaveInterval) / (FinishWaveTime / 2);

            }
            else
            {
                Alpha = 1- (WaveTimeCount - FinishWaveInterval-FinishWaveTime/2) / (FinishWaveTime / 2);

            }

            FinishWave.GetComponent<Image>().color = Color.Lerp(new Color(FinishColor.r, FinishColor.g, FinishColor.b, 0), new Color(FinishColor.r, FinishColor.g, FinishColor.b, 1), Alpha);
            if (WaveTimeCount < FinishWaveInterval)
            {
                FinishWave.GetComponent<Image>().color = new Color(FinishColor.r, FinishColor.g, FinishColor.b, 0);
            }
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

    private void OnMouseDown()
    {
        if (!Swtiching && GameManager.gameState == GameState.Menu)
        {
            ResetButton();
            SelectedEffect.GetComponent<Image>().enabled = true;
            EventManager.instance.Fire(new CallLoadLevel(LoadLevelType.FromSelectionMenu, LevelIndex, gameObject));
        }
    }

    private IEnumerator Appear()
    {
        yield return new WaitForSeconds(AppearDelay);

        float TImeCount = 0;
        while (TImeCount < AppearTime)
        {
            TImeCount += Time.deltaTime;
            SetColor(Image, Text, TImeCount / AppearTime);
            yield return null;
        }

        LevelButtonClicked = false;
    }

    private IEnumerator UnselectedDisappeear()
    {
        ResetButton();
        float TimeCount = 0;
        while (TimeCount < UnselectedDisappearTime)
        {
            TimeCount += Time.deltaTime;
            SetColor(Image, Text, 1 - TimeCount / UnselectedDisappearTime);
            yield return null;
        }
    }

    private IEnumerator SelectedDisappear()
    {
        ResetButton();
        float TimeCount = 0;
        SelectedEffect.GetComponent<Image>().enabled = true;
        Color color = SelectedEffect.GetComponent<Image>().color;

        while (TimeCount < SelectedEffectTime)
        {
            TimeCount += Time.deltaTime;
            SelectedEffect.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * SelectedEffectScale, TimeCount / SelectedEffectTime);
            SelectedEffect.GetComponent<Image>().color = Color.Lerp(new Color(color.r, color.g, color.b, 1), new Color(color.r, color.g, color.b, 0), TimeCount / SelectedEffectTime);
            yield return null;
        }

        yield return new WaitForSeconds(AfterSelectedEffectTime);

        TimeCount = 0;

        while (TimeCount < SelectedDisappearTime)
        {
            TimeCount += Time.deltaTime;
            SetColor(Image, Text, 1 - TimeCount / SelectedDisappearTime);
            yield return null;
        }
    }

    private void OnBackToMenu(BackToMenu Back)
    {
        StartCoroutine(Appear());
    }

    public ParallelTasks GetUnselectedDisappearTask()
    {
        ParallelTasks UnselectedDisappearTask = new ParallelTasks();

        Color ImageColor = Image.GetComponent<Image>().color;
        Color TextColor = Text.GetComponent<Text>().color;

        UnselectedDisappearTask.Add(new ColorChangeTask(Image, Utility.ColorWithAlpha(ImageColor, 1), Utility.ColorWithAlpha(ImageColor, 0), UnselectedDisappearTime, ColorChangeType.Image));

        UnselectedDisappearTask.Add(new ColorChangeTask(Text, Utility.ColorWithAlpha(TextColor, 1), Utility.ColorWithAlpha(TextColor, 0), UnselectedDisappearTime, ColorChangeType.Text));

        return UnselectedDisappearTask;
    }

    public SerialTasks GetSelectedDisappearTask()
    {
        SerialTasks SelectedDisappearTask = new SerialTasks();

        ParallelTasks SelectedEffectTask = new ParallelTasks();

        SelectedEffectTask.Add(new ScaleChangeTask(SelectedEffect, 1, SelectedEffectScale, SelectedEffectTime));

        Color EffectColor = SelectedEffect.GetComponent<Image>().color;
        SelectedEffectTask.Add(new ColorChangeTask(SelectedEffect, Utility.ColorWithAlpha(EffectColor, 1), Utility.ColorWithAlpha(EffectColor, 0), SelectedEffectTime, ColorChangeType.Image));

        SelectedDisappearTask.Add(SelectedEffectTask);
        SelectedDisappearTask.Add(new WaitTask(AfterSelectedEffectTime));

        ParallelTasks SelfDisappearTask = new ParallelTasks();

        Color ImageColor = Image.GetComponent<Image>().color;
        Color TextColor = Text.GetComponent<Text>().color;

        SelfDisappearTask.Add(new ColorChangeTask(Image, Utility.ColorWithAlpha(ImageColor, 1), Utility.ColorWithAlpha(ImageColor, 0), SelectedDisappearTime, ColorChangeType.Image));

        SelfDisappearTask.Add(new ColorChangeTask(Text, Utility.ColorWithAlpha(TextColor, 1), Utility.ColorWithAlpha(TextColor, 0), SelectedDisappearTime, ColorChangeType.Text));

        SelectedDisappearTask.Add(SelfDisappearTask);

        return SelectedDisappearTask;
    }

    public ParallelTasks GetAppearTask()
    {
        ParallelTasks AppearTask = new ParallelTasks();

        Color ImageColor = Image.GetComponent<Image>().color;
        Color TextColor = Text.GetComponent<Text>().color;

        AppearTask.Add(new ColorChangeTask(Image, Utility.ColorWithAlpha(ImageColor, 0), Utility.ColorWithAlpha(ImageColor, 1), AppearTime, ColorChangeType.Image));

        AppearTask.Add(new ColorChangeTask(Text, Utility.ColorWithAlpha(TextColor, 0), Utility.ColorWithAlpha(TextColor, 1), AppearTime, ColorChangeType.Text));

        return AppearTask;
    }
}
