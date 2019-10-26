using System.Collections;
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
    Run,
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

    public GameObject Mask;
    public List<GameObject> Selectors;
    public float ScreenAppearTime;
    public float ScreenFadeTime;
    public float LevelFinishWaitTime;
    public float MenuLoadLevelWaitTime;

    public GameObject CopiedLevel;

    private GameObject AllLevel;
    private List<GameObject> SortedLevelList;
    //private GameObject CopiedLevel;

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
        StartCoroutine(ScreenAppear());

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
        levelState = LevelState.Run;
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
            yield return new WaitForSeconds(LevelFinishWaitTime);
        }
        else
        {
            yield return new WaitForSeconds(MenuLoadLevelWaitTime);
        }

        if (index <= MaxLevelIndex)
        {
            yield return StartCoroutine(ScreenFade());

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

            AssignSelectors();

            StartCoroutine(ScreenAppear());
        }
        else
        {
            yield return StartCoroutine(ScreenFade());
            CurrentSaveInfo.CurrentLevel = index;
            SaveProgress();
            SceneManager.LoadScene(NextLevelScene);
        }
    }

    private void SetSelector(GameObject obj, BubbleType Type, Color AvailableColor, Color UsedUpColor)
    {
        obj.GetComponent<BubbleSelector>().Type = Type;
        obj.GetComponent<BubbleSelector>().AvailableColor = AvailableColor;
        obj.GetComponent<BubbleSelector>().UsedUpColor = UsedUpColor;
    }

    private void AssignSelectors()
    {
        for(int i = 0; i < Selectors.Count; i++)
        {
            Selectors[i].GetComponent<BubbleSelector>().Type = BubbleType.Null;
        }

        var Data = GetComponent<ColorData>();

        if (LevelManager.RemainedDisappearBubble > 0)
        {
            SetSelector(Selectors[0], BubbleType.Disappear, Data.DisappearBubble, Data.ExhaustDisappearBubble);

            if (LevelManager.RemainedNormalBubble > 0)
            {
                SetSelector(Selectors[1], BubbleType.Normal, Data.NormalBubble, Data.ExhaustNormalBubble);
            }
        }
        else if(LevelManager.RemainedNormalBubble > 0)
        {
            SetSelector(Selectors[0], BubbleType.Normal, Data.NormalBubble, Data.ExhaustNormalBubble);
        }

        EventManager.instance.Fire(new BubbleNumSet(BubbleType.Disappear,LevelManager.RemainedDisappearBubble));
        EventManager.instance.Fire(new BubbleNumSet(BubbleType.Normal,LevelManager.RemainedNormalBubble));
        EventManager.instance.Fire(new CallActivateBubbleSelectors());

    }

    private IEnumerator ScreenFade()
    {
        float TimeCount = 0;
        while (TimeCount < ScreenFadeTime)
        {
            TimeCount += Time.deltaTime;
            Mask.GetComponent<Image>().color = Color.Lerp(new Color(0, 0, 0, 0), Color.black, TimeCount / ScreenFadeTime);
            yield return null;
        }
    }

    private IEnumerator ScreenAppear()
    {
        float TimeCount = 0;
        while (TimeCount < ScreenAppearTime)
        {
            TimeCount += Time.deltaTime;
            Mask.GetComponent<Image>().color = Color.Lerp(Color.black, new Color(0, 0, 0, 0), TimeCount / ScreenAppearTime);
            yield return null;
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
}

