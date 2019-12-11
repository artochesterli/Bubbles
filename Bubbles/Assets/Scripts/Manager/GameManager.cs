using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;

public enum GameState
{
    MainMenu,
    SelectLevelMenu,
    Level,
    HelpText
}

public enum LevelState
{
    SetUp,
    Play,
    Executing,
    RollBack,
    Clear
}

public enum CursorState
{
    Holding,
    Release
}

public enum LoadLevelType
{
    FromSelectionMenu,
    FromMainMenu,
    LevelFinish

}

public class GameStatistics
{
    public int time;
    public int RemainedDisappearBubble;
    public int RemainedNormalBubble;

    public GameStatistics(int t,int d, int n)
    {
        time = t;
        RemainedDisappearBubble = d;
        RemainedNormalBubble = n;
    }
}


[System.Serializable]
public class SaveData
{
    public int TotalLevelNumber;
    public int CurrentLevel;
    public List<bool> LevelFinished;

    public SaveData(int totalNumber,int currentLevel,List<bool> finishList)
    {
        TotalLevelNumber = totalNumber;
        CurrentLevel = currentLevel;
        LevelFinished = finishList;
    }
}

public class GameManager : MonoBehaviour
{
    public static GameState gameState;
    public static LevelState levelState;
    public static CursorState cursorState;
    public static BubbleType HeldBubbleType;
    public static SaveData CurrentSaveInfo;
    public static GameObject ActivatedLevel;

    public GameObject Title;
    public GameObject PlayButton;
    public GameObject SelectLevelButton;
    public GameObject SettingButton;
    public GameObject CreditButton;
    public GameObject BackButton;
    public GameObject HelpText;

    public float ButtonUnselectedDisappearTime;
    public float ButtonSelectedDisappearTime;
    public float ButtonAppearTime;
    public float ButtonSelectedEffectTime;
    public float ButtonSelectedEffectScale;

    public int MinLevelIndex;
    public int MaxLevelIndex;
    public int TotalLevelNumber;

    public GameObject AllUseableBubbles;
    public float UseableBubblesHeightGap;
    public GameObject UseableBubblePrefab;
    public float UseableBubblesXGap;

    public GameObject UseableAreaMarkLeft;
    public GameObject UseableAreaMarkRight;
    public GameObject LevelMarkLeft;
    public GameObject LevelMarkRight;
    public GameObject LevelNumber;
    public float LevelMarkBaseLength;
    public float LevelMarkHeight;
    public float LevelMarkLengthDeduction;

    public float MarkFillTime;
    public float LevelNumberAndUseableBubbleAppearTime;

    public float PerformLevelFinishEffectWaitTime;
    public float StartNewLevelWaitTime;

    public float LevelFinishWaitTime;
    public float MenuLoadLevelWaitTime;

    public GameObject AllLevelButtons;
    public GameObject LevelSelectionArrows;

    public float BackToMenuMapDisappearGap;
    public float BackToMenuSelectionMenuAppearGap;

    public GameObject CopiedLevel;

    public float RollBackInputInterval;

    private GameObject AllLevel;
    private List<GameObject> SortedLevelList;

    private List<GameObject> UseableBubbleList;

    private List<GameStatistics> LevelFinishStat=new List<GameStatistics>();
    private float Timer;

    private float RollBackInputIntervalTimeCount;
    private bool RollBackFirstTap;

    private const string SaveFolderName = "PlayerSave";
    private const string SaveFileName = "PlayerSave";
    private const string SaveFileExtension = ".dat";
    private const string DataFolderName = "PlayerData";
    private const string DataFileName = "PlayerData";
    private const string DataFileExtension = ".txt";

    // Start is called before the first frame update
    void Start()
    {
        //Screen.SetResolution(540, 960, false);
        Init();

        EventManager.instance.AddHandler<Place>(OnPlace);
        EventManager.instance.AddHandler<CallLoadLevel>(OnCallLoadLevel);
        EventManager.instance.AddHandler<CallGoToHelp>(OnCallGoToHelp);
        EventManager.instance.AddHandler<CallGoToSelectLevel>(OnCallGoToSelectLevel);
        EventManager.instance.AddHandler<CallBackToMainMenu>(OnCallBackToMainMenu);
        EventManager.instance.AddHandler<CallBackToSelectLevel>(OnCallBackToSelectLevel);
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveHandler<Place>(OnPlace);
        EventManager.instance.RemoveHandler<CallLoadLevel>(OnCallLoadLevel);
        EventManager.instance.RemoveHandler<CallGoToHelp>(OnCallGoToHelp);
        EventManager.instance.RemoveHandler<CallGoToSelectLevel>(OnCallGoToSelectLevel);
        EventManager.instance.RemoveHandler<CallBackToMainMenu>(OnCallBackToMainMenu);
        EventManager.instance.RemoveHandler<CallBackToSelectLevel>(OnCallBackToSelectLevel);
    }

