﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;

public enum GameState
{
    Menu,
    Level
}

public enum LevelState
{
    SetUp,
    Play,
    Executing,
    Clear
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
    public static BubbleType HeldBubbleType;
    public static SaveData CurrentSaveInfo;
    public static GameObject ActivatedLevel;
    public static GameObject Instance;

    public int MinLevelIndex;
    public int MaxLevelIndex;
    public int TotalLevelNumber;
    public string NextLevelScene;

    public GameObject AllUseableBubbles;
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

    public GameObject Mask;
    public List<GameObject> Selectors;

    public float PerformLevelFinishEffectWaitTime;
    public float StartNewLevelWaitTime;

    public float LevelFinishWaitTime;
    public float MenuLoadLevelWaitTime;

    public GameObject CopiedLevel;

    private GameObject AllLevel;
    private List<GameObject> SortedLevelList;

    private List<GameObject> UseableBubbleList;

    private List<GameStatistics> LevelFinishStat=new List<GameStatistics>();
    private float Timer;

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
        EventManager.instance.AddHandler<LevelFinish>(OnLevelFinish);
        EventManager.instance.AddHandler<CallLoadLevel>(OnCallLoadLevel);
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveHandler<Place>(OnPlace);
        EventManager.instance.RemoveHandler<LevelFinish>(OnLevelFinish);
        EventManager.instance.RemoveHandler<CallLoadLevel>(OnCallLoadLevel);
    }

    // Update is called once per frame
    void Update()
    {
        Timer += Time.deltaTime;

        if (Input.GetKey(KeyCode.Escape))
        {
            gameState = GameState.Menu;
            StopCurrentLevel();
            EventManager.instance.Fire(new BackToMenu());
        }
    }

    private void Init()
    {
        Instance = gameObject;

        HeldBubbleType = BubbleType.Null;

        UseableBubbleList = new List<GameObject>();


        GetLevelInfo();

        //SortedLevelList[CurrentLevel - MinLevelIndex].SetActive(false);

        if (!LoadProgress())
        {
            SaveProgress();
        }

        /*if (!LoadProgress())
        {
            //CurrentLevel = MinLevelIndex;
            SaveProgress();
        }*/

        //CopiedLevel = Instantiate(SortedLevelList[CurrentSaveInfo.CurrentLevel - MinLevelIndex]);
        //CopiedLevel.transform.parent = AllLevel.transform;
        //CopiedLevel.SetActive(false);
       // SortedLevelList[CurrentSaveInfo.CurrentLevel - MinLevelIndex].SetActive(true);

        //State = GameState.Play;
        //EventManager.instance.Fire(new LevelLoaded(CurrentSaveInfo.CurrentLevel));

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

    private void OnLevelFinish(LevelFinish L)
    {
        levelState = LevelState.Clear;
        StartCoroutine(LoadLevel(L.Index + 1,false));

        SavePlayerData();
    }

    private void SavePlayerData()
    {
        //SaveStat(new GameStatistics(Mathf.RoundToInt(Timer), LevelManager.RemainedDisappearBubble, LevelManager.RemainedNormalBubble));

        Timer = 0;
    }

    private IEnumerator LoadLevel(int index,bool FromMenu)
    {
        if (!FromMenu)
        {
            SerialTasks LevelEndUITasks = GetLevelEndTask();

            while (!LevelEndUITasks.IsFinished)
            {
                LevelEndUITasks.Update();
                yield return null;
            }

            yield return new WaitForSeconds(PerformLevelFinishEffectWaitTime);

            ParallelTasks BubblePowerUp = GetBubblePowerUpTasks();

            while (!BubblePowerUp.IsFinished)
            {
                BubblePowerUp.Update();
                yield return null;
            }

            ParallelTasks ShockWaveEmitAndFade = GetShockWaveEmitAndFadeTasks();

            while (!ShockWaveEmitAndFade.IsFinished)
            {
                ShockWaveEmitAndFade.Update();
                yield return null;
            }

            ParallelTasks MoveOutPrepare = GetBubbleMoveOutPrepareTasks();

            while (!MoveOutPrepare.IsFinished)
            {
                MoveOutPrepare.Update();
                yield return null;
            }

            ParallelTasks MoveOutEscape = GetBubbleEscapeTasks();

            while (!MoveOutEscape.IsFinished)
            {
                MoveOutEscape.Update();
                yield return null;
            }

            yield return new WaitForSeconds(StartNewLevelWaitTime);
        }
        else
        {
            yield return new WaitForSeconds(MenuLoadLevelWaitTime);
        }

        if (index <= MaxLevelIndex)
        {
            levelState = LevelState.SetUp;

            if (CurrentSaveInfo.CurrentLevel >= MinLevelIndex && !FromMenu)
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

            if (!FromMenu)
            {
                CurrentSaveInfo.LevelFinished[CurrentSaveInfo.CurrentLevel-1] = true;
            }
            CurrentSaveInfo.CurrentLevel = index;
            SaveProgress();
            
            EventManager.instance.Fire(new LevelLoaded(CurrentSaveInfo.CurrentLevel));

            SerialTasks LevelStartUITasks = GetLevelStartTask();

            while (!LevelStartUITasks.IsFinished)
            {
                LevelStartUITasks.Update();
                yield return null;
            }

            levelState = LevelState.Play;
        }
    }

    private void SetSelector(GameObject obj, BubbleType Type, Color AvailableColor, Color UsedUpColor)
    {
        obj.GetComponent<BubbleSelector>().Type = Type;
        obj.GetComponent<BubbleSelector>().AvailableColor = AvailableColor;
        obj.GetComponent<BubbleSelector>().UsedUpColor = UsedUpColor;
    }

    private void GenerateUseableBubbles()
    {
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
        //CurrentLevel = (int)binaryFormatter.Deserialize(stream);
        stream.Close();
        return true;
    }

    /*private void SaveStat(GameStatistics S)
    {
        string Dic = Path.Combine(Application.dataPath, DataFolderName);

        if (!Directory.Exists(Dic))
        {
            Directory.CreateDirectory(Dic);
        }

        string file = Path.Combine(Application.dataPath, DataFolderName, DataFileName + DataFileExtension);

        if (!File.Exists(file))
        {
            StreamWriter f=File.CreateText(file);
            f.Close();
        }

        StreamWriter stream = new StreamWriter(file, true);

        stream.WriteLine(CurrentSaveInfo.CurrentLevel.ToString()+" "+ S.time.ToString() + " " + S.RemainedDisappearBubble.ToString() + " " + S.RemainedNormalBubble);
        stream.Close();
    }

    private int LoadStat()
    {
        int num = 0;

        string file = Path.Combine(Application.dataPath, DataFolderName, DataFileName + DataFileExtension);

        if (!File.Exists(file))
        {
            return num;
        }

        StreamReader stream = new StreamReader(file, true);
        while (!stream.EndOfStream)
        {
            stream.ReadLine();
            num++;
        }

        stream.Close();
        return num;
    }*/

    private void OnCallLoadLevel(CallLoadLevel Call)
    {
        gameState = GameState.Level;
        StartCoroutine(LoadLevel(Call.index,true));
    }

    private void StopCurrentLevel()
    {
        Destroy(SortedLevelList[CurrentSaveInfo.CurrentLevel - MinLevelIndex]);
        SortedLevelList[CurrentSaveInfo.CurrentLevel - MinLevelIndex] = CopiedLevel;
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
        

        UseableBubbleAndLevelNumebrAppearTask.Add(new UITextColorChangeTask(LevelNumber, Utility.ColorWithAlpha(levelnumbercolor, 0), Utility.ColorWithAlpha(levelnumbercolor,1), LevelNumberAndUseableBubbleAppearTime));
        for(int i = 0; i < UseableBubbleList.Count; i++)
        {
            Color color = UseableBubbleList[i].GetComponent<SpriteRenderer>().color;
            UseableBubbleAndLevelNumebrAppearTask.Add(new ColorChangeTask(UseableBubbleList[i], Utility.ColorWithAlpha(color, 0), Utility.ColorWithAlpha(color, 1), LevelNumberAndUseableBubbleAppearTime));
        }

        UseableBubbleAndLevelNumebrAppearTask.Add(ActivatedLevel.GetComponent<LevelManager>().GetMapAppearTask());

        LevelStartUITasks.Add(MarkAppearTask);
        LevelStartUITasks.Add(UseableBubbleAndLevelNumebrAppearTask);

        return LevelStartUITasks;
    }

    private SerialTasks GetLevelEndTask()
    {
        for(int i = 0; i < UseableBubbleList.Count; i++)
        {
            Destroy(UseableBubbleList[i]);
        }

        UseableBubbleList.Clear();

        SerialTasks LevelEndUITasks = new SerialTasks();

        ParallelTasks AllTask = new ParallelTasks();
        Color levelnumbercolor = LevelNumber.GetComponent<Text>().color;
        AllTask.Add(new UITextColorChangeTask(LevelNumber, Utility.ColorWithAlpha(levelnumbercolor, 1), Utility.ColorWithAlpha(levelnumbercolor, 0), LevelNumberAndUseableBubbleAppearTime));

        SerialTasks MarkDisappear = new SerialTasks();

        MarkDisappear.Add(new WaitTask(LevelNumberAndUseableBubbleAppearTime - MarkFillTime));

        ParallelTasks MarkDisappearTask = new ParallelTasks();

        MarkDisappearTask.Add(new UIFillTask(LevelMarkLeft, 1, 0, MarkFillTime));
        MarkDisappearTask.Add(new UIFillTask(LevelMarkRight, 1, 0, MarkFillTime));
        MarkDisappearTask.Add(new UIFillTask(UseableAreaMarkLeft, 1, 0, MarkFillTime));
        MarkDisappearTask.Add(new UIFillTask(UseableAreaMarkRight, 1, 0, MarkFillTime));

        MarkDisappear.Add(MarkDisappearTask);

        AllTask.Add(MarkDisappear);

        LevelEndUITasks.Add(AllTask);

        return LevelEndUITasks;
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

