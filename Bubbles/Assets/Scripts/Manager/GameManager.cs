using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;

public enum GameState
{
    Play,
    Show,
    Init,
    Finish
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

public class GameManager : MonoBehaviour
{
    public static GameState State;
    public static BubbleType HeldBubbleType = BubbleType.Disappear;
    public static int CurrentLevel;

    public int MinLevelIndex;
    public int MaxLevelIndex;
    public string NextLevelScene;

    public GameObject Mask;
    public float ScreenAppearTime;
    public float ScreenFadeTime;
    public float LevelFinishWaitTime;

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
        Init();

        EventManager.instance.AddHandler<Place>(OnPlace);
        EventManager.instance.AddHandler<LevelFinish>(OnLevelFinish);
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveHandler<Place>(OnPlace);
        EventManager.instance.RemoveHandler<LevelFinish>(OnLevelFinish);
    }

    // Update is called once per frame
    void Update()
    {
        Timer += Time.deltaTime;
    }

    private void Init()
    {
        SortedLevelList = new List<GameObject>();
        GetLevelInfo();

        SortedLevelList[CurrentLevel - MinLevelIndex].SetActive(false);

        /*if (!LoadProgress())
        {
            //CurrentLevel = MinLevelIndex;
            SaveProgress();
        }*/

        SortedLevelList[CurrentLevel - MinLevelIndex].SetActive(true);

        /*CopiedLevel = Instantiate(SortedLevelList[CurrentLevel-MinLevelIndex]);
        CopiedLevel.transform.parent = AllLevel.transform;
        CopiedLevel.SetActive(false);*/

        //State = GameState.Play;
        EventManager.instance.Fire(new LevelLoaded(CurrentLevel));
        StartCoroutine(ScreenAppear());

    }

    private void GetLevelInfo()
    {
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

        for(int i = 0; i < SortedLevelList.Count; i++)
        {
            if (SortedLevelList[i].activeSelf)
            {
                CurrentLevel = IndexList[i];
            }
        }

    }

    private void OnPlace(Place P)
    {
        State = GameState.Show;
    }

    private void OnLevelFinish(LevelFinish L)
    {
        State = GameState.Finish;
        StartCoroutine(LoadLevel(L.Index + 1));


        SavePlayerData();
    }

    private void SavePlayerData()
    {
        SaveStat(new GameStatistics(Mathf.RoundToInt(Timer), LevelManager.RemainedDisappearBubble, LevelManager.RemainedNormalBubble));

        Timer = 0;
    }

    private IEnumerator LoadLevel(int index)
    {
        yield return new WaitForSeconds(LevelFinishWaitTime);
        if (index <= MaxLevelIndex)
        {
            yield return StartCoroutine(ScreenFade());

            SortedLevelList[CurrentLevel - MinLevelIndex].SetActive(false);
            //Destroy(SortedLevelList[CurrentLevel - MinLevelIndex]);
            /*SortedLevelList[CurrentLevel - MinLevelIndex] = CopiedLevel;
            CopiedLevel = Instantiate(SortedLevelList[index - MinLevelIndex]);
            CopiedLevel.transform.parent = AllLevel.transform;
            CopiedLevel.SetActive(false);*/
            SortedLevelList[index - MinLevelIndex].SetActive(true);
            
            CurrentLevel = index;
            SaveProgress();
            //State = GameState.Play;
            EventManager.instance.Fire(new LevelLoaded(CurrentLevel));

            StartCoroutine(ScreenAppear());
        }
        else
        {
            yield return StartCoroutine(ScreenFade());
            CurrentLevel = index;
            SaveProgress();
            SceneManager.LoadScene(NextLevelScene);
        }
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
        binaryFormatter.Serialize(stream, CurrentLevel);
        stream.Close();
    }

    private bool LoadProgress()
    {
        string file = Path.Combine(Application.dataPath, SaveFolderName, SaveFileName + SaveFileExtension);
        if (!File.Exists(file))
        {
            return false;
        }
        FileStream stream = File.Open(file, FileMode.Open);
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        CurrentLevel = (int)binaryFormatter.Deserialize(stream);
        stream.Close();
        return true;
    }

    private void SaveStat(GameStatistics S)
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

        stream.WriteLine(CurrentLevel.ToString()+" "+ S.time.ToString() + " " + S.RemainedDisappearBubble.ToString() + " " + S.RemainedNormalBubble);
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
    }
}

