using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public enum GameState
{
    Play,
    Show,
    Pause,
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
    private GameObject CopiedLevel;

    private List<GameStatistics> LevelFinishStat=new List<GameStatistics>();
    private float Timer;

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
        State = GameState.Play;
        SortedLevelList = new List<GameObject>();
        GetLevelInfo();
        CopiedLevel = Instantiate(SortedLevelList[CurrentLevel-MinLevelIndex]);
        CopiedLevel.transform.parent = AllLevel.transform;
        CopiedLevel.SetActive(false);

        State = GameState.Play;
        EventManager.instance.Fire(new LevelLoaded(CurrentLevel));
        StartCoroutine(ScreenAppear());


        /*State = GameState.Finish;
        int num = LoadStat();
        if (num > 0)
        {
            if (num + 1 == CurrentLevel)
            {
                Destroy(SortedLevelList[num + 1 - MinLevelIndex]);
                SortedLevelList[num + 1 - MinLevelIndex] = CopiedLevel;
                CopiedLevel.SetActive(true);
                CopiedLevel = Instantiate(CopiedLevel);
                CopiedLevel.transform.parent = AllLevel.transform;
                CopiedLevel.SetActive(false);
            }
            else
            {
                Destroy(SortedLevelList[CurrentLevel - MinLevelIndex]);
                SortedLevelList[CurrentLevel - MinLevelIndex] = CopiedLevel;
                CopiedLevel = Instantiate(SortedLevelList[num + 1 - MinLevelIndex]);
                CopiedLevel.transform.parent = AllLevel.transform;
                CopiedLevel.SetActive(false);
                SortedLevelList[num + 1 - MinLevelIndex].SetActive(true);
            }

            CurrentLevel = num + 1;
            State = GameState.Play;
            EventManager.instance.Fire(new LevelLoaded(CurrentLevel));
            StartCoroutine(ScreenAppear());
        }
        else
        {
            State = GameState.Play;
            EventManager.instance.Fire(new LevelLoaded(CurrentLevel));
            StartCoroutine(ScreenAppear());
        }*/

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
        if (LoadStat() < CurrentLevel)
        {
            SaveStat(new GameStatistics(Mathf.RoundToInt(Timer), LevelManager.RemainedDisappearBubble, LevelManager.RemainedNormalBubble));
        }

        Timer = 0;
    }

    private IEnumerator QuickLoadLevel(int index)
    {

        if (index <= MaxLevelIndex)
        {
            yield return StartCoroutine(ScreenFade());

            if (index == CurrentLevel)
            {
                Destroy(SortedLevelList[index - MinLevelIndex]);
                SortedLevelList[index - MinLevelIndex] = CopiedLevel;
                CopiedLevel.SetActive(true);
                CopiedLevel = Instantiate(CopiedLevel);
                CopiedLevel.transform.parent = AllLevel.transform;
                CopiedLevel.SetActive(false);
            }
            else
            {
                Destroy(SortedLevelList[CurrentLevel - MinLevelIndex]);
                SortedLevelList[CurrentLevel - MinLevelIndex] = CopiedLevel;
                CopiedLevel = Instantiate(SortedLevelList[index - MinLevelIndex]);
                CopiedLevel.transform.parent = AllLevel.transform;
                CopiedLevel.SetActive(false);
                SortedLevelList[index - MinLevelIndex].SetActive(true);
            }

            CurrentLevel = index;
            State = GameState.Play;
            EventManager.instance.Fire(new LevelLoaded(CurrentLevel));

            yield return StartCoroutine(ScreenAppear());
        }
    }

    private IEnumerator LoadLevel(int index)
    {
        yield return new WaitForSeconds(LevelFinishWaitTime);
        if (index <= MaxLevelIndex)
        {
            yield return StartCoroutine(ScreenFade());
            if (index == CurrentLevel)
            {
                Destroy(SortedLevelList[index - MinLevelIndex]);
                SortedLevelList[index - MinLevelIndex] = CopiedLevel;
                CopiedLevel.SetActive(true);
                CopiedLevel = Instantiate(CopiedLevel);
                CopiedLevel.transform.parent = AllLevel.transform;
                CopiedLevel.SetActive(false);
            }
            else
            {
                Destroy(SortedLevelList[CurrentLevel - MinLevelIndex]);
                SortedLevelList[CurrentLevel- MinLevelIndex] = CopiedLevel;
                CopiedLevel = Instantiate(SortedLevelList[index - MinLevelIndex]);
                CopiedLevel.transform.parent = AllLevel.transform;
                CopiedLevel.SetActive(false);
                SortedLevelList[index - MinLevelIndex].SetActive(true);
            }
            
            CurrentLevel = index;
            State = GameState.Play;
            EventManager.instance.Fire(new LevelLoaded(CurrentLevel));

            yield return StartCoroutine(ScreenAppear());
        }
        else
        {
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

    private void SaveStat(GameStatistics S)
    {
        if(!Directory.Exists(Application.dataPath + "/Data"))
        {
            Directory.CreateDirectory(Application.dataPath + "/Data");
        }

        if (!File.Exists(Application.dataPath + "/Data/Data.txt"))
        {
            StreamWriter f=File.CreateText(Application.dataPath + "/Data/Data.txt");
            f.Close();
        }

        StreamWriter file = new StreamWriter(Application.dataPath + "/Data/Data.txt", true);

        file.WriteLine(CurrentLevel.ToString()+" "+ S.time.ToString() + " " + S.RemainedDisappearBubble.ToString() + " " + S.RemainedNormalBubble);
        file.Close();
    }

    private int LoadStat()
    {
        int num = 0;
        if(!File.Exists(Application.dataPath + "/Data/Data.txt"))
        {
            return num;
        }

        StreamReader file = new StreamReader(Application.dataPath + "/Data/Data.txt", true);
        while (!file.EndOfStream)
        {
            file.ReadLine();
            num++;
        }

        file.Close();
        return num;
    }
}