    // Update is called once per frame
    void Update()
    {
        Timer += Time.deltaTime;


        GetRollBackInput();

    }

    private void Init()
    {
        gameState = GameState.MainMenu;
        levelState = LevelState.SetUp;
        cursorState = CursorState.Release;

        HeldBubbleType = BubbleType.Null;

        UseableBubbleList = new List<GameObject>();

        GetLevelInfo();

        CurrentSaveInfo = new SaveData(TotalLevelNumber, 1, new List<bool>());
        for (int i = 0; i < CurrentSaveInfo.TotalLevelNumber; i++)
        {
            CurrentSaveInfo.LevelFinished.Add(false);
        }
        /*if (!LoadProgress())
        {
            SaveProgress();
        }*/
    }

    private void GetLevelInfo()
    {
        SortedLevelList = new List<GameObject>();
        List<int> IndexList = new List<int>();
        AllLevel = GameObject.Find("AllLevel").gameObject;
        foreach(Transform child in AllLevel.transform)
        {
            SortedLevelList.Add(child.gameObject);
            IndexList.Add(child.GetComponent<LevelManager>().LevelIndex);
        }

        for(int i = 0; i < IndexList.Count; i++)
        {
            for(int j = 0; j < IndexList.Count - i - 1; j++)
            {
                if (IndexList[j] > IndexList[j + 1])
                {
                    int temp = IndexList[j];
                    IndexList[j] = IndexList[j + 1];
                    IndexList[j + 1] = temp;
                    GameObject g = SortedLevelList[j];
                    SortedLevelList[j] = SortedLevelList[j + 1];
                    SortedLevelList[j + 1] = g;
                }
            }
        }

        /*for(int i = 0; i < SortedLevelList.Count; i++)
        {
            if (SortedLevelList[i].activeSelf)
            {
                CurrentLevel = IndexList[i];
                return;
            }
        }

        CurrentLevel = MinLevelIndex - 1;*/
    }

    private void OnPlace(Place P)
    {
        levelState = LevelState.Executing;
    }

    private void SavePlayerData()
    {
        //SaveStat(new GameStatistics(Mathf.RoundToInt(Timer), LevelManager.RemainedDisappearBubble, LevelManager.RemainedNormalBubble));

        Timer = 0;
    }


    private IEnumerator BackToSelectionMenu()
    {
        gameState = GameState.SelectLevelMenu;

        BackButton.GetComponent<BoxCollider2D>().enabled = false;

        levelState = LevelState.Clear;

        ParallelTasks ClearLevelTasks = new ParallelTasks();

        SerialTasks LevelEndUITask = GetLevelEndUITask(true);

        ClearLevelTasks.Add(LevelEndUITask);

        SerialTasks MapDisappearTask = ActivatedLevel.GetComponent<LevelManager>().GetMapDisappearTask();

        ClearLevelTasks.Add(MapDisappearTask);

        while (!ClearLevelTasks.IsFinished)
        {
            ClearLevelTasks.Update();
            yield return null;
        }

        yield return new WaitForSeconds(BackToMenuSelectionMenuAppearGap);

        ParallelTasks LevelButtonAppearTasks = new ParallelTasks();

        foreach (Transform child in AllLevelButtons.transform)
        {
            LevelButtonAppearTasks.Add(child.GetComponent<LevelButton>().GetAppearTask(ButtonAppearTime));
        }

        foreach (Transform child in LevelSelectionArrows.transform)
        {
            LevelButtonAppearTasks.Add(child.GetComponent<LevelSelectionArrow>().GetAppearTask());
        }

        LevelButtonAppearTasks.Add(BackButton.GetComponent<BackButton>().GetAppearTask(ButtonAppearTime));

        while (!LevelButtonAppearTasks.IsFinished)
        {
            LevelButtonAppearTasks.Update();
            yield return null;
        }

        BackButton.GetComponent<BoxCollider2D>().enabled = true;


        ClearUseableBubbles();

        SortedLevelList[CurrentSaveInfo.CurrentLevel - MinLevelIndex].SetActive(false);
        Destroy(SortedLevelList[CurrentSaveInfo.CurrentLevel - MinLevelIndex]);
        SortedLevelList[CurrentSaveInfo.CurrentLevel - MinLevelIndex] = CopiedLevel;

        gameState = GameState.SelectLevelMenu;

        SetSelectLevelEnable(true);
    }

    private IEnumerator BackToMainMenu(GameState OriginalState)
    {
        gameState = GameState.MainMenu;

        BackButton.GetComponent<BoxCollider2D>().enabled = false;

        switch (OriginalState)
        {
            case GameState.SelectLevelMenu:
                SetSelectLevelEnable(false);

                ParallelTasks SelectLevelDisappearTask = new ParallelTasks();

                foreach (Transform child in AllLevelButtons.transform)
                {
                    SelectLevelDisappearTask.Add(child.GetComponent<LevelButton>().GetUnselectedDisappearTask(ButtonUnselectedDisappearTime));
                }

                foreach (Transform child in LevelSelectionArrows.transform)
                {
                    SelectLevelDisappearTask.Add(child.GetComponent<LevelSelectionArrow>().GetDisappearTask());
                }

                SelectLevelDisappearTask.Add(BackButton.GetComponent<BackButton>().GetDisappearTask(ButtonUnselectedDisappearTime,true));

                while (!SelectLevelDisappearTask.IsFinished)
                {
                    SelectLevelDisappearTask.Update();
                    yield return null;
                }
                break;
            case GameState.HelpText:
                ParallelTasks HelpDisappearTask = new ParallelTasks();

                HelpDisappearTask.Add(BackButton.GetComponent<BackButton>().GetDisappearTask(ButtonUnselectedDisappearTime,true));
                HelpDisappearTask.Add(HelpText.GetComponent<HelpText>().GetDisappearTask(ButtonUnselectedDisappearTime));

                while (!HelpDisappearTask.IsFinished)
                {
                    HelpDisappearTask.Update();
                    yield return null;
                }

                break;
        }



        ParallelTasks MainMenuAppearTask = new ParallelTasks();

        MainMenuAppearTask.Add(Title.GetComponent<Title>().GetAppearTask(ButtonAppearTime));
        MainMenuAppearTask.Add(PlayButton.GetComponent<MainMenuButton>().GetAppearTask(ButtonAppearTime));
        MainMenuAppearTask.Add(SelectLevelButton.GetComponent<MainMenuButton>().GetAppearTask(ButtonAppearTime));
        MainMenuAppearTask.Add(SettingButton.GetComponent<MainMenuButton>().GetAppearTask(ButtonAppearTime));
        MainMenuAppearTask.Add(CreditButton.GetComponent<MainMenuButton>().GetAppearTask(ButtonAppearTime));

        while (!MainMenuAppearTask.IsFinished)
        {
            MainMenuAppearTask.Update();
            yield return null;
        }

        SetMainMenuEnable(true);

    }

    private IEnumerator GoToHelp()
    {
        gameState = GameState.HelpText;

        SetMainMenuEnable(false);

        ParallelTasks MainMenuDisappearTask = new ParallelTasks();

        MainMenuDisappearTask.Add(Title.GetComponent<Title>().GetDisappearTask(ButtonUnselectedDisappearTime));
        MainMenuDisappearTask.Add(PlayButton.GetComponent<MainMenuButton>().GetUnselectedDisappearTask(ButtonUnselectedDisappearTime));
        MainMenuDisappearTask.Add(SelectLevelButton.GetComponent<MainMenuButton>().GetUnselectedDisappearTask(ButtonUnselectedDisappearTime));
        MainMenuDisappearTask.Add(SettingButton.GetComponent<MainMenuButton>().GetUnselectedDisappearTask(ButtonUnselectedDisappearTime));
        MainMenuDisappearTask.Add(CreditButton.GetComponent<MainMenuButton>().GetSelectedDisappearTask(ButtonSelectedEffectTime, ButtonSelectedDisappearTime));
        //MainMenuDisappearTask.Add(CreditButton.GetComponent<MainMenuButton>().GetSelectedDisappearTask(ButtonSelectedEffectScale, ));

        while (!MainMenuDisappearTask.IsFinished)
        {
            MainMenuDisappearTask.Update();
            yield return null;
        }

        ParallelTasks HelpAppearTask = new ParallelTasks();

        HelpAppearTask.Add(HelpText.GetComponent<HelpText>().GetAppearTask(ButtonAppearTime));
        HelpAppearTask.Add(BackButton.GetComponent<BackButton>().GetAppearTask(ButtonAppearTime));

        while (!HelpAppearTask.IsFinished)
        {
            HelpAppearTask.Update();
            yield return null;
        }

        BackButton.GetComponent<BoxCollider2D>().enabled = true;
    }

    private IEnumerator GoToSelectedLevel()
    {
        gameState = GameState.SelectLevelMenu;
        SetMainMenuEnable(false);

        ParallelTasks MainMenuDisappearTask = new ParallelTasks();

        
        MainMenuDisappearTask.Add(Title.GetComponent<Title>().GetDisappearTask(ButtonUnselectedDisappearTime));
        MainMenuDisappearTask.Add(PlayButton.GetComponent<MainMenuButton>().GetUnselectedDisappearTask(ButtonUnselectedDisappearTime));

        MainMenuDisappearTask.Add(SelectLevelButton.GetComponent<MainMenuButton>().GetSelectedDisappearTask(ButtonSelectedEffectTime, ButtonSelectedDisappearTime));
        //MainMenuDisappearTask.Add(SelectLevelButton.GetComponent<MainMenuButton>().GetSelectedDisappearTask(ButtonSelectedEffectScale,ButtonSelectedEffectTime,ButtonSelectedDisappearTime));
        MainMenuDisappearTask.Add(SettingButton.GetComponent<MainMenuButton>().GetUnselectedDisappearTask(ButtonUnselectedDisappearTime));
        MainMenuDisappearTask.Add(CreditButton.GetComponent<MainMenuButton>().GetUnselectedDisappearTask(ButtonUnselectedDisappearTime));

        while (!MainMenuDisappearTask.IsFinished)
        {
            MainMenuDisappearTask.Update();
            yield return null;
        }

        ParallelTasks SelectLevelAppearTasks = new ParallelTasks();

        foreach(Transform child in AllLevelButtons.transform)
        {
            SelectLevelAppearTasks.Add(child.GetComponent<LevelButton>().GetAppearTask(ButtonAppearTime));
        }

        foreach(Transform child in LevelSelectionArrows.transform)
        {
            SelectLevelAppearTasks.Add(child.GetComponent<LevelSelectionArrow>().GetAppearTask());
        }

        SelectLevelAppearTasks.Add(BackButton.GetComponent<BackButton>().GetAppearTask(ButtonAppearTime));

        while (!SelectLevelAppearTasks.IsFinished)
        {
            SelectLevelAppearTasks.Update();
            yield return null;
        }

        SetSelectLevelEnable(true);

        BackButton.GetComponent<BoxCollider2D>().enabled = true;
    }

    private IEnumerator LoadLevel(int index, LoadLevelType Type, GameObject LevelButton = null)
    {
        switch (Type)
        {
            case LoadLevelType.FromMainMenu:

                SetMainMenuEnable(false);

                ParallelTasks MainMenuDisappearTask = new ParallelTasks();


                MainMenuDisappearTask.Add(Title.GetComponent<Title>().GetDisappearTask(ButtonUnselectedDisappearTime));
                MainMenuDisappearTask.Add(PlayButton.GetComponent<MainMenuButton>().GetSelectedDisappearTask(ButtonSelectedEffectTime, ButtonSelectedDisappearTime));
                //MainMenuDisappearTask.Add(PlayButton.GetComponent<MainMenuButton>().GetSelectedDisappearTask(ButtonSelectedEffectScale,ButtonSelectedEffectTime,ButtonSelectedDisappearTime));
                MainMenuDisappearTask.Add(SelectLevelButton.GetComponent<MainMenuButton>().GetUnselectedDisappearTask(ButtonUnselectedDisappearTime));
                MainMenuDisappearTask.Add(SettingButton.GetComponent<MainMenuButton>().GetUnselectedDisappearTask(ButtonUnselectedDisappearTime));
                MainMenuDisappearTask.Add(CreditButton.GetComponent<MainMenuButton>().GetUnselectedDisappearTask(ButtonUnselectedDisappearTime));

                while (!MainMenuDisappearTask.IsFinished)
                {
                    MainMenuDisappearTask.Update();
                    yield return null;
                }


                break;

            case LoadLevelType.FromSelectionMenu:

                BackButton.GetComponent<BoxCollider2D>().enabled = false;
                SetSelectLevelEnable(false);

                ParallelTasks LevelButtonDisappearTasks = new ParallelTasks();

                foreach(Transform child in AllLevelButtons.transform)
                {
                    if(child.gameObject == LevelButton)
                    {
                        LevelButtonDisappearTasks.Add(child.GetComponent<LevelButton>().GetSelectedDisappearTask(ButtonSelectedEffectTime,ButtonSelectedDisappearTime));
                    }
                    else
                    {
                        LevelButtonDisappearTasks.Add(child.GetComponent<LevelButton>().GetUnselectedDisappearTask(ButtonUnselectedDisappearTime));
                    }
                }

                foreach (Transform child in LevelSelectionArrows.transform)
                {
                    LevelButtonDisappearTasks.Add(child.GetComponent<LevelSelectionArrow>().GetDisappearTask());
                }

                LevelButtonDisappearTasks.Add(BackButton.GetComponent<BackButton>().GetDisappearTask(ButtonUnselectedDisappearTime,false));

                while (!LevelButtonDisappearTasks.IsFinished)
                {
                    LevelButtonDisappearTasks.Update();
                    yield return null;
                }

                break;
            case LoadLevelType.LevelFinish:

                BackButton.GetComponent<BoxCollider2D>().enabled = false;

                levelState = LevelState.Clear;

                SerialTasks LevelEndTasks = new SerialTasks();

                LevelEndTasks.Add(GetLevelEndUITask(false));
                LevelEndTasks.Add(new WaitTask(PerformLevelFinishEffectWaitTime));
                LevelEndTasks.Add(GetBubblePowerUpTasks());
                LevelEndTasks.Add(GetShockWaveEmitAndFadeTasks());
                LevelEndTasks.Add(GetBubbleMoveOutPrepareTasks());

                while (!LevelEndTasks.IsFinished)
                {
                    LevelEndTasks.Update();
                    yield return null;
                }

                LevelEndTasks.Clear();

                LevelEndTasks.Add(GetBubbleEscapeTasks());
                LevelEndTasks.Add(new WaitTask(StartNewLevelWaitTime));

                while (!LevelEndTasks.IsFinished)
                {
                    LevelEndTasks.Update();
                    yield return null;
                }

                ClearUseableBubbles();

                break;
        }


        if (index <= MaxLevelIndex)
        {
            levelState = LevelState.SetUp;

            if (CurrentSaveInfo.CurrentLevel >= MinLevelIndex && Type == LoadLevelType.LevelFinish)
            {
                SortedLevelList[CurrentSaveInfo.CurrentLevel - MinLevelIndex].SetActive(false);
                Destroy(SortedLevelList[CurrentSaveInfo.CurrentLevel - MinLevelIndex]);
                SortedLevelList[CurrentSaveInfo.CurrentLevel - MinLevelIndex] = CopiedLevel;
            }

            CopiedLevel = Instantiate(SortedLevelList[index - MinLevelIndex]);
            CopiedLevel.transform.parent = AllLevel.transform;
            CopiedLevel.SetActive(false);
            SortedLevelList[index - MinLevelIndex].SetActive(true);
            ActivatedLevel = SortedLevelList[index - MinLevelIndex];

            if (Type == LoadLevelType.LevelFinish)
            {
                CurrentSaveInfo.LevelFinished[CurrentSaveInfo.CurrentLevel-1] = true;
            }
            CurrentSaveInfo.CurrentLevel = index;
            //SaveProgress();
            
            EventManager.instance.Fire(new LevelLoaded(CurrentSaveInfo.CurrentLevel));

            SerialTasks LevelStartUITasks = GetLevelStartTask();

            while (!LevelStartUITasks.IsFinished)
            {
                LevelStartUITasks.Update();
                yield return null;
            }

            levelState = LevelState.Play;

            EventManager.instance.Fire(new FinishLoadLevel(CurrentSaveInfo.CurrentLevel));

            ColorChangeTask BackButtonAppearTask = BackButton.GetComponent<BackButton>().GetAppearTask(ButtonAppearTime);

            while (!BackButtonAppearTask.IsFinished)
            {
                BackButtonAppearTask.Update();
                yield return null;
            }

            BackButton.GetComponent<BoxCollider2D>().enabled = true;
        }
    }

    private void SetSelectLevelEnable(bool enable)
    {
        foreach (Transform child in AllLevelButtons.transform)
        {
            child.GetComponent<CircleCollider2D>().enabled = enable;
        }

        foreach (Transform child in LevelSelectionArrows.transform)
        {
            child.GetComponent<BoxCollider2D>().enabled = enable;
        }
    }

    private void SetMainMenuEnable(bool enable)
    {
        PlayButton.GetComponent<CircleCollider2D>().enabled = enable;
        SelectLevelButton.GetComponent<CircleCollider2D>().enabled = enable;
        SettingButton.GetComponent<CircleCollider2D>().enabled = enable;
        CreditButton.GetComponent<CircleCollider2D>().enabled = enable;
    }

    private void GenerateUseableBubbles()
    {

        float height = -ActivatedLevel.GetComponent<LevelManager>().GetMapSize().y * 0.5f - UseableBubblesHeightGap;

        AllUseableBubbles.transform.position = new Vector2(AllUseableBubbles.transform.position.x, height);

        int BubbleNumber = LevelManager.RemainedDisappearBubble + LevelManager.RemainedNormalBubble;

        float StartX;

        if(BubbleNumber % 2 == 1)
        {
            StartX = -UseableBubblesXGap * (BubbleNumber / 2);
        }
        else
        {
            StartX = -UseableBubblesXGap * (BubbleNumber / 2) + 0.5f * UseableBubblesXGap;
        }

        var ColorData = GetComponent<ColorData>();

        for(int i = 0; i < LevelManager.RemainedDisappearBubble; i++)
        {
            GameObject Bubble = GameObject.Instantiate(UseableBubblePrefab);
            Bubble.transform.parent = AllUseableBubbles.transform;
            Bubble.transform.localPosition = new Vector3(StartX + i * UseableBubblesXGap, 0, 0);
            Bubble.GetComponent<UsableCircle>().Type = BubbleType.Disappear;
            Bubble.GetComponent<SpriteRenderer>().color = Utility.ColorWithAlpha(ColorData.DisappearBubble,0);

            UseableBubbleList.Add(Bubble);
        }
        
        for (int i = 0; i < LevelManager.RemainedNormalBubble; i++)
        {
            GameObject Bubble = GameObject.Instantiate(UseableBubblePrefab);
            Bubble.transform.parent = AllUseableBubbles.transform;
            Bubble.transform.localPosition = new Vector3(StartX + (i+LevelManager.RemainedDisappearBubble) * UseableBubblesXGap, 0, 0);
            Bubble.GetComponent<UsableCircle>().Type = BubbleType.Normal;
            Bubble.GetComponent<SpriteRenderer>().color = Utility.ColorWithAlpha(ColorData.NormalBubble,0);

            UseableBubbleList.Add(Bubble);
        }
    }
    
    private void SaveProgress()
    {
        string Dic = Path.Combine(Application.dataPath, SaveFolderName);
         
        if (!Directory.Exists(Dic))
        {
            Directory.CreateDirectory(Dic);
        }

        string file = Path.Combine(Application.dataPath, SaveFolderName, SaveFileName + SaveFileExtension);

        FileStream stream;

        if (!File.Exists(file))
        {
            stream = File.Open(file, FileMode.Create);
        }
        else
        {
            stream = File.Open(file, FileMode.Open);
        }

        BinaryFormatter binaryFormatter = new BinaryFormatter();
        binaryFormatter.Serialize(stream, CurrentSaveInfo);
        stream.Close();
    }

    private bool LoadProgress()
    {
        string file = Path.Combine(Application.dataPath, SaveFolderName, SaveFileName + SaveFileExtension);
        if (!File.Exists(file))
        {
            CurrentSaveInfo = new SaveData(TotalLevelNumber, 0, new List<bool>());
            for(int i = 0; i < CurrentSaveInfo.TotalLevelNumber; i++)
            {
                CurrentSaveInfo.LevelFinished.Add(false);
            }
            return false;
        }
        FileStream stream = File.Open(file, FileMode.Open);
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        CurrentSaveInfo = (SaveData)binaryFormatter.Deserialize(stream);
        stream.Close();
        return true;
    }

    private void GetRollBackInput()
    {
        if (gameState == GameState.Level && levelState == LevelState.Play && cursorState == CursorState.Release)
        {
            if (RollBackFirstTap)
            {
                RollBackInputIntervalTimeCount += Time.deltaTime;
                if (RollBackInputIntervalTimeCount >= RollBackInputInterval)
                {
                    RollBackFirstTap = false;
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (!RollBackFirstTap)
                {
                    RollBackInputIntervalTimeCount = 0;
                    RollBackFirstTap = true;
                }
                else
                {
                    EventManager.instance.Fire(new RollBack());
                }
            }
        }
        else
        {
            RollBackFirstTap = false;
            RollBackInputIntervalTimeCount = 0;
        }
    }

    private void OnCallLoadLevel(CallLoadLevel Call)
    {
        gameState = GameState.Level;

        switch (Call.Type)
        {
            case LoadLevelType.FromSelectionMenu:
                StartCoroutine(LoadLevel(Call.index, Call.Type, Call.SelectedLevelButton));
                break;
            case LoadLevelType.FromMainMenu:
                StartCoroutine(LoadLevel(Call.index, Call.Type));
                break;
            case LoadLevelType.LevelFinish:
                StartCoroutine(LoadLevel(Call.index, Call.Type));
                break;
        }

    }

    private void OnCallGoToHelp(CallGoToHelp e)
    {
        StartCoroutine(GoToHelp());
    }

    private void OnCallGoToSelectLevel(CallGoToSelectLevel e)
    {
        StartCoroutine(GoToSelectedLevel());
    }

    private void OnCallBackToSelectLevel(CallBackToSelectLevel e)
    {
        StartCoroutine(BackToSelectionMenu());
    }

    private void OnCallBackToMainMenu(CallBackToMainMenu e)
    {
        StartCoroutine(BackToMainMenu(gameState));
    }

    private SerialTasks GetLevelStartTask()
    {
        LevelNumber.GetComponent<Text>().text = CurrentSaveInfo.CurrentLevel.ToString();
        LevelMarkLeft.GetComponent<RectTransform>().sizeDelta = new Vector2(LevelMarkBaseLength - LevelMarkLengthDeduction * (CurrentSaveInfo.CurrentLevel / 10), LevelMarkHeight);
        LevelMarkRight.GetComponent<RectTransform>().sizeDelta = new Vector2(LevelMarkBaseLength - LevelMarkLengthDeduction * (CurrentSaveInfo.CurrentLevel / 10), LevelMarkHeight);
        GenerateUseableBubbles();

        SerialTasks LevelStartUITasks = new SerialTasks();

        ParallelTasks MarkAppearTask = new ParallelTasks();
        ParallelTasks UseableBubbleAndLevelNumebrAppearTask = new ParallelTasks();

        MarkAppearTask.Add(new UIFillTask(LevelMarkLeft, 0, 1, MarkFillTime));
        MarkAppearTask.Add(new UIFillTask(LevelMarkRight, 0, 1, MarkFillTime));
        MarkAppearTask.Add(new UIFillTask(UseableAreaMarkLeft, 0, 1, MarkFillTime));
        MarkAppearTask.Add(new UIFillTask(UseableAreaMarkRight, 0, 1, MarkFillTime));

        Color levelnumbercolor = LevelNumber.GetComponent<Text>().color;
        

        UseableBubbleAndLevelNumebrAppearTask.Add(new ColorChangeTask(LevelNumber, Utility.ColorWithAlpha(levelnumbercolor, 0), Utility.ColorWithAlpha(levelnumbercolor,1), LevelNumberAndUseableBubbleAppearTime,ColorChangeType.Text));
        for(int i = 0; i < UseableBubbleList.Count; i++)
        {
            Color color = UseableBubbleList[i].GetComponent<SpriteRenderer>().color;
            UseableBubbleAndLevelNumebrAppearTask.Add(new ColorChangeTask(UseableBubbleList[i], Utility.ColorWithAlpha(color, 0), Utility.ColorWithAlpha(color, 1), LevelNumberAndUseableBubbleAppearTime,ColorChangeType.Sprite));
        }

        UseableBubbleAndLevelNumebrAppearTask.Add(ActivatedLevel.GetComponent<LevelManager>().GetMapAppearTask());

        LevelStartUITasks.Add(MarkAppearTask);
        LevelStartUITasks.Add(UseableBubbleAndLevelNumebrAppearTask);

        return LevelStartUITasks;
    }

    private SerialTasks GetLevelEndUITask(bool ClickBackButton)
    {
        SerialTasks LevelEndUITasks = new SerialTasks();

        ParallelTasks AllTask = new ParallelTasks();
        Color levelnumbercolor = LevelNumber.GetComponent<Text>().color;

        AllTask.Add(BackButton.GetComponent<BackButton>().GetDisappearTask(ButtonUnselectedDisappearTime, ClickBackButton));

        AllTask.Add(new ColorChangeTask(LevelNumber, Utility.ColorWithAlpha(levelnumbercolor, 1), Utility.ColorWithAlpha(levelnumbercolor, 0), LevelNumberAndUseableBubbleAppearTime,ColorChangeType.Text));

        SerialTasks MarkDisappear = new SerialTasks();

        MarkDisappear.Add(new WaitTask(LevelNumberAndUseableBubbleAppearTime - MarkFillTime));

        ParallelTasks MarkDisappearTask = new ParallelTasks();

        MarkDisappearTask.Add(new UIFillTask(LevelMarkLeft, 1, 0, MarkFillTime));
        MarkDisappearTask.Add(new UIFillTask(LevelMarkRight, 1, 0, MarkFillTime));
        MarkDisappearTask.Add(new UIFillTask(UseableAreaMarkLeft, 1, 0, MarkFillTime));
        MarkDisappearTask.Add(new UIFillTask(UseableAreaMarkRight, 1, 0, MarkFillTime));

        MarkDisappear.Add(MarkDisappearTask);

        AllTask.Add(MarkDisappear);

        ParallelTasks UseableCircleDisappearTasks = new ParallelTasks();

        foreach(Transform child in AllUseableBubbles.transform)
        {
            if (child.gameObject.activeSelf)
            {
                UseableCircleDisappearTasks.Add(child.GetComponent<UsableCircle>().GetDisappearTask());
            }
        }

        AllTask.Add(UseableCircleDisappearTasks);

        LevelEndUITasks.Add(AllTask);

        return LevelEndUITasks;
    }

    private void ClearUseableBubbles()
    {
        for (int i = 0; i < UseableBubbleList.Count; i++)
        {
            Destroy(UseableBubbleList[i]);
        }

        UseableBubbleList.Clear();
    }

    private ParallelTasks GetBubblePowerUpTasks()
    {
        ParallelTasks AllPowerUp = new ParallelTasks();
        GameObject AllBubbles = ActivatedLevel.GetComponent<LevelManager>().AllBubble;

        foreach(Transform child in AllBubbles.transform)
        {
            AllPowerUp.Add(child.GetComponent<NormalBubble>().GetShockWavePowerUpTask());
        }

        return AllPowerUp;
    }

    private ParallelTasks GetShockWaveEmitAndFadeTasks()
    {
        ParallelTasks AllEmitAndFade = new ParallelTasks();
        GameObject AllBubbles = ActivatedLevel.GetComponent<LevelManager>().AllBubble;
        GameObject AllSlots = ActivatedLevel.GetComponent<LevelManager>().AllSlot;

        foreach (Transform child in AllBubbles.transform)
        {
            AllEmitAndFade.Add(child.GetComponent<NormalBubble>().GetShockWaveEmitTask());
        }

        foreach(Transform child in AllSlots.transform)
        {
            AllEmitAndFade.Add(child.GetComponent<SlotObject>().GetFadeTask());
        }

        return AllEmitAndFade;
    }

    private ParallelTasks GetBubbleMoveOutPrepareTasks()
    {
        ParallelTasks AllPrepare = new ParallelTasks();

        GameObject AllBubbles = ActivatedLevel.GetComponent<LevelManager>().AllBubble;

        foreach (Transform child in AllBubbles.transform)
        {
            AllPrepare.Add(child.GetComponent<NormalBubble>().GetMoveOutPrepareTask());
        }

        return AllPrepare;
    }

    private ParallelTasks GetBubbleEscapeTasks()
    {
        ParallelTasks AllEscape = new ParallelTasks();

        GameObject AllBubbles = ActivatedLevel.GetComponent<LevelManager>().AllBubble;

        foreach (Transform child in AllBubbles.transform)
        {
            AllEscape.Add(child.GetComponent<NormalBubble>().GetMoveOutEscapeTask());
        }

        return AllEscape;
    }
}

